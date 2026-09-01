using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Core.Features.Departments.GetById;

public sealed class GetDepartmentByIdHandler(IReadDbContext readDbContext)
    : IQueryHandler<GetDepartmentByIdQuery, Result<DepartmentResponse, Error>>
{
    private readonly IReadDbContext _readDbContext =
        readDbContext ?? throw new ArgumentNullException(nameof(readDbContext));

    public async Task<Result<DepartmentResponse, Error>> HandleAsync(
        GetDepartmentByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var response = await _readDbContext.Departments
            .Where(department => department.Id == query.Id)
            .Select(department => new DepartmentResponse(
                department.Id,
                department.Name.Value,
                department.Slug.Value,
                department.Path.Value,
                department.ParentId,
                department.CreatedAt,
                department.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);

        return response is null
            ? Error.NotFound(
                "department.not.found",
                $"Департамент с идентификатором {query.Id} не найден")
            : response;
    }
}
