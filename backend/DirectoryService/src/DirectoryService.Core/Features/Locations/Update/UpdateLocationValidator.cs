using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Validations;
using DirectoryService.Domain.Locations;
using FluentValidation;

namespace DirectoryService.Core.Features.Locations.Update;

public sealed class UpdateLocationValidator : AbstractValidator<UpdateLocationDto>
{
    public UpdateLocationValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(LocationName.Create);

        RuleFor(x => x.Address)
            .MustBeValueObject(a => Address.Create(a.Country, a.City, a.Street, a.Building));
    }
}
