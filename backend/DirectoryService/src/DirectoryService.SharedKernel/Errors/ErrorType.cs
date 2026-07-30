using System.Text.Json.Serialization;

namespace DirectoryService.SharedKernel.Errors;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ErrorType
{
	Validation,
	NotFound,
	Failure,
	Conflict
}