using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Logging;
using DirectoryService.Core.Positions;
using DirectoryService.Domain.Positions;
using DirectoryService.SharedKernel.Errors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Features.Positions.Rename;

public sealed class RenamePositionHandler(
    TimeProvider timeProvider,
    IPositionRepository positionRepository,
    ILogger<RenamePositionHandler> logger)
    : ICommandHandler<RenamePositionCommand, UnitResult<Error>>
{
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    private readonly IPositionRepository _positionRepository =
        positionRepository ?? throw new ArgumentNullException(nameof(positionRepository));
    private readonly ILogger<RenamePositionHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<UnitResult<Error>> HandleAsync(RenamePositionCommand command,
        CancellationToken cancellationToken = default)
    {
        var position = await _positionRepository.GetByIdAsync(command.PositionId, cancellationToken);
        if (position.IsFailure)
            return position.Error;

        var name = PositionName.Create(command.Dto.Name);
        if (name.IsFailure)
            return name.Error;

        if (!string.Equals(position.Value.Name.Value, command.Dto.Name, StringComparison.OrdinalIgnoreCase) &&
            await _positionRepository.ExistWithSameNameAsync(command.Dto.Name, cancellationToken))
        {
            return Error.Conflict("position.name.exists", "Должность с таким наименованием уже существует");
        }

        var result = position.Value.Update(name.Value, _timeProvider.GetUtcNow().UtcDateTime);
        if (result.IsFailure)
            return result.Error;

        _logger.PositionRenamed(command.PositionId);
        return UnitResult.Success<Error>();
    }
}
