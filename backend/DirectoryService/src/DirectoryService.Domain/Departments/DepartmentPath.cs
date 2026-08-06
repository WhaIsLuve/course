using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Domain.Departments;

public partial record DepartmentPath
{
    private static readonly Regex SegmentRegex = Regex;

    private DepartmentPath(string value)
    {
        Value = value;
    }

    public string Value { get; }

    [GeneratedRegex("^[a-z0-9][a-z0-9_-]*[a-z0-9]$|^[a-z0-9]$", RegexOptions.Compiled, 1000)]
    private static partial Regex Regex { get; }

    public static Result<DepartmentPath, Error> Create(string value)
    {
        const string code = "department.path.invalid";
        if (string.IsNullOrWhiteSpace(value))
            return Error.Validation(code, "Путь пустой");

        if (!value.StartsWith('/'))
            return Error.Validation(code, "Путь должен начинаться с /");

        if (value.Contains("//", StringComparison.InvariantCultureIgnoreCase))
            return Error.Validation(code, "Путь не может содержать //");

        var segments = value.TrimStart('/').Split('/');
        foreach (var segment in segments)
        {
            if (string.IsNullOrWhiteSpace(segment))
                return Error.Validation(code, "Путь не может содержать пустые сегменты");

            if (!SegmentRegex.IsMatch(segment))
                return Error.Validation(code, "Путь может содержать только символы в нижнем регистре и цифры");
        }

        return new DepartmentPath(value);
    }
}