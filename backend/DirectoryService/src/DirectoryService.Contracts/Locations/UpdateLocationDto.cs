namespace DirectoryService.Contracts.Locations;

public record UpdateLocationDto(string Name, string Country, string City, string? Street, string? Building);