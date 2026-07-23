using DirectoryService.Contracts.Departments;
using FluentValidation;

namespace DirectoryService.Core.Departments;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentDto>
{
	public CreateDepartmentValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage("Наименование не может быть пустым");
		RuleFor(x => x.ParentId)
			.NotEqual(Guid.Empty)
			.WithMessage("Идентификатор родителя не может быть пустым");
		RuleFor(x => x.Slug)
			.NotEmpty()
			.WithMessage("Slug не может быть пустым");
		RuleForEach(x => x.LocationIds)
			.NotEqual(Guid.Empty);
	}
}