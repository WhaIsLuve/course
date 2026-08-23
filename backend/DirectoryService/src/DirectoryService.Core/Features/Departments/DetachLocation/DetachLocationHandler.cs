using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Departments;
using DirectoryService.Core.Locations;
using DirectoryService.Core.Logging;
using DirectoryService.SharedKernel.Errors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Features.Departments.DetachLocation;

public sealed class DetachLocationHandler(
    IDepartmentRepository departmentRepository,
    ILocationRepository locationRepository,
    ILogger<DetachLocationHandler> logger)
    : ICommandHandler<DetachLocationCommand, UnitResult<Error>>
{
    private readonly IDepartmentRepository _departmentRepository =
        departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
    private readonly ILocationRepository _locationRepository =
        locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
    private readonly ILogger<DetachLocationHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<UnitResult<Error>> HandleAsync(
        DetachLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetByIdAsync(command.DepartmentId, cancellationToken);
        if (department.IsFailure)
            return department.Error;
        var location = await _locationRepository.GetByIdAsync(command.LocationId, cancellationToken);
        if (location.IsFailure)
            return location.Error;
        var departmentLocation = await _departmentRepository.GetDepartmentLocation(
            department.Value.Id,
            location.Value.Id,
            cancellationToken);
        if (departmentLocation.IsFailure)
            return departmentLocation.Error;

        _departmentRepository.RemoveDepartmentLocation(departmentLocation.Value);
        _logger.LocationDetached(command.DepartmentId, command.LocationId);
        return UnitResult.Success<Error>();
    }
}
