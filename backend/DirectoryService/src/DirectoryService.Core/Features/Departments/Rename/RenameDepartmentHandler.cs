using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Departments;
using DirectoryService.Core.Extensions;
using DirectoryService.Core.Logging;
using DirectoryService.Domain.Departments;
using DirectoryService.SharedKernel.Errors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Features.Departments.Rename;

public sealed class RenameDepartmentHandler(
    IValidator<UpdateDepartmentNameDto> validator,
    IDepartmentRepository departmentRepository,
    TimeProvider timeProvider,
    ILogger<RenameDepartmentHandler> logger)
    : ICommandHandler<RenameDepartmentCommand, UnitResult<Error>>
{
    private readonly IValidator<UpdateDepartmentNameDto> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly IDepartmentRepository _departmentRepository =
        departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    private readonly ILogger<RenameDepartmentHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<UnitResult<Error>> HandleAsync(
        RenameDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command.Dto, cancellationToken);
        if (!validationResult.IsValid)
            return Error.Validation(validationResult.ToErrorMessages());

        var department = await _departmentRepository.GetByIdAsync(command.DepartmentId, cancellationToken);
        if (department.IsFailure)
            return department.Error;

        var newName = DepartmentName.Create(command.Dto.Name);
        if (newName.IsFailure)
            return newName.Error;

        var result = department.Value.UpdateName(newName.Value, _timeProvider.GetUtcNow().UtcDateTime);
        if (result.IsFailure)
            return result.Error;

        result = await _departmentRepository.Save(cancellationToken);
        if (result.IsFailure)
            return result.Error;

        _logger.DepartmentRenamed(command.DepartmentId);
        return UnitResult.Success<Error>();
    }
}
