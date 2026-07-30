using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.SharedKernel.Exceptions;

public class NotFoundException : Exception
{
	public NotFoundException(Error error)
		: base(error.GetMessage())
	{
		Error = error;
	}

	public NotFoundException()
	{
	}

	public NotFoundException(string message)
		: base(message)
	{
	}

	public NotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public Error Error { get; } = null!;
}