using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments;

public partial record DepartmentSlug
{
    private static readonly Regex SlugRegex = Regex;

    private DepartmentSlug(string value)
    {
        Value = value;
    }

    public string Value { get; }

    [GeneratedRegex(@"^[a-z0-9][a-z0-9_-]*[a-z0-9]$|^[a-z0-9]$", RegexOptions.Compiled, 1000)]
    private static partial Regex Regex { get; }

    public static Result<DepartmentSlug, string> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<DepartmentSlug, string>("Slug is required");

        if (!SlugRegex.IsMatch(value))
            return Result.Failure<DepartmentSlug, string>(
                "Slug must contain only lowercase letters, numbers, hyphens and underscores, and cannot start or end with hyphen or underscore");

        return Result.Success<DepartmentSlug, string>(new DepartmentSlug(value));
    }
}