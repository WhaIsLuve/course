using DirectoryService.Contracts.Positions;
using DirectoryService.Core.Validations;
using DirectoryService.Domain.Positions;
using FluentValidation;

namespace DirectoryService.Core.Features.Positions.Create;

public sealed class CreatePositionValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionValidator()
    {
        RuleFor(x => x.Dto.Name).MustBeValueObject(PositionName.Create);
    }
}
