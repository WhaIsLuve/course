using DirectoryService.Core.Validations;
using DirectoryService.Domain.Positions;
using FluentValidation;

namespace DirectoryService.Core.Features.Positions.Rename;

public sealed class RenamePositionValidator : AbstractValidator<RenamePositionCommand>
{
    public RenamePositionValidator()
    {
        RuleFor(x => x.Dto.Name).MustBeValueObject(PositionName.Create);
    }
}
