using System.Text.Json;
using DirectoryService.SharedKernel.Errors;
using FluentValidation.Results;

namespace DirectoryService.Core.Extensions;

public static class ValidationResultExtensions
{
	public static ErrorMessage[] ToErrorMessages(this ValidationResult validationResult)
	{
		return validationResult.Errors.Select(f => JsonSerializer.Deserialize<Error>(f.ErrorMessage))
			.SelectMany(x => x!.Messages)
			.ToArray();
	}
}