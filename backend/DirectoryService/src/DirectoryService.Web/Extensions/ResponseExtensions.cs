using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Web.Extensions;

internal static class ResponseExtensions
{
	public static IResult ToResponse(this Error error)
	{
		return TypedResults.Json(error, statusCode: error.Type.GetStatusCodeByErrorType());
	}

	private static int GetStatusCodeByErrorType(this ErrorType errorType)
	{
		return errorType switch
		{
			ErrorType.Validation => StatusCodes.Status400BadRequest,
			ErrorType.NotFound => StatusCodes.Status404NotFound,
			ErrorType.Failure => StatusCodes.Status500InternalServerError,
			ErrorType.Conflict => StatusCodes.Status409Conflict,
			_ => StatusCodes.Status500InternalServerError
		};
	}
}