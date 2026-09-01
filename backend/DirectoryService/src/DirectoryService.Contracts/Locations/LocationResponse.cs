namespace DirectoryService.Contracts.Locations;

public sealed record LocationResponse(
    Guid Id,
    string Name,
    string Country,
    string City,
    string? Street,
    string? Building,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
