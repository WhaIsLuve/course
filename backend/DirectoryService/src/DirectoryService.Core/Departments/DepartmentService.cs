using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Locations;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using FluentValidation;

namespace DirectoryService.Core.Departments;

public class DepartmentService(
	IValidator<CreateDepartmentDto> validator,
	IDepartmentRepository repository,
	TimeProvider timeProvider,
	ILocationRepository locationRepository)
	: IDepartmentService
{
	private readonly IValidator<CreateDepartmentDto> _validator =
		validator ?? throw new ArgumentNullException(nameof(validator));

	private readonly IDepartmentRepository _departmentRepository =
		repository ?? throw new ArgumentNullException(nameof(repository));

	private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

	private readonly ILocationRepository _locationRepository =
		locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));

	public async Task<Guid> CreateAsync(CreateDepartmentDto dto, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
		if (!validationResult.IsValid)
		{
			throw new ValidationException(validationResult.Errors);
		}

		Department? parent = null;

		if (dto.ParentId != null)
		{
			parent = await _departmentRepository.GetByIdAsync(dto.ParentId.Value, cancellationToken) ??
			         throw new InvalidOperationException("Не найден родитель");
		}

		var id = Guid.CreateVersion7();
		var name = DepartmentName.Create(dto.Name);
		var slug = DepartmentSlug.Create(dto.Slug);
		var parentInfo = parent is null ? null : new ParentInfo(parent.Id, parent.Path);
		var dateTimeNow = _timeProvider.GetUtcNow().UtcDateTime;
		if (name.IsFailure) throw new ValidationException(name.Error);
		if (slug.IsFailure) throw new ValidationException(slug.Error);

		var department =
			Department.Create(id, name.Value, slug.Value, parentInfo, dateTimeNow);
		if (department.IsFailure) throw new InvalidOperationException(department.Error);

		await AddNewLocationInDepartment(dto.LocationIds, department.Value, dateTimeNow, cancellationToken);

		_departmentRepository.AddDepartment(department.Value);

		await _departmentRepository.Save(cancellationToken);


		return id;
	}

	private async ValueTask AddNewLocationInDepartment(IReadOnlyList<Guid> dtoLocationIds,
		Department department,
		DateTime dateTimeNow,
		CancellationToken cancellationToken)
	{
		if (dtoLocationIds.Count == 0)
		{
			return;
		}

		var locations = await _locationRepository.GetByIdsAsync(dtoLocationIds, cancellationToken);
		var missedLocations = dtoLocationIds.Except(locations.Select(l => l.Id)).ToList();
		if (missedLocations.Count != 0)
		{
			throw new InvalidOperationException("Переданы не существующие локации.");
		}

		var departmentLocation = locations.Select(x =>
				DepartmentLocation.Create(Guid.CreateVersion7(), department.Id, x.Id, dateTimeNow))
			.Where(x => x.IsFailure ? throw new ValidationException(x.Error) : x.IsSuccess)
			.Select(x => x.Value)
			.ToList();

		_departmentRepository.AddDepartmentLocations(departmentLocation);
	}
}