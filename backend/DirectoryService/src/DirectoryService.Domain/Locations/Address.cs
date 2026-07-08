using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Locations;

public record Address
{
    private const int CountryMaxLength = 200;
    private const int CityMaxLength = 200;
    private const int StreetMaxLength = 200;
    private const int BuildingMaxLength = 50;

    private Address(string country, string city, Maybe<string> street, Maybe<string> building)
    {
        Country = country;
        City = city;
        Street = street;
        Building = building;
    }

    public string Country { get; }

    public string City { get; }

    public Maybe<string> Street { get; }

    public Maybe<string> Building { get; }

    public static Result<Address, string> Create(
        string country,
        string city,
        Maybe<string> street,
        Maybe<string> building)
    {
        if (string.IsNullOrWhiteSpace(country))
            return Result.Failure<Address, string>("Country is required");

        if (country.Length > CountryMaxLength)
            return Result.Failure<Address, string>($"Country cannot exceed {CountryMaxLength} characters");

        if (string.IsNullOrWhiteSpace(city))
            return Result.Failure<Address, string>("City is required");

        if (city.Length > CityMaxLength)
            return Result.Failure<Address, string>($"City cannot exceed {CityMaxLength} characters");

        if (street.HasValue && string.IsNullOrWhiteSpace(street.Value))
            return Result.Failure<Address, string>("Street cannot be empty when specified");

        if (street.HasValue && street.Value.Length > StreetMaxLength)
            return Result.Failure<Address, string>($"Street cannot exceed {StreetMaxLength} characters");

        if (building.HasValue && string.IsNullOrWhiteSpace(building.Value))
            return Result.Failure<Address, string>("Building cannot be empty when specified");

        if (building.HasValue && building.Value.Length > BuildingMaxLength)
            return Result.Failure<Address, string>($"Building cannot exceed {BuildingMaxLength} characters");

        return Result.Success<Address, string>(new Address(country, city, street, building));
    }
}