using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using FluentValidation;

namespace DirectoryService.Core.Locations;

public class CreateLocationValidator : AbstractValidator<CreateLocationDto>
{
	public CreateLocationValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty()
			.NotNull()
			.WithMessage("Имя обязательное поле")
			.MaximumLength(LocationName.MaxLength)
			.WithMessage("Наименование должно быть меньше 200 символов");

		RuleFor(x => x.Address)
			.ChildRules(x =>
			{
				x.RuleFor(a => a.Country)
					.NotEmpty()
					.NotNull()
					.WithMessage("Страна обязательное поле")
					.MaximumLength(Address.CountryMaxLength)
					.WithMessage("Страна должно быть меньше 200 символов");
				x.RuleFor(a => a.City)
					.NotEmpty()
					.NotNull()
					.WithMessage("Город обязательное поле")
					.MaximumLength(Address.CityMaxLength)
					.WithMessage("Город должно быть меньше 200 символов");
				x.RuleFor(a => a.Street)
					.Must(a => a == null || !string.IsNullOrWhiteSpace(a))
					.WithMessage("Улица обязательное поле")
					.MaximumLength(Address.StreetMaxLength)
					.WithMessage("Улица должно быть меньше 200 символов");
				x.RuleFor(a => a.Building)
					.Must(a => a == null || !string.IsNullOrWhiteSpace(a))
					.WithMessage("Дом обязательное поле")
					.MaximumLength(Address.BuildingMaxLength)
					.WithMessage("Дом должно быть меньше 50 символов");
			});
	}
}