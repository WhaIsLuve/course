using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Logging;
using DirectoryService.Core.Positions;
using DirectoryService.Domain.Positions;
using DirectoryService.SharedKernel.Errors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Features.Positions.Create;

public sealed class CreatePositionHandler(
    TimeProvider timeProvider,
    IPositionRepository positionRepository,
    ILogger<CreatePositionHandler> logger)
    : ICommandHandler<CreatePositionCommand, Result<Guid, Error>>
{
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    private readonly IPositionRepository _positionRepository =
        positionRepository ?? throw new ArgumentNullException(nameof(positionRepository));
    private readonly ILogger<CreatePositionHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result<Guid, Error>> HandleAsync(CreatePositionCommand command,
        CancellationToken cancellationToken = default)
    {
        if (await _positionRepository.ExistWithSameNameAsync(command.Dto.Name, cancellationToken))
        {
            return Error.Conflict("position.name.exists", "Должность с таким наименованием уже существует");
        }

        var name = PositionName.Create(command.Dto.Name);
        if (name.IsFailure)
            return name.Error;

        var id = Guid.CreateVersion7();
        var position = Position.Create(id, name.Value, _timeProvider.GetUtcNow().UtcDateTime);
        if (position.IsFailure)
            return position.Error;

        _positionRepository.Add(position.Value);
        _logger.PositionCreated(id);
        return id;
    }
}
