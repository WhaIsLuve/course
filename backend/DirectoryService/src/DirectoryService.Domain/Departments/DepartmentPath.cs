using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments;

public partial record DepartmentPath
{
    private static readonly Regex SegmentRegex = Regex;

    private DepartmentPath(string value)
    {
        Value = value;
    }

    public string Value { get; }

    [GeneratedRegex(@"^[a-z0-9][a-z0-9_-]*[a-z0-9]$|^[a-z0-9]$", RegexOptions.Compiled, 1000)]
    private static partial Regex Regex { get; }

    public static Result<DepartmentPath, string> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<DepartmentPath, string>("Path is required");

        if (!value.StartsWith('/'))
            return Result.Failure<DepartmentPath, string>("Path must start with /");

        if (value.Contains("//", StringComparison.InvariantCultureIgnoreCase))
            return Result.Failure<DepartmentPath, string>("Path cannot contain //");

        var segments = value.TrimStart('/').Split('/');
        foreach (var segment in segments)
        {
            if (string.IsNullOrWhiteSpace(segment))
                return Result.Failure<DepartmentPath, string>("Path cannot contain empty segments");

            if (!SegmentRegex.IsMatch(segment))
                return Result.Failure<DepartmentPath, string>(
                    $"Invalid path segment: {segment}. Must contain only lowercase letters, numbers, hyphens and underscores");
        }

        return Result.Success<DepartmentPath, string>(new DepartmentPath(value));
    }
}