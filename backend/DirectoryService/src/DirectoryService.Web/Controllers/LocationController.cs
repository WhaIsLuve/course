using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Locations;
using DirectoryService.SharedKernel.Envelopes;
using DirectoryService.Web.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Web.Controllers;

[ApiController]
[Route("api/v1/locations")]
#pragma warning disable CA1515
public sealed class LocationController(ILocationService locationService) : ControllerBase
#pragma warning restore CA1515
{
    private readonly ILocationService _locationService =
        locationService ?? throw new ArgumentNullException(nameof(locationService));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Envelope))]
    public async Task<IResult> Create([FromBody] CreateLocationDto dto, CancellationToken cancellationToken)
    {
        var result = await _locationService.CreateAsync(dto, cancellationToken);
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
    public async Task<EndpointResult> Update([FromRoute] Guid id,
        [FromBody] UpdateLocationDto dto,
        CancellationToken cancellationToken)
    {
        return await _locationService.UpdateAsync(id, dto, cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        return TypedResults.Ok();
    }
}