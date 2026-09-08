namespace DirectoryService.Contracts.Locations;

public sealed record LocationListItemDto(
    Guid Id,
    string Name,
    string Country,
    string City,
    string? Street,
    string? Building,
    DateTime CreatedAt,
    int DepartmentCount);
