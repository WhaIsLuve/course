using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Positions;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Core.Features.Positions.GetById;

public sealed class GetPositionByIdHandler(IReadDbContext readDbContext)
    : IQueryHandler<GetPositionByIdQuery, Result<PositionResponse, Error>>
{
    private readonly IReadDbContext _readDbContext =
        readDbContext ?? throw new ArgumentNullException(nameof(readDbContext));

    public async Task<Result<PositionResponse, Error>> HandleAsync(
        GetPositionByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var response = await _readDbContext.Positions
            .Where(position => position.Id == query.Id)
            .Select(position => new PositionResponse(
                position.Id,
                position.Name.Value,
                position.CreatedAt,
                position.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);

        return response is null
            ? Error.NotFound(
                "position.not.found",
                $"Должность с идентификатором {query.Id} не найдена")
            : response;
    }
}
