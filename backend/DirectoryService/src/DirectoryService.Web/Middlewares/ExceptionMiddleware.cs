using DirectoryService.SharedKernel.Envelopes;
using DirectoryService.SharedKernel.Errors;
using DirectoryService.SharedKernel.Exceptions;

namespace DirectoryService.Web.Middlewares;

internal sealed partial class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await next(context);
		}
#pragma warning disable CA1031
		catch (Exception ex)
#pragma warning restore CA1031
		{
			await HandleExceptionAsync(context, ex);
		}
	}

	private async Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		var (statusCode, error, isTechnicalFailure) = exception switch
		{
			NotFoundException ex => (StatusCodes.Status404NotFound, ex.Error, false),
			ValidationException ex => (StatusCodes.Status400BadRequest, ex.Error, false),
			ConflictException ex => (StatusCodes.Status409Conflict, ex.Error, false),
			FailureException ex => (StatusCodes.Status500InternalServerError, ex.Error, true),
			BadHttpRequestException => (StatusCodes.Status400BadRequest,
				Error.Validation("request.invalid", "Некорректный HTTP-запрос"), false),
			_ => (StatusCodes.Status500InternalServerError,
				Error.Failure("server.internal", "Внутренняя ошибка сервера"), true)
		};

		if (isTechnicalFailure)
		{
			LogUnhandledException(exception);
		}

		var envelope = Envelope.Fail(error);
		context.Response.ContentType = "application/json";
		context.Response.StatusCode = statusCode;

		await context.Response.WriteAsJsonAsync(envelope);
	}

	[LoggerMessage(LogLevel.Error, "Техническая ошибка при обработке HTTP-запроса")]
	partial void LogUnhandledException(Exception exception);
}
