using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Extensions;
using DirectoryService.Core.Locations;
using DirectoryService.Core.Logging;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.SharedKernel.Errors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Departments;

public sealed class DepartmentService(
	IValidator<CreateDepartmentDto> createDepartmentValidator,
	IDepartmentRepository repository,
	TimeProvider timeProvider,
	ILocationRepository locationRepository,
	IValidator<UpdateDepartmentNameDto> updateDepartmentNameValidator,
	ILogger<DepartmentService> logger)
	: IDepartmentService
{
	private readonly IValidator<CreateDepartmentDto> _createDepartmentValidator =
		createDepartmentValidator ?? throw new ArgumentNullException(nameof(createDepartmentValidator));

	private readonly IDepartmentRepository _departmentRepository =
		repository ?? throw new ArgumentNullException(nameof(repository));

	private readonly ILogger<DepartmentService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

	private readonly ILocationRepository _locationRepository =
		locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));

	private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

	private readonly IValidator<UpdateDepartmentNameDto> _updateDepartmentNameValidator =
		updateDepartmentNameValidator ?? throw new ArgumentNullException(nameof(updateDepartmentNameValidator));

	public async Task<Result<Guid, Error>> CreateAsync(CreateDepartmentDto dto,
		CancellationToken cancellationToken = default)
	{
		var validationResult = await _createDepartmentValidator.ValidateAsync(dto, cancellationToken);
		if (!validationResult.IsValid)
			return Error.Validation(validationResult.ToErrorMessages());

		Department? parent = null;

		if (dto.ParentId != null)
		{
			var parentResult = await _departmentRepository.GetByIdAsync(dto.ParentId.Value, cancellationToken);
			if (parentResult.IsFailure)
			{
				return parentResult.Error;
			}

			parent = parentResult.Value;
		}

		var id = Guid.CreateVersion7();
		var name = DepartmentName.Create(dto.Name);
		if (name.IsFailure) return name.Error;
		var slug = DepartmentSlug.Create(dto.Slug);
		if (slug.IsFailure) return slug.Error;
		var parentInfo = parent is null ? null : new ParentInfo(parent.Id, parent.Path);
		var dateTimeNow = _timeProvider.GetUtcNow().UtcDateTime;

		var department = Department.Create(id, name.Value, slug.Value, parentInfo, dateTimeNow);
		if (department.IsFailure) return department.Error;

		var result = await AddNewLocationInDepartment(dto.LocationIds, department.Value, dateTimeNow, cancellationToken);
		if (result.IsFailure)
		{
			return result.Error;
		}

		_departmentRepository.AddDepartment(department.Value);

		result = await _departmentRepository.Save(cancellationToken);
		if (result.IsFailure)
		{
			return result.Error;
		}

		_logger.DepartmentCreated(id, dto.ParentId, dto.LocationIds);
		return id;
	}

	public async Task<UnitResult<Error>> UpdateNameAsync(Guid id, UpdateDepartmentNameDto dto,
		CancellationToken cancellationToken = default)
	{
		var validationResult = await _updateDepartmentNameValidator.ValidateAsync(dto, cancellationToken);
		if (!validationResult.IsValid)
			return Error.Validation(validationResult.ToErrorMessages());
		var department = await _departmentRepository.GetByIdAsync(id, cancellationToken);
		if (department.IsFailure) return department.Error;
		var newName = DepartmentName.Create(dto.Name);
		if (newName.IsFailure) return newName.Error;
		var result = department.Value.UpdateName(newName.Value, _timeProvider.GetUtcNow().UtcDateTime);
		if (result.IsFailure) return result.Error;

		result = await _departmentRepository.Save(cancellationToken);
		if (result.IsFailure) return result.Error;

		_logger.DepartmentRenamed(id);
		return UnitResult.Success<Error>();
	}

	public async Task<UnitResult<Error>> AttachLocation(Guid departmentId, Guid locationId,
		CancellationToken cancellationToken = default)
	{
		var department = await _departmentRepository.GetByIdAsync(departmentId, cancellationToken);
		if (department.IsFailure) return department.Error;
		var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken);
		if (location.IsFailure) return location.Error;
		var existDepartmentLocation = await _departmentRepository.ExistDepartmentLocation(
			department.Value.Id, location.Value.Id, cancellationToken);
		if (existDepartmentLocation)
			return Error.Conflict("department.location.exist",
				"Связь между локацией и департаментов уже существует.");

		var departmentLocation = DepartmentLocation.Create(Guid.CreateVersion7(), department.Value.Id, location.Value.Id,
			_timeProvider.GetUtcNow().UtcDateTime);
		if (departmentLocation.IsFailure) return departmentLocation.Error;

		_departmentRepository.AddDepartmentLocations([departmentLocation.Value]);
		var result = await _departmentRepository.Save(cancellationToken);
		if (result.IsFailure) return result.Error;

		_logger.LocationAttached(departmentId, locationId);
		return UnitResult.Success<Error>();
	}

	public async Task<UnitResult<Error>> DetachLocation(Guid departmentId, Guid locationId,
		CancellationToken cancellationToken = default)
	{
		var department = await _departmentRepository.GetByIdAsync(departmentId, cancellationToken);
		if (department.IsFailure) return department.Error;
		var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken);
		if (location.IsFailure) return location.Error;
		var departmentLocation = await _departmentRepository.GetDepartmentLocation(
			department.Value.Id, location.Value.Id, cancellationToken);
		if (departmentLocation.IsFailure) return departmentLocation.Error;
		_departmentRepository.RemoveDepartmentLocation(departmentLocation.Value);

		var result = await _departmentRepository.Save(cancellationToken);
		if (result.IsFailure) return result.Error;

		_logger.LocationDetached(departmentId, locationId);
		return UnitResult.Success<Error>();
	}

	private async ValueTask<UnitResult<Error>> AddNewLocationInDepartment(IReadOnlyList<Guid> dtoLocationIds,
		Department department,
		DateTime dateTimeNow,
		CancellationToken cancellationToken)
	{
		if (dtoLocationIds.Count == 0) return UnitResult.Success<Error>();

		var locations = await _locationRepository.GetByIdsAsync(dtoLocationIds, cancellationToken);
		if (locations.IsFailure) return locations.Error;

		var departmentLocations = new List<DepartmentLocation>();
		foreach (var location in locations.Value)
		{
			var departmentLocation = DepartmentLocation.Create(Guid.CreateVersion7(), department.Id, location.Id, dateTimeNow);
			if (departmentLocation.IsFailure) return departmentLocation.Error;
			departmentLocations.Add(departmentLocation.Value);
		}

		_departmentRepository.AddDepartmentLocations(departmentLocations);
		return UnitResult.Success<Error>();
	}
}
