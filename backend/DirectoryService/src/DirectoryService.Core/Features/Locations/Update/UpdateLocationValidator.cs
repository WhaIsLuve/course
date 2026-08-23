using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Validations;
using DirectoryService.Domain.Locations;
using FluentValidation;

namespace DirectoryService.Core.Features.Locations.Update;

public sealed class UpdateLocationValidator : AbstractValidator<UpdateLocationCommand>
{
    public UpdateLocationValidator()
    {
        RuleFor(x => x.Dto.Name)
            .MustBeValueObject(LocationName.Create);

        RuleFor(x => x.Dto.Address)
            .MustBeValueObject(a => Address.Create(a.Country, a.City, a.Street, a.Building));
    }
}
