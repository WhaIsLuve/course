using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments;

public record DepartmentName
{
    private DepartmentName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<DepartmentName, string> Create(string value)
    {
        return Result.SuccessIf<DepartmentName, string>(!string.IsNullOrWhiteSpace(value), new DepartmentName(value),
            "value is required");
    }
}