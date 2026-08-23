using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Departments;
using DirectoryService.Core.Logging;
using DirectoryService.SharedKernel.Errors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Features.Departments.Delete;

public sealed class DeleteDepartmentHandler(
    IDepartmentRepository departmentRepository,
    ILogger<DeleteDepartmentHandler> logger)
    : ICommandHandler<DeleteDepartmentCommand, UnitResult<Error>>
{
    private readonly IDepartmentRepository _departmentRepository =
        departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
    private readonly ILogger<DeleteDepartmentHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<UnitResult<Error>> HandleAsync(DeleteDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetByIdAsync(command.DepartmentId, cancellationToken);
        if (department.IsFailure)
            return department.Error;

        _departmentRepository.RemoveDepartment(department.Value);
        _logger.DepartmentDeleted(command.DepartmentId);
        return UnitResult.Success<Error>();
    }
}
