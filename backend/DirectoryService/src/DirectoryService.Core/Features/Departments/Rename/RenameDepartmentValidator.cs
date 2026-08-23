using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Validations;
using DirectoryService.Domain.Departments;
using FluentValidation;

namespace DirectoryService.Core.Features.Departments.Rename;

public sealed class RenameDepartmentValidator : AbstractValidator<RenameDepartmentCommand>
{
    public RenameDepartmentValidator()
    {
        RuleFor(x => x.Dto.Name)
            .MustBeValueObject(DepartmentName.Create);
    }
}
