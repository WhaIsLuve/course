namespace DirectoryService.SharedKernel.Errors;

public record ErrorMessage(string Code, string Message, string? InvalidField = null);