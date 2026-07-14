using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Locations;

public record Address
{
    private const int CountryMaxLength = 200;
    private const int CityMaxLength = 200;
    private const int StreetMaxLength = 200;
    private const int BuildingMaxLength = 50;

    private Address(string country, string city, string? street, string? building)
    {
        Country = country;
        City = city;
        Street = street;
        Building = building;
    }

    private Address()
    {
    }

    public string Country { get; } = null!;

    public string City { get; } = null!;

    public string? Street { get; }

    public string? Building { get; }

    public static Result<Address, string> Create(
        string country,
        string city,
        string? street,
        string? building)
    {
        if (string.IsNullOrWhiteSpace(country))
            return Result.Failure<Address, string>("Country is required");

        if (country.Length > CountryMaxLength)
            return Result.Failure<Address, string>($"Country cannot exceed {CountryMaxLength} characters");

        if (string.IsNullOrWhiteSpace(city))
            return Result.Failure<Address, string>("City is required");

        if (city.Length > CityMaxLength)
            return Result.Failure<Address, string>($"City cannot exceed {CityMaxLength} characters");

        if (street != null && string.IsNullOrWhiteSpace(street))
            return Result.Failure<Address, string>("Street cannot be empty when specified");

        if (street is { Length: > StreetMaxLength })
            return Result.Failure<Address, string>($"Street cannot exceed {StreetMaxLength} characters");

        if (building != null && string.IsNullOrWhiteSpace(building))
            return Result.Failure<Address, string>("Building cannot be empty when specified");

        if (building is { Length: > BuildingMaxLength })
            return Result.Failure<Address, string>($"Building cannot exceed {BuildingMaxLength} characters");

        return Result.Success<Address, string>(new Address(country, city, street, building));
    }
}