using System.Text.Json.Serialization;

namespace DirectoryService.SharedKernel.Errors;

#pragma warning disable CA1716
public record Error
#pragma warning restore CA1716
{
	[JsonConstructor]
	private Error(IReadOnlyList<ErrorMessage> messages, ErrorType type)
	{
		Messages = [..messages];
		Type = type;
	}

	private Error(IEnumerable<ErrorMessage> messages, ErrorType type)
	{
		Messages = messages.ToArray();
		Type = type;
	}


	public IReadOnlyList<ErrorMessage> Messages { get; } = [];

	public ErrorType Type { get; }

	public static Error Validation(string code, string message, string? invalidField = null)
	{
		return new Error([new ErrorMessage(code, message, invalidField)], ErrorType.Validation);
	}

	public static Error Validation(IEnumerable<ErrorMessage> messages)
	{
		return new Error(messages, ErrorType.Validation);
	}

	public string GetMessage()
	{
		return string.Join(';', Messages.Select(m => m.ToString()));
	}

	public static Error NotFound(string code, string message, string? invalidField = null)
	{
		return new Error([new ErrorMessage(code, message, invalidField)], ErrorType.NotFound);
	}

	public static Error Failure(string code, string message, string? invalidField = null)
	{
		return new Error([new ErrorMessage(code, message, invalidField)], ErrorType.Failure);
	}

	public static Error Conflict(string code, string message, string? invalidField = null)
	{
		return new Error([new ErrorMessage(code, message, invalidField)], ErrorType.Conflict);
	}
}