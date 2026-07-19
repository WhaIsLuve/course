namespace DirectoryService.Contracts.Locations;

public record LocationResponse(Guid Id, string Name, AddressResponse Address, DateTime CreatedAt, DateTime? UpdatedAt);