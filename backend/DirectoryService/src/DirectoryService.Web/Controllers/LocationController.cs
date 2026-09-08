using CSharpFunctionalExtensions;
using IResult = Microsoft.AspNetCore.Http.IResult;
using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Features.Locations.Create;
using DirectoryService.Core.Features.Locations.Delete;
using DirectoryService.Core.Features.Locations.GetById;
using DirectoryService.Core.Features.Locations.GetList;
using DirectoryService.Core.Features.Locations.Top;
using DirectoryService.Core.Features.Locations.Update;
using DirectoryService.SharedKernel.Envelopes;
using DirectoryService.SharedKernel.Errors;
using DirectoryService.Web.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Web.Controllers;

[ApiController]
[Route("api/v1/locations")]
#pragma warning disable CA1515
#pragma warning disable S6960
public sealed class LocationController(
    ICommandHandler<CreateLocationCommand, Result<Guid, Error>> createLocationHandler,
    ICommandHandler<UpdateLocationCommand, UnitResult<Error>> updateLocationHandler,
    ICommandHandler<DeleteLocationCommand, UnitResult<Error>> deleteLocationHandler,
    IQueryHandler<GetLocationByIdQuery, Result<LocationResponse, Error>> getLocationByIdHandler,
    IQueryHandler<GetTopLocationQuery, Result<LocationTopResponse[], Error>> getTopLocationHandler,
    IQueryHandler<GetLocationsQuery, Result<PagedResult<LocationListItemDto>, Error>> getLocationsHandler) : ControllerBase
#pragma warning restore CA1515
#pragma warning restore S6960
{
    private readonly ICommandHandler<CreateLocationCommand, Result<Guid, Error>> _createLocationHandler =
        createLocationHandler ?? throw new ArgumentNullException(nameof(createLocationHandler));

    private readonly ICommandHandler<UpdateLocationCommand, UnitResult<Error>> _updateLocationHandler =
        updateLocationHandler ?? throw new ArgumentNullException(nameof(updateLocationHandler));

    private readonly ICommandHandler<DeleteLocationCommand, UnitResult<Error>> _deleteLocationHandler =
        deleteLocationHandler ?? throw new ArgumentNullException(nameof(deleteLocationHandler));

    private readonly IQueryHandler<GetLocationByIdQuery, Result<LocationResponse, Error>> _getLocationByIdHandler =
        getLocationByIdHandler ?? throw new ArgumentNullException(nameof(getLocationByIdHandler));

    private readonly IQueryHandler<GetTopLocationQuery, Result<LocationTopResponse[], Error>> _getTopLocationHandler =
        getTopLocationHandler ?? throw new ArgumentNullException(nameof(getTopLocationHandler));

    private readonly IQueryHandler<GetLocationsQuery, Result<PagedResult<LocationListItemDto>, Error>> _getLocationsHandler =
        getLocationsHandler ?? throw new ArgumentNullException(nameof(getLocationsHandler));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Envelope))]
    public async Task<IResult> Create([FromBody] CreateLocationDto dto, CancellationToken cancellationToken)
    {
        var result = await _createLocationHandler.HandleAsync(new CreateLocationCommand(dto), cancellationToken);
        if (result.IsFailure)
            return new ErrorResult(result.Error);

        return new CreatedResult<Guid>(result.Value);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Envelope<PagedResult<LocationListItemDto>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Envelope))]
    public async Task<EndpointResult<PagedResult<LocationListItemDto>>> Get(
        [FromQuery] string? search,
        [FromQuery] int? minDepartmentCount,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return await _getLocationsHandler.HandleAsync(
            new GetLocationsQuery(search, minDepartmentCount, sortBy, sortDir, page, pageSize),
            cancellationToken);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Envelope<LocationResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Envelope))]
    public async Task<EndpointResult<LocationResponse>> GetById([FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        return await _getLocationByIdHandler.HandleAsync(new GetLocationByIdQuery(id), cancellationToken);
    }

    [HttpGet("top")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Envelope<LocationTopResponse[]>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Envelope))]
    public async Task<EndpointResult<LocationTopResponse[]>> GetTop(CancellationToken cancellationToken)
    {
        return await _getTopLocationHandler.HandleAsync(new GetTopLocationQuery(), cancellationToken);
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
        return await _updateLocationHandler.HandleAsync(new UpdateLocationCommand(id, dto), cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Envelope))]
    public async Task<EndpointResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        return await _deleteLocationHandler.HandleAsync(new DeleteLocationCommand(id), cancellationToken);
    }
}
