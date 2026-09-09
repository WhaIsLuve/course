using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Locations;
using DirectoryService.Core.Logging;
using DirectoryService.SharedKernel.Errors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Features.Locations.Delete;

public sealed class DeleteLocationHandler(
    ILocationRepository locationRepository,
    TimeProvider timeProvider,
    ILogger<DeleteLocationHandler> logger)
    : ICommandHandler<DeleteLocationCommand, UnitResult<Error>>
{
    private readonly ILocationRepository _locationRepository =
        locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    private readonly ILogger<DeleteLocationHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<UnitResult<Error>> HandleAsync(DeleteLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        var location = await _locationRepository.GetByIdAsync(command.LocationId, cancellationToken);
        if (location.IsFailure)
            return location.Error;

        if (await _locationRepository.HasDepartmentLinksAsync(command.LocationId, cancellationToken))
        {
            return Error.Conflict("location.department.links.exist",
                "Нельзя удалить локацию, привязанную к подразделению");
        }

        var deleteResult = location.Value.Delete(_timeProvider.GetUtcNow().UtcDateTime);
        if (deleteResult.IsFailure)
            return deleteResult.Error;

        _logger.LocationDeleted(command.LocationId);
        return UnitResult.Success<Error>();
    }
}
