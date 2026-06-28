using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Positions;

public record PositionName
{
    private const int MaxLength = 200;

    private PositionName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<PositionName, string> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<PositionName, string>("Name is required");

        if (value.Length > MaxLength)
            return Result.Failure<PositionName, string>($"Name cannot exceed {MaxLength} characters");

        return Result.Success<PositionName, string>(new PositionName(value));
    }
}