namespace DirectoryService.Contracts.Locations;

public record LocationTopResponse(
    Guid Id,
    string Name,
    string Country,
    string City,
    string? Street,
    string? Building,
    int DepartmentCount
);
