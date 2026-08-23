using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Departments;
using DirectoryService.Core.Extensions;
using DirectoryService.Core.Locations;
using DirectoryService.Core.Logging;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.SharedKernel.Errors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Features.Departments.Create;

public sealed class CreateDepartmentHandler(
    IValidator<CreateDepartmentDto> validator,
    IDepartmentRepository departmentRepository,
    TimeProvider timeProvider,
    ILocationRepository locationRepository,
    ILogger<CreateDepartmentHandler> logger)
    : ICommandHandler<CreateDepartmentCommand, Result<Guid, Error>>
{
    private readonly IValidator<CreateDepartmentDto> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly IDepartmentRepository _departmentRepository =
        departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    private readonly ILocationRepository _locationRepository =
        locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
    private readonly ILogger<CreateDepartmentHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result<Guid, Error>> HandleAsync(
        CreateDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command.Dto, cancellationToken);
        if (!validationResult.IsValid)
            return Error.Validation(validationResult.ToErrorMessages());

        Department? parent = null;

        if (command.Dto.ParentId != null)
        {
            var parentResult = await _departmentRepository.GetByIdAsync(command.Dto.ParentId.Value, cancellationToken);
            if (parentResult.IsFailure)
                return parentResult.Error;

            parent = parentResult.Value;
        }

        var id = Guid.CreateVersion7();
        var name = DepartmentName.Create(command.Dto.Name);
        if (name.IsFailure) return name.Error;
        var slug = DepartmentSlug.Create(command.Dto.Slug);
        if (slug.IsFailure) return slug.Error;
        var parentInfo = parent is null ? null : new ParentInfo(parent.Id, parent.Path);
        var dateTimeNow = _timeProvider.GetUtcNow().UtcDateTime;

        var department = Department.Create(id, name.Value, slug.Value, parentInfo, dateTimeNow);
        if (department.IsFailure) return department.Error;

        var result = await AddNewLocationInDepartment(
            command.Dto.LocationIds,
            department.Value,
            dateTimeNow,
            cancellationToken);
        if (result.IsFailure)
            return result.Error;

        _departmentRepository.AddDepartment(department.Value);

        result = await _departmentRepository.Save(cancellationToken);
        if (result.IsFailure)
            return result.Error;

        _logger.DepartmentCreated(id, command.Dto.ParentId, command.Dto.LocationIds);
        return id;
    }

    private async ValueTask<UnitResult<Error>> AddNewLocationInDepartment(
        IReadOnlyList<Guid> locationIds,
        Department department,
        DateTime dateTimeNow,
        CancellationToken cancellationToken)
    {
        if (locationIds.Count == 0)
            return UnitResult.Success<Error>();

        var locations = await _locationRepository.GetByIdsAsync(locationIds, cancellationToken);
        if (locations.IsFailure)
            return locations.Error;

        var departmentLocations = new List<DepartmentLocation>();
        foreach (var location in locations.Value)
        {
            var departmentLocation = DepartmentLocation.Create(
                Guid.CreateVersion7(),
                department.Id,
                location.Id,
                dateTimeNow);
            if (departmentLocation.IsFailure)
                return departmentLocation.Error;

            departmentLocations.Add(departmentLocation.Value);
        }

        _departmentRepository.AddDepartmentLocations(departmentLocations);
        return UnitResult.Success<Error>();
    }
}
