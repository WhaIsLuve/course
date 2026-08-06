using DirectoryService.SharedKernel.Envelopes;

namespace DirectoryService.Web.EndpointResults;

#pragma warning disable CA1515
public sealed class SuccessResult : IResult
#pragma warning restore CA1515
{
	public Task ExecuteAsync(HttpContext httpContext)
	{
		ArgumentNullException.ThrowIfNull(httpContext);

		var envelope = Envelope.Ok();

		httpContext.Response.StatusCode = StatusCodes.Status200OK;

		return httpContext.Response.WriteAsJsonAsync(envelope);
	}
}

#pragma warning disable CA1515
public sealed class SuccessResult<TValue> : IResult
#pragma warning restore CA1515
{
	private readonly TValue _value;

	public SuccessResult(TValue value)
	{
		_value = value;
	}

	public Task ExecuteAsync(HttpContext httpContext)
	{
		ArgumentNullException.ThrowIfNull(httpContext);

		var envelope = Envelope<TValue>.Ok(_value);

		httpContext.Response.StatusCode = StatusCodes.Status200OK;

		return httpContext.Response.WriteAsJsonAsync(envelope);
	}
}