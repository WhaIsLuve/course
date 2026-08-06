using DirectoryService.SharedKernel.Envelopes;

namespace DirectoryService.Web.EndpointResults;

#pragma warning disable CA1515
public sealed class CreatedResult<TValue> : IResult where TValue: notnull
#pragma warning restore CA1515
{
	private readonly TValue _value;

	public CreatedResult(TValue value)
	{
		_value = value;
	}

	public Task ExecuteAsync(HttpContext httpContext)
	{
		ArgumentNullException.ThrowIfNull(httpContext);

		var envelope = Envelope<TValue>.Ok(_value);

		httpContext.Response.StatusCode = StatusCodes.Status201Created;

		return httpContext.Response.WriteAsJsonAsync(envelope);
	}
}