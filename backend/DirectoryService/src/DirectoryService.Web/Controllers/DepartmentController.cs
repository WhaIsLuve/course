using CSharpFunctionalExtensions;
using IResult = Microsoft.AspNetCore.Http.IResult;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Features.Departments.AttachLocation;
using DirectoryService.Core.Features.Departments.Create;
using DirectoryService.Core.Features.Departments.DetachLocation;
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
    ICommandHandler<DetachLocationCommand, UnitResult<Error>> detachLocationHandler) : ControllerBase
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
        return await _renameDepartmentHandler.HandleAsync(new RenameDepartmentCommand(id, dto), cancellationToken);
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

    [HttpPut("{id:guid}/position")]
    public async Task<IResult> AddPosition([FromRoute] Guid id, [FromQuery] Guid positionId,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok();
    }
}
