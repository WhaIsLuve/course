using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using FluentValidation;

namespace DirectoryService.Core.Locations;

public class UpdateLocationValidator : AbstractValidator<UpdateLocationDto>
{
	public UpdateLocationValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage("Имя обязательное поле")
			.WithErrorCode("location.name")
			.MaximumLength(LocationName.MaxLength)
			.WithMessage("Наименование должно быть меньше 200 символов")
			.WithErrorCode("location.name");

		RuleFor(x => x.Address)
			.ChildRules(x =>
			{
				x.RuleFor(a => a.Country)
					.NotEmpty()
					.WithMessage("Страна обязательное поле")
					.WithErrorCode("location.address.country")
					.MaximumLength(Address.CountryMaxLength)
					.WithErrorCode("location.address.country")
					.WithMessage("Страна должно быть меньше 200 символов");
				x.RuleFor(a => a.City)
					.NotEmpty()
					.WithMessage("Город обязательное поле")
					.WithErrorCode("location.address.city")
					.MaximumLength(Address.CityMaxLength)
					.WithErrorCode("location.address.city")
					.WithMessage("Город должно быть меньше 200 символов");
				x.RuleFor(a => a.Street)
					.Must(a => a == null || !string.IsNullOrWhiteSpace(a))
					.WithMessage("Улица обязательное поле")
					.WithErrorCode("location.address.street")
					.MaximumLength(Address.StreetMaxLength)
					.WithErrorCode("location.address.street")
					.WithMessage("Улица должно быть меньше 200 символов");
				x.RuleFor(a => a.Building)
					.Must(a => a == null || !string.IsNullOrWhiteSpace(a))
					.WithMessage("Дом обязательное поле")
					.WithErrorCode("location.address.building")
					.MaximumLength(Address.BuildingMaxLength)
					.WithErrorCode("location.address.building")
					.WithMessage("Дом должно быть меньше 50 символов");
			});
	}
}