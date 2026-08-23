using CSharpFunctionalExtensions;
using DirectoryService.Core.Positions;
using DirectoryService.Domain.Positions;
using DirectoryService.SharedKernel.Errors;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

internal sealed class PositionRepository(AppDbContext dbContext) : IPositionRepository
{
    private readonly AppDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public void Add(Position position)
    {
        _dbContext.Positions.Add(position);
    }

    public void Remove(Position position)
    {
        _dbContext.Positions.Remove(position);
    }

    public async ValueTask<Result<Position, Error>> GetByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var position = await _dbContext.Positions.FindAsync([id], cancellationToken);
        return position.ToResult(Error.NotFound("position.not.found", "Должность не найдена"));
    }

    public Task<bool> ExistWithSameNameAsync(string name, CancellationToken cancellationToken = default)
    {
#pragma warning disable CA1862, RCS1155, MA0011, CA1304, CA1311
        return _dbContext.Positions.AnyAsync(
            p => p.Name.Value.ToUpper() == name.ToUpper(), cancellationToken);
#pragma warning restore CA1862, RCS1155, MA0011, CA1304, CA1311
    }

    public Task<bool> HasDepartmentLinksAsync(Guid positionId, CancellationToken cancellationToken = default)
    {
        return _dbContext.DepartmentPositions.AnyAsync(x => x.PositionId == positionId, cancellationToken);
    }
}
