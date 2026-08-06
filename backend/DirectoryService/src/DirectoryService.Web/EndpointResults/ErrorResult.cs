using DirectoryService.SharedKernel.Envelopes;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Web.EndpointResults;

#pragma warning disable CA1515
public sealed class ErrorResult : IResult
#pragma warning restore CA1515
{
	private readonly Error _error;

	public ErrorResult(Error error)
	{
		_error = error;
	}

	public Task ExecuteAsync(HttpContext httpContext)
	{
		ArgumentNullException.ThrowIfNull(httpContext);

		int statusCode = GetStatusCodeFromErrorType(_error.Type);

		var envelope = Envelope.Fail(_error);
		httpContext.Response.StatusCode = statusCode;

		return httpContext.Response.WriteAsJsonAsync(envelope);
	}

	private static int GetStatusCodeFromErrorType(ErrorType errorType) =>
		errorType switch
		{
			ErrorType.Validation => StatusCodes.Status400BadRequest,
			ErrorType.NotFound => StatusCodes.Status404NotFound,
			ErrorType.Conflict => StatusCodes.Status409Conflict,
			ErrorType.Failure => StatusCodes.Status500InternalServerError,
			_ => StatusCodes.Status500InternalServerError
		};
}