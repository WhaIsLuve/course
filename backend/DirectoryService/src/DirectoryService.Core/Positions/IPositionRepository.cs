using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Positions;

public interface IPositionRepository
{
    void Add(Position position);

    void Remove(Position position);

    ValueTask<Result<Position, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistWithSameNameAsync(string name, CancellationToken cancellationToken = default);

    Task<bool> HasDepartmentLinksAsync(Guid positionId, CancellationToken cancellationToken = default);
}
