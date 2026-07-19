using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Web.Controllers;

[ApiController]
[Route("api/v1/positions")]
#pragma warning disable CA1515
public sealed class PositionController : ControllerBase
#pragma warning restore CA1515
{
	[HttpPost]
	public async Task<IResult> Create([FromBody] string name, CancellationToken cancellationToken)
	{
		HttpContext.Response.Headers.Append("Location", Guid.CreateVersion7().ToString());
		return TypedResults.Created();
	}

	[HttpGet]
	public async Task<IResult> Get(CancellationToken cancellationToken)
	{
		return TypedResults.Ok();
	}

	[HttpGet("{id:guid}")]
	public async Task<IResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
	{
		return TypedResults.Ok();
	}

	[HttpPut("{id:guid}")]
	public async Task<IResult> Update([FromRoute] Guid id,
		[FromBody] string name,
		CancellationToken cancellationToken)
	{
		return TypedResults.Ok();
	}

	[HttpDelete("{id:guid}")]
	public async Task<IResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
	{
		return TypedResults.Ok();
	}
}