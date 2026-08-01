using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.SharedKernel.Exceptions;

public class FailureException : Exception
{
	public FailureException(Error error)
		: base(error.GetMessage())
	{
		Error = error;
	}

	public FailureException()
	{
	}

	public FailureException(string message)
		: base(message)
	{
	}

	public FailureException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public Error Error { get; } = null!;
}