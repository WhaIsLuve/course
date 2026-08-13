using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Validations;
using DirectoryService.Domain.Departments;
using FluentValidation;

namespace DirectoryService.Core.Departments;

public class UpdateDepartmentNameValidator : AbstractValidator<UpdateDepartmentNameDto>
{
    public UpdateDepartmentNameValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(DepartmentName.Create);
    }
}