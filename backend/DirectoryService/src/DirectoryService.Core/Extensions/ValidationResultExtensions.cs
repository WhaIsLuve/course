using DirectoryService.SharedKernel.Errors;
using FluentValidation.Results;

namespace DirectoryService.Core.Extensions;

public static class ValidationResultExtensions
{
	public static ErrorMessage[] ToErrorMessages(this ValidationResult validationResult)
	{
		return validationResult.Errors.Select(f => new ErrorMessage(f.ErrorCode, f.ErrorMessage, f.PropertyName))
			.ToArray();
	}
}