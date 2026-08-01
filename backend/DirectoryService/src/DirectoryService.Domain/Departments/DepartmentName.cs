using CSharpFunctionalExtensions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Domain.Departments;

public record DepartmentName
{
    private DepartmentName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<DepartmentName, Error> Create(string value)
    {
        return Result.SuccessIf(!string.IsNullOrWhiteSpace(value), new DepartmentName(value),
            Error.Validation("department.name.invalid", "Наименование указано некоректно"));
    }
}