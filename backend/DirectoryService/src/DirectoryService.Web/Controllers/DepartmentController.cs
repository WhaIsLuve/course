using CSharpFunctionalExtensions;
using IResult = Microsoft.AspNetCore.Http.IResult;
using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Features.Departments.AttachLocation;
using DirectoryService.Core.Features.Departments.AttachPosition;
using DirectoryService.Core.Features.Departments.Create;
using DirectoryService.Core.Features.Departments.Delete;
using DirectoryService.Core.Features.Departments.DetachLocation;
using DirectoryService.Core.Features.Departments.DetachPosition;
using DirectoryService.Core.Features.Departments.GetById;
using DirectoryService.Core.Features.Departments.GetList;
using DirectoryService.Core.Features.Departments.Rename;
using DirectoryService.SharedKernel.Envelopes;
using DirectoryService.SharedKernel.Errors;
using DirectoryService.Web.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Web.Controllers;

[ApiController]
[Route("api/v1/departments")]
#pragma warning disable CA1515
#pragma warning disable S6960
public sealed class DepartmentController(
    ICommandHandler<CreateDepartmentCommand, Result<Guid, Error>> createDepartmentHandler,
    ICommandHandler<RenameDepartmentCommand, UnitResult<Error>> renameDepartmentHandler,
    ICommandHandler<AttachLocationCommand, UnitResult<Error>> attachLocationHandler,
    ICommandHandler<DetachLocationCommand, UnitResult<Error>> detachLocationHandler,
    ICommandHandler<DeleteDepartmentCommand, UnitResult<Error>> deleteDepartmentHandler,
    ICommandHandler<AttachPositionCommand, UnitResult<Error>> attachPositionHandler,
    ICommandHandler<DetachPositionCommand, UnitResult<Error>> detachPositionHandler,
    IQueryHandler<GetDepartmentByIdQuery, Result<DepartmentResponse, Error>> getDepartmentByIdHandler,
    IQueryHandler<GetDepartmentsQuery, Result<PagedResult<DepartmentListItemDto>, Error>> getDepartmentsHandler) : ControllerBase
#pragma warning restore CA1515
#pragma warning restore S6960
{
    private readonly ICommandHandler<CreateDepartmentCommand, Result<Guid, Error>> _createDepartmentHandler =
        createDepartmentHandler ?? throw new ArgumentNullException(nameof(createDepartmentHandler));
    private readonly ICommandHandler<RenameDepartmentCommand, UnitResult<Error>> _renameDepartmentHandler =
        renameDepartmentHandler ?? throw new ArgumentNullException(nameof(renameDepartmentHandler));
    private readonly ICommandHandler<AttachLocationCommand, UnitResult<Error>> _attachLocationHandler =
        attachLocationHandler ?? throw new ArgumentNullException(nameof(attachLocationHandler));
    private readonly ICommandHandler<DetachLocationCommand, UnitResult<Error>> _detachLocationHandler =
        detachLocationHandler ?? throw new ArgumentNullException(nameof(detachLocationHandler));
    private readonly ICommandHandler<DeleteDepartmentCommand, UnitResult<Error>> _deleteDepartmentHandler =
        deleteDepartmentHandler ?? throw new ArgumentNullException(nameof(deleteDepartmentHandler));
    private readonly ICommandHandler<AttachPositionCommand, UnitResult<Error>> _attachPositionHandler =
        attachPositionHandler ?? throw new ArgumentNullException(nameof(attachPositionHandler));
    private readonly ICommandHandler<DetachPositionCommand, UnitResult<Error>> _detachPositionHandler =
        detachPositionHandler ?? throw new ArgumentNullException(nameof(detachPositionHandler));
    private readonly IQueryHandler<GetDepartmentByIdQuery, Result<DepartmentResponse, Error>> _getDepartmentByIdHandler =
        getDepartmentByIdHandler ?? throw new ArgumentNullException(nameof(getDepartmentByIdHandler));
    private readonly IQueryHandler<GetDepartmentsQuery, Result<PagedResult<DepartmentListItemDto>, Error>> _getDepartmentsHandler =
        getDepartmentsHandler ?? throw new ArgumentNullException(nameof(getDepartmentsHandler));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Envelope))]
    public async Task<IResult> Create([FromBody] CreateDepartmentDto dto, CancellationToken cancellationToken)
    {
        var result = await _createDepartmentHandler.HandleAsync(new CreateDepartmentCommand(dto), cancellationToken);
        if (result.IsFailure)
            return new ErrorResult(result.Error);

        return new CreatedResult<Guid>(result.Value);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Envelope<PagedResult<DepartmentListItemDto>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Envelope))]
    public async Task<EndpointResult<PagedResult<DepartmentListItemDto>>> Get(
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return await _getDepartmentsHandler.HandleAsync(
            new GetDepartmentsQuery(search, sortBy, sortDir, page, pageSize),
            cancellationToken);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Envelope<DepartmentResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Envelope))]
    public async Task<EndpointResult<DepartmentResponse>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        return await _getDepartmentByIdHandler.HandleAsync(new GetDepartmentByIdQuery(id), cancellationToken);
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
        return await _renameDepartmentHandler.HandleAsync(new RenameDepartmentCommand(id, dto), cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Envelope))]
    public async Task<EndpointResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        return await _deleteDepartmentHandler.HandleAsync(new DeleteDepartmentCommand(id), cancellationToken);
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
        return await _attachLocationHandler.HandleAsync(
            new AttachLocationCommand(departmentId, locationId),
            cancellationToken);
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
        return await _detachLocationHandler.HandleAsync(
            new DetachLocationCommand(departmentId, locationId),
            cancellationToken);
    }

    [HttpPost("{departmentId:guid}/positions/{positionId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Envelope))]
    public async Task<EndpointResult> AttachPosition([FromRoute] Guid departmentId, [FromRoute] Guid positionId,
        CancellationToken cancellationToken)
    {
        return await _attachPositionHandler.HandleAsync(
            new AttachPositionCommand(departmentId, positionId), cancellationToken);
    }

    [HttpDelete("{departmentId:guid}/positions/{positionId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Envelope))]
    public async Task<EndpointResult> DetachPosition([FromRoute] Guid departmentId, [FromRoute] Guid positionId,
        CancellationToken cancellationToken)
    {
        return await _detachPositionHandler.HandleAsync(
            new DetachPositionCommand(departmentId, positionId), cancellationToken);
    }
}
