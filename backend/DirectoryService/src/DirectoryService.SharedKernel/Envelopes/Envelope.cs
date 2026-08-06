using System.Text.Json.Serialization;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.SharedKernel.Envelopes;

public record Envelope
{
	public object? Result { get; }

	public Error? Error { get; }

	public bool IsError => Error != null;

	public DateTime TimeGenerated { get; }

	[JsonConstructor]
	private Envelope(object? result, Error? error)
	{
		Result = result;
		Error = error;
		TimeGenerated = DateTime.UtcNow;
	}

	public static Envelope Ok(object? result = null) =>
		new(result, null);

	public static Envelope Fail(Error error) =>
		new(null, error);
}

public record Envelope<T>
{
	public T? Result { get; }

	public Error? Error { get; }

	public bool IsError => Error != null;

	public DateTime TimeGenerated { get; }

	[JsonConstructor]
	private Envelope(T? result, Error? error)
	{
		Result = result;
		Error = error;
		TimeGenerated = DateTime.UtcNow;
	}

#pragma warning disable CA1000
	public static Envelope<T> Ok(T? result = default) =>
#pragma warning restore CA1000
		new(result, null);

#pragma warning disable CA1000
	public static Envelope<T> Fail(Error error) =>
#pragma warning restore CA1000
		new(default, error);
}