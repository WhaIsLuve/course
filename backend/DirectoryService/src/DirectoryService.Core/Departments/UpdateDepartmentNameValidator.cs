using DirectoryService.Contracts.Departments;
using FluentValidation;

namespace DirectoryService.Core.Departments;

public class UpdateDepartmentNameValidator : AbstractValidator<UpdateDepartmentNameDto>
{
    public UpdateDepartmentNameValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Наименование не может быть пустым")
            .WithErrorCode("department.name.invalid");
    }
}