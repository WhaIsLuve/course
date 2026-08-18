using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Logging;

internal static partial class DirectoryServiceLogMessages
{
	[LoggerMessage(EventId = 1201, Level = LogLevel.Information,
		Message = "Создана локация: {LocationId}")]
	internal static partial void LocationCreated(this ILogger logger, Guid locationId);

	[LoggerMessage(EventId = 1202, Level = LogLevel.Information,
		Message = "Обновлена локация: {LocationId}")]
	internal static partial void LocationUpdated(this ILogger logger, Guid locationId);

	[LoggerMessage(EventId = 1203, Level = LogLevel.Information,
		Message = "Создано подразделение: {DepartmentId}, родительское подразделение {ParentDepartmentId}, локации {LocationIds}")]
	internal static partial void DepartmentCreated(this ILogger logger, Guid departmentId,
		Guid? parentDepartmentId, IReadOnlyCollection<Guid> locationIds);

	[LoggerMessage(EventId = 1204, Level = LogLevel.Information,
		Message = "Переименовано подразделение: {DepartmentId}")]
	internal static partial void DepartmentRenamed(this ILogger logger, Guid departmentId);

	[LoggerMessage(EventId = 1205, Level = LogLevel.Information,
		Message = "К подразделению привязана локация: {DepartmentId}, {LocationId}")]
	internal static partial void LocationAttached(this ILogger logger, Guid departmentId, Guid locationId);

	[LoggerMessage(EventId = 1206, Level = LogLevel.Information,
		Message = "От подразделения отвязана локация: {DepartmentId}, {LocationId}")]
	internal static partial void LocationDetached(this ILogger logger, Guid departmentId, Guid locationId);
}
