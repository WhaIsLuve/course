using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Positions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Features.Positions.Create;
using DirectoryService.Core.Features.Positions.Delete;
using DirectoryService.Core.Features.Positions.Rename;
using DirectoryService.SharedKernel.Envelopes;
using DirectoryService.SharedKernel.Errors;
using DirectoryService.Web.EndpointResults;
using Microsoft.AspNetCore.Mvc;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace DirectoryService.Web.Controllers;

[ApiController]
[Route("api/v1/positions")]
#pragma warning disable CA1515
#pragma warning disable S6960
public sealed class PositionController(
    ICommandHandler<CreatePositionCommand, Result<Guid, Error>> createPositionHandler,
    ICommandHandler<RenamePositionCommand, UnitResult<Error>> renamePositionHandler,
    ICommandHandler<DeletePositionCommand, UnitResult<Error>> deletePositionHandler) : ControllerBase
#pragma warning restore CA1515
#pragma warning restore S6960
{
    private readonly ICommandHandler<CreatePositionCommand, Result<Guid, Error>> _createPositionHandler =
        createPositionHandler ?? throw new ArgumentNullException(nameof(createPositionHandler));
    private readonly ICommandHandler<RenamePositionCommand, UnitResult<Error>> _renamePositionHandler =
        renamePositionHandler ?? throw new ArgumentNullException(nameof(renamePositionHandler));
    private readonly ICommandHandler<DeletePositionCommand, UnitResult<Error>> _deletePositionHandler =
        deletePositionHandler ?? throw new ArgumentNullException(nameof(deletePositionHandler));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Envelope))]
    public async Task<IResult> Create([FromBody] CreatePositionDto dto, CancellationToken cancellationToken)
    {
        var result = await _createPositionHandler.HandleAsync(new CreatePositionCommand(dto), cancellationToken);
        return result.IsSuccess ? new CreatedResult<Guid>(result.Value) : new ErrorResult(result.Error);
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
    public async Task<EndpointResult> Rename([FromRoute] Guid id,
        [FromBody] UpdatePositionDto dto, CancellationToken cancellationToken)
    {
        return await _renamePositionHandler.HandleAsync(new RenamePositionCommand(id, dto), cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(Envelope))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Envelope))]
    public async Task<EndpointResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        return await _deletePositionHandler.HandleAsync(new DeletePositionCommand(id), cancellationToken);
    }
}
