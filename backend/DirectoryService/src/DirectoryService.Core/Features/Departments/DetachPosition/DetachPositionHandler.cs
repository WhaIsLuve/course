using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Departments;
using DirectoryService.Core.Logging;
using DirectoryService.Core.Positions;
using DirectoryService.SharedKernel.Errors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Features.Departments.DetachPosition;

public sealed class DetachPositionHandler(
    IDepartmentRepository departmentRepository,
    IPositionRepository positionRepository,
    ILogger<DetachPositionHandler> logger)
    : ICommandHandler<DetachPositionCommand, UnitResult<Error>>
{
    private readonly IDepartmentRepository _departmentRepository =
        departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
    private readonly IPositionRepository _positionRepository =
        positionRepository ?? throw new ArgumentNullException(nameof(positionRepository));
    private readonly ILogger<DetachPositionHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<UnitResult<Error>> HandleAsync(DetachPositionCommand command,
        CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetByIdAsync(command.DepartmentId, cancellationToken);
        if (department.IsFailure)
            return department.Error;

        var position = await _positionRepository.GetByIdAsync(command.PositionId, cancellationToken);
        if (position.IsFailure)
            return position.Error;

        var departmentPosition = await _departmentRepository.GetDepartmentPosition(
            command.DepartmentId, command.PositionId, cancellationToken);
        if (departmentPosition.IsFailure)
            return departmentPosition.Error;

        _departmentRepository.RemoveDepartmentPosition(departmentPosition.Value);
        _logger.PositionDetached(command.DepartmentId, command.PositionId);
        return UnitResult.Success<Error>();
    }
}
