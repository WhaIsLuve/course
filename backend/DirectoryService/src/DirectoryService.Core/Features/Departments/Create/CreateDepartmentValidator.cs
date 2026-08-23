using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Validations;
using DirectoryService.Domain.Departments;
using DirectoryService.SharedKernel.Errors;
using FluentValidation;

namespace DirectoryService.Core.Features.Departments.Create;

public sealed class CreateDepartmentValidator : AbstractValidator<CreateDepartmentDto>
{
    public CreateDepartmentValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(DepartmentName.Create);
        RuleFor(x => x.ParentId)
            .NotEqual(Guid.Empty)
            .WithError(Error.Validation("department.parent.id.invalid", "Идентификатор родителя не может быть пустым"));
        RuleFor(x => x.Slug)
            .MustBeValueObject(DepartmentSlug.Create);
        RuleForEach(x => x.LocationIds)
            .NotEqual(Guid.Empty)
            .WithError(Error.Validation("department.location.id.invalid", "Идентификаторы локаций не могут быть пустыми"));
    }
}
