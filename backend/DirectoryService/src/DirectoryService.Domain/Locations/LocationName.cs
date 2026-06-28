using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Locations;

public record LocationName
{
    private const int MaxLength = 200;

    private LocationName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<LocationName, string> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<LocationName, string>("Name is required");

        if (value.Length > MaxLength)
            return Result.Failure<LocationName, string>($"Name cannot exceed {MaxLength} characters");

        return Result.Success<LocationName, string>(new LocationName(value));
    }
}