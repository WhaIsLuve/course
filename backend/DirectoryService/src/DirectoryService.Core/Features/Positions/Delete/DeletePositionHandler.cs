using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Logging;
using DirectoryService.Core.Positions;
using DirectoryService.SharedKernel.Errors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Features.Positions.Delete;

public sealed class DeletePositionHandler(
    IPositionRepository positionRepository,
    TimeProvider timeProvider,
    ILogger<DeletePositionHandler> logger)
    : ICommandHandler<DeletePositionCommand, UnitResult<Error>>
{
    private readonly IPositionRepository _positionRepository =
        positionRepository ?? throw new ArgumentNullException(nameof(positionRepository));
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    private readonly ILogger<DeletePositionHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<UnitResult<Error>> HandleAsync(DeletePositionCommand command,
        CancellationToken cancellationToken = default)
    {
        var position = await _positionRepository.GetByIdAsync(command.PositionId, cancellationToken);
        if (position.IsFailure)
            return position.Error;

        if (await _positionRepository.HasDepartmentLinksAsync(command.PositionId, cancellationToken))
        {
            return Error.Conflict("position.department.links.exist",
                "Нельзя удалить должность, привязанную к подразделению");
        }

        var deleteResult = position.Value.Delete(_timeProvider.GetUtcNow().UtcDateTime);
        if (deleteResult.IsFailure)
            return deleteResult.Error;

        _logger.PositionDeleted(command.PositionId);
        return UnitResult.Success<Error>();
    }
}
