using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.SharedKernel.Exceptions;

public class ValidationException : Exception
{
	public ValidationException(Error error)
		: base(error.GetMessage())
	{
		Error = error;
	}

	public ValidationException()
	{
	}

	public ValidationException(string message)
		: base(message)
	{
	}

	public ValidationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public Error Error { get; } = null!;
}