using DirectoryService.Contracts.Departments;
using FluentValidation;

namespace DirectoryService.Core.Departments;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentDto>
{
	public CreateDepartmentValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage("Наименование не может быть пустым")
			.WithErrorCode("department.name.invalid");
		RuleFor(x => x.ParentId)
			.NotEqual(Guid.Empty)
			.WithMessage("Идентификатор родителя не может быть пустым")
			.WithErrorCode("department.parent.id.invalid");
		RuleFor(x => x.Slug)
			.NotEmpty()
			.WithMessage("Slug не может быть пустым")
			.WithErrorCode("department.slug.invalid");
		RuleForEach(x => x.LocationIds)
			.NotEqual(Guid.Empty)
			.WithErrorCode("department.location.id.invalid");
	}
}