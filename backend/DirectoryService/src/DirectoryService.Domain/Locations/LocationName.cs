using CSharpFunctionalExtensions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Domain.Locations;

public record LocationName
{
    public const int MaxLength = 200;

    private LocationName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<LocationName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<LocationName, Error>(Error.Validation("location.name", "Name is required"));

        if (value.Length > MaxLength)
            return Result.Failure<LocationName, Error>(Error.Validation("location.name", $"Name cannot exceed {MaxLength} characters"));

        return Result.Success<LocationName, Error>(new LocationName(value));
    }
}