namespace DirectoryService.Contracts.Positions;

public record PositionResponse(Guid Id, string Name, DateTime CreatedAt, DateTime? UpdatedAt);