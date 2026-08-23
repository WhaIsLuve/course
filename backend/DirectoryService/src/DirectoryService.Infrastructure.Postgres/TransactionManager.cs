using System.Data.Common;
using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DirectoryService.Infrastructure.Postgres;

internal sealed partial class TransactionManager(
	AppDbContext dbContext,
	ILogger<TransactionManager> logger)
	: ITransactionManager
{
	private readonly AppDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
#pragma warning disable CA1823
	private readonly ILogger<TransactionManager> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
#pragma warning restore CA1823
	private IDbContextTransaction? _transaction;

	public async Task<UnitResult<Error>> BeginTransactionAsync(CancellationToken cancellationToken = default)
	{
		if (_transaction is not null)
			return Error.Failure("database.transaction.state", "Не удалось начать операцию сохранения данных");

		try
		{
			_transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
			return UnitResult.Success<Error>();
		}
		catch (Exception exception) when (exception is not OperationCanceledException)
		{
			return MapAndLog(exception);
		}
	}

	public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			await _dbContext.SaveChangesAsync(cancellationToken);
			return UnitResult.Success<Error>();
		}
		catch (Exception exception) when (exception is not OperationCanceledException)
		{
			return MapAndLog(exception);
		}
	}

	public async Task<UnitResult<Error>> CommitTransactionAsync(CancellationToken cancellationToken = default)
	{
		if (_transaction is null)
			return Error.Failure("database.transaction.state", "Не удалось завершить операцию сохранения данных");

		try
		{
			await _transaction.CommitAsync(cancellationToken);
			return UnitResult.Success<Error>();
		}
		catch (Exception exception) when (exception is not OperationCanceledException)
		{
			return MapAndLog(exception);
		}
		finally
		{
			await DisposeTransactionAsync();
		}
	}

	public async Task<UnitResult<Error>> RollbackTransactionAsync(CancellationToken cancellationToken = default)
	{
		if (_transaction is null)
			return UnitResult.Success<Error>();

		try
		{
			await _transaction.RollbackAsync(cancellationToken);
			return UnitResult.Success<Error>();
		}
		catch (Exception exception) when (exception is not OperationCanceledException)
		{
			return MapAndLog(exception);
		}
		finally
		{
			await DisposeTransactionAsync();
		}
	}

	public Error? TryMapDatabaseException(Exception exception)
	{
		ArgumentNullException.ThrowIfNull(exception);

		var error = MapDatabaseException(exception);
		if (error is not null)
			LogDatabaseException(exception, error);

		return error;
	}

	private static Error? MapDatabaseException(Exception exception)
	{
		if (exception is DbUpdateConcurrencyException)
			return Error.Conflict("database.concurrency.conflict", "Данные были изменены параллельно. Повторите операцию.");

		var postgresException = FindInnerException<PostgresException>(exception);
		if (postgresException is not null)
		{
			return postgresException.SqlState switch
			{
				PostgresErrorCodes.UniqueViolation => Error.Conflict(
					"database.unique.conflict", "Запись с такими данными уже существует."),
				PostgresErrorCodes.ForeignKeyViolation => Error.Conflict(
					"database.foreign-key.conflict", "Связанная запись была изменена. Повторите операцию."),
				_ => Error.Failure("database.failure", "Произошла ошибка базы данных.")
			};
		}

		return exception is DbUpdateException or DbException
			? Error.Failure("database.failure", "Произошла ошибка базы данных.")
			: null;
	}

	private UnitResult<Error> MapAndLog(Exception exception)
	{
		var error = TryMapDatabaseException(exception);
		if (error is not null)
			return error;

		error = Error.Failure("database.failure", "Произошла ошибка базы данных.");
		LogDatabaseException(exception, error);
		return error;
	}

	private void LogDatabaseException(Exception exception, Error error)
	{
		var postgresException = FindInnerException<PostgresException>(exception);
		LogDatabaseError(
			exception,
			error.Messages.Count == 0 ? null : error.Messages[0].Code,
			postgresException?.SqlState,
			postgresException?.ConstraintName);
	}

	[LoggerMessage(
		Level = LogLevel.Error,
		Message = "Ошибка операции базы данных. ErrorCode: {ErrorCode}, SqlState: {SqlState}, Constraint: {ConstraintName}")]
	partial void LogDatabaseError(
		Exception exception,
		string? errorCode,
		string? sqlState,
		string? constraintName);

	private async ValueTask DisposeTransactionAsync()
	{
		if (_transaction is null)
			return;

		await _transaction.DisposeAsync();
		_transaction = null;
	}

	private static TException? FindInnerException<TException>(Exception exception)
		where TException : Exception
	{
		for (Exception? current = exception; current is not null; current = current.InnerException)
		{
			if (current is TException matched)
				return matched;
		}

		return null;
	}
}
