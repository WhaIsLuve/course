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

	[LoggerMessage(EventId = 1207, Level = LogLevel.Information,
		Message = "Создана должность: {PositionId}")]
	internal static partial void PositionCreated(this ILogger logger, Guid positionId);

	[LoggerMessage(EventId = 1208, Level = LogLevel.Information,
		Message = "Переименована должность: {PositionId}")]
	internal static partial void PositionRenamed(this ILogger logger, Guid positionId);

	[LoggerMessage(EventId = 1209, Level = LogLevel.Information,
		Message = "Удалена должность: {PositionId}")]
	internal static partial void PositionDeleted(this ILogger logger, Guid positionId);

	[LoggerMessage(EventId = 1210, Level = LogLevel.Information,
		Message = "Удалена локация: {LocationId}")]
	internal static partial void LocationDeleted(this ILogger logger, Guid locationId);

	[LoggerMessage(EventId = 1211, Level = LogLevel.Information,
		Message = "Удалено подразделение: {DepartmentId}")]
	internal static partial void DepartmentDeleted(this ILogger logger, Guid departmentId);

	[LoggerMessage(EventId = 1212, Level = LogLevel.Information,
		Message = "К подразделению привязана должность: {DepartmentId}, {PositionId}")]
	internal static partial void PositionAttached(this ILogger logger, Guid departmentId, Guid positionId);

	[LoggerMessage(EventId = 1213, Level = LogLevel.Information,
		Message = "От подразделения отвязана должность: {DepartmentId}, {PositionId}")]
	internal static partial void PositionDetached(this ILogger logger, Guid departmentId, Guid positionId);
}
