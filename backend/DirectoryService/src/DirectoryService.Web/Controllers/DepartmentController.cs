using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Departments;
using DirectoryService.SharedKernel.Envelopes;
using DirectoryService.Web.EndpointResults;
using DirectoryService.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Web.Controllers;

[ApiController]
[Route("api/v1/departments")]
#pragma warning disable CA1515
public sealed class DepartmentController(IDepartmentService departmentService) : ControllerBase
#pragma warning restore CA1515
{
	private readonly IDepartmentService _departmentService =
		departmentService ?? throw new ArgumentNullException(nameof(departmentService));

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Envelope))]
	[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Envelope))]
	[ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Envelope))]
	[ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(Envelope))]
	[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Envelope))]
	public async Task<IResult> Create([FromBody] CreateDepartmentDto dto, CancellationToken cancellationToken)
	{
		var result = await _departmentService.CreateAsync(dto, cancellationToken);
		if (result.IsFailure)
		{
			return new ErrorResult(result.Error);
		}

		return new CreatedResult<Guid>(result.Value);
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

	[HttpPatch("{id:guid}")]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Envelope))]
	[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Envelope))]
	[ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Envelope))]
	[ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(Envelope))]
	[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Envelope))]
	public async Task<EndpointResult> UpdateName([FromRoute] Guid id, [FromBody] UpdateDepartmentNameDto dto,
		CancellationToken cancellationToken)
	{
		return await _departmentService.UpdateNameAsync(id, dto, cancellationToken);
	}

	[HttpDelete("{id:guid}")]
	public async Task<IResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
	{
		return TypedResults.Ok();
	}

	[HttpPost("{departmentId:guid}/location/{locationId:guid}")]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Envelope))]
	[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Envelope))]
	[ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Envelope))]
	[ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(Envelope))]
	[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Envelope))]
	public async Task<EndpointResult> AttachLocation([FromRoute] Guid departmentId, [FromRoute] Guid locationId,
		CancellationToken cancellationToken)
	{
		return await _departmentService.AttachLocation(departmentId, locationId, cancellationToken);
	}

	[HttpDelete("{departmentId:guid}/location/{locationId:guid}")]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Envelope))]
	[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Envelope))]
	[ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Envelope))]
	[ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(Envelope))]
	[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Envelope))]
	public async Task<EndpointResult> DetachLocation([FromRoute] Guid departmentId, [FromRoute] Guid locationId,
		CancellationToken cancellationToken)
	{
		return await _departmentService.DetachLocation(departmentId, locationId, cancellationToken);
	}

	[HttpPut("{id:guid}/position")]
	public async Task<IResult> AddPosition([FromRoute] Guid id, [FromQuery] Guid positionId,
		CancellationToken cancellationToken)
	{
		return TypedResults.Ok();
	}
}