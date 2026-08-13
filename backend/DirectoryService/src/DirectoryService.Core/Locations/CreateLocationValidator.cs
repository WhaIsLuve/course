using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Validations;
using DirectoryService.Domain.Locations;
using FluentValidation;

namespace DirectoryService.Core.Locations;

public class CreateLocationValidator : AbstractValidator<CreateLocationDto>
{
    public CreateLocationValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(LocationName.Create);

        RuleFor(x => x.Address)
            .MustBeValueObject(a => Address.Create(a.Country, a.City, a.Street, a.Building));
    }
}