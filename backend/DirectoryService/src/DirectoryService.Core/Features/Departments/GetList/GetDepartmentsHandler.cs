using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Common;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Abstractions;
using DirectoryService.Domain.Departments;
using DirectoryService.SharedKernel.Errors;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Core.Features.Departments.GetList;

public sealed class GetDepartmentsHandler(IReadDbContext readDbContext)
    : IQueryHandler<GetDepartmentsQuery, Result<PagedResult<DepartmentListItemDto>, Error>>
{
    private const int MaxPageSize = 100;
    private const int MaxSearchLength = 200;

    private readonly IReadDbContext _readDbContext =
        readDbContext ?? throw new ArgumentNullException(nameof(readDbContext));

    public async Task<Result<PagedResult<DepartmentListItemDto>, Error>> HandleAsync(
        GetDepartmentsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var sortBy = query.SortBy ?? "name";
        var sortDir = query.SortDir ?? "asc";

        var validationError = Validate(query, sortBy, sortDir);
        if (validationError is not null)
            return validationError;

        int skip;
        try
        {
            skip = checked((query.Page - 1) * query.PageSize);
        }
        catch (OverflowException)
        {
            return Error.Validation(
                "departments.page.invalid",
                "Номер страницы слишком велик для выбранного размера страницы",
                "page");
        }

        var filteredDepartments = _readDbContext.Departments;
        if (!string.IsNullOrEmpty(query.Search))
        {
            var searchPattern = $"%{EscapeLikePattern(query.Search)}%".ToUpperInvariant();
#pragma warning disable CA1304, CA1311, MA0011 // Parameterless ToUpper is translated to SQL by EF Core.
            filteredDepartments = filteredDepartments.Where(department =>
                EF.Functions.Like(department.Name.Value.ToUpper(), searchPattern, "\\"));
#pragma warning restore CA1304, CA1311, MA0011
        }

        var totalCount = await filteredDepartments.CountAsync(cancellationToken);

        IOrderedQueryable<Department> orderedDepartments;
        if (string.Equals(sortBy, "name", StringComparison.Ordinal))
        {
            orderedDepartments = string.Equals(sortDir, "asc", StringComparison.Ordinal)
                ? filteredDepartments.OrderBy(department => department.Name.Value)
                : filteredDepartments.OrderByDescending(department => department.Name.Value);
        }
        else
        {
            orderedDepartments = string.Equals(sortDir, "asc", StringComparison.Ordinal)
                ? filteredDepartments.OrderBy(department => department.CreatedAt)
                : filteredDepartments.OrderByDescending(department => department.CreatedAt);
        }

        orderedDepartments = orderedDepartments.ThenBy(department => department.Id);

        var items = await orderedDepartments
            .Skip(skip)
            .Take(query.PageSize)
            .Select(department => new DepartmentListItemDto(
                department.Id,
                department.Name.Value,
                department.Slug.Value,
                department.Path.Value,
                department.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<DepartmentListItemDto>(items, totalCount, query.Page, query.PageSize);
    }

    private static Error? Validate(GetDepartmentsQuery query, string sortBy, string sortDir)
    {
        if (query.Page < 1)
            return Error.Validation("departments.page.invalid", "Номер страницы должен начинаться с 1", "page");

        if (query.PageSize < 1 || query.PageSize > MaxPageSize)
            return Error.Validation(
                "departments.page.size.invalid",
                $"Размер страницы должен быть от 1 до {MaxPageSize}",
                "pageSize");

        if (query.Search is not null && query.Search.Length > MaxSearchLength)
            return Error.Validation(
                "departments.search.too.long",
                $"Поисковая строка не может быть длиннее {MaxSearchLength} символов",
                "search");

        if (sortBy is not ("name" or "createdAt"))
            return Error.Validation(
                "departments.sort.by.invalid",
                "Поле сортировки должно быть name или createdAt",
                "sortBy");

        if (sortDir is not ("asc" or "desc"))
            return Error.Validation(
                "departments.sort.direction.invalid",
                "Направление сортировки должно быть asc или desc",
                "sortDir");

        return null;
    }

    private static string EscapeLikePattern(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
}
