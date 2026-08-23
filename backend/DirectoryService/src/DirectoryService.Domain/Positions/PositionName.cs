using CSharpFunctionalExtensions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Domain.Positions;

public record PositionName
{
    public const int MaxLength = 200;

    private PositionName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<PositionName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.Validation("position.name.required", "Название должности обязательно", "name");

        if (value.Length > MaxLength)
            return Error.Validation("position.name.length",
                $"Название должности не может превышать {MaxLength} символов", "name");

        return Result.Success<PositionName, Error>(new PositionName(value));
    }
}
