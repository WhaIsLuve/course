using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.SharedKernel.Exceptions;

public class ConflictException : Exception
{
	public ConflictException(Error error)
		: base(error.GetMessage())
	{
		Error = error;
	}

	public ConflictException()
	{
	}

	public ConflictException(string message)
		: base(message)
	{
	}

	public ConflictException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public Error Error { get; } = null!;
}