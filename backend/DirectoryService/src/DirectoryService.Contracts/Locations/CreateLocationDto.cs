namespace DirectoryService.Contracts.Locations;

public record CreateLocationDto(string Name, string Country, string City, string? Street, string? Building);