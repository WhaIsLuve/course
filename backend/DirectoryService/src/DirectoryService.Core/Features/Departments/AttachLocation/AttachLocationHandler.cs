using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Departments;
using DirectoryService.Core.Locations;
using DirectoryService.Core.Logging;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.SharedKernel.Errors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Features.Departments.AttachLocation;

public sealed class AttachLocationHandler(
    IDepartmentRepository departmentRepository,
    ILocationRepository locationRepository,
    TimeProvider timeProvider,
    ILogger<AttachLocationHandler> logger)
    : ICommandHandler<AttachLocationCommand, UnitResult<Error>>
{
    private readonly IDepartmentRepository _departmentRepository =
        departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
    private readonly ILocationRepository _locationRepository =
        locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    private readonly ILogger<AttachLocationHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<UnitResult<Error>> HandleAsync(
        AttachLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetByIdAsync(command.DepartmentId, cancellationToken);
        if (department.IsFailure)
            return department.Error;
        var location = await _locationRepository.GetByIdAsync(command.LocationId, cancellationToken);
        if (location.IsFailure)
            return location.Error;
        var existDepartmentLocation = await _departmentRepository.ExistDepartmentLocation(
            department.Value.Id,
            location.Value.Id,
            cancellationToken);
        if (existDepartmentLocation)
            return Error.Conflict(
                "department.location.exist",
                "Связь между локацией и департаментов уже существует.");

        var departmentLocation = DepartmentLocation.Create(
            Guid.CreateVersion7(),
            department.Value.Id,
            location.Value.Id,
            _timeProvider.GetUtcNow().UtcDateTime);
        if (departmentLocation.IsFailure)
            return departmentLocation.Error;

        _departmentRepository.AddDepartmentLocations([departmentLocation.Value]);
        _logger.LocationAttached(command.DepartmentId, command.LocationId);
        return UnitResult.Success<Error>();
    }
}
