namespace DirectoryService.Domain;

public interface ISoftDeletable
{
    bool IsDeleted { get; }

    DateTime? DeletedAt { get; }
}
