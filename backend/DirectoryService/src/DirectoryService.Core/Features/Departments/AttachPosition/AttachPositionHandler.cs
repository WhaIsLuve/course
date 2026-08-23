using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Departments;
using DirectoryService.Core.Logging;
using DirectoryService.Core.Positions;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.SharedKernel.Errors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Features.Departments.AttachPosition;

public sealed class AttachPositionHandler(
    IDepartmentRepository departmentRepository,
    IPositionRepository positionRepository,
    TimeProvider timeProvider,
    ILogger<AttachPositionHandler> logger)
    : ICommandHandler<AttachPositionCommand, UnitResult<Error>>
{
    private readonly IDepartmentRepository _departmentRepository =
        departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
    private readonly IPositionRepository _positionRepository =
        positionRepository ?? throw new ArgumentNullException(nameof(positionRepository));
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    private readonly ILogger<AttachPositionHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<UnitResult<Error>> HandleAsync(AttachPositionCommand command,
        CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetByIdAsync(command.DepartmentId, cancellationToken);
        if (department.IsFailure)
            return department.Error;

        var position = await _positionRepository.GetByIdAsync(command.PositionId, cancellationToken);
        if (position.IsFailure)
            return position.Error;

        if (await _departmentRepository.ExistDepartmentPosition(
                command.DepartmentId, command.PositionId, cancellationToken))
        {
            return Error.Conflict("department.position.exists",
                "Связь между подразделением и должностью уже существует");
        }

        var departmentPosition = DepartmentPosition.Create(
            Guid.CreateVersion7(), command.DepartmentId, command.PositionId, _timeProvider.GetUtcNow().UtcDateTime);
        if (departmentPosition.IsFailure)
            return departmentPosition.Error;

        _departmentRepository.AddDepartmentPosition(departmentPosition.Value);
        _logger.PositionAttached(command.DepartmentId, command.PositionId);
        return UnitResult.Success<Error>();
    }
}
