using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Core.Features.Locations.GetById;

public sealed class GetLocationByIdHandler(IReadDbContext readDbContext)
    : IQueryHandler<GetLocationByIdQuery, Result<LocationResponse, Error>>
{
    private readonly IReadDbContext _readDbContext =
        readDbContext ?? throw new ArgumentNullException(nameof(readDbContext));

    public async Task<Result<LocationResponse, Error>> HandleAsync(
        GetLocationByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var response = await _readDbContext.Locations
            .Where(location => location.Id == query.Id)
            .Select(location => new LocationResponse(
                location.Id,
                location.Name.Value,
                location.Address.Country,
                location.Address.City,
                location.Address.Street,
                location.Address.Building,
                location.CreatedAt,
                location.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);

        return response is null
            ? Error.NotFound(
                "location.not.found",
                $"Локация с идентификатором {query.Id} не найдена")
            : response;
    }
}
