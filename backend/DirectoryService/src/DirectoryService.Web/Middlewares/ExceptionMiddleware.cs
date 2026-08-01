

using DirectoryService.SharedKernel.Errors;
using DirectoryService.SharedKernel.Exceptions;

namespace DirectoryService.Web.Middlewares;

internal sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
#pragma warning disable CA1848
		logger.LogError(exception, "Exception was thrown in education service");
#pragma warning restore CA1848

		var (statusCode, error) = exception switch
		{
			NotFoundException ex => (StatusCodes.Status404NotFound, ex.Error),

			ValidationException ex => (StatusCodes.Status400BadRequest, ex.Error),

			ConflictException ex => (StatusCodes.Status409Conflict, ex.Error),

			FailureException ex => (StatusCodes.Status500InternalServerError, ex.Error),

			BadHttpRequestException => (StatusCodes.Status400BadRequest,
				Error.Validation("request.invalid", exception.Message)),

			_ => (StatusCodes.Status500InternalServerError, Error.Failure("server.internal", exception.Message)),
		};
		context.Response.ContentType = "application/json";
		context.Response.StatusCode = statusCode;

		await context.Response.WriteAsJsonAsync(error);
	}
}