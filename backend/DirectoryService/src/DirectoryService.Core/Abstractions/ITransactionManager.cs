using CSharpFunctionalExtensions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Abstractions;

public interface ITransactionManager
{
	Task<UnitResult<Error>> BeginTransactionAsync(CancellationToken cancellationToken = default);

	Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken = default);

	Task<UnitResult<Error>> CommitTransactionAsync(CancellationToken cancellationToken = default);

	Task<UnitResult<Error>> RollbackTransactionAsync(CancellationToken cancellationToken = default);

	Error? TryMapDatabaseException(Exception exception);
}
