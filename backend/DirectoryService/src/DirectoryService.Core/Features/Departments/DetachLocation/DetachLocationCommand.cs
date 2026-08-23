using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Features.Departments.DetachLocation;

public sealed record DetachLocationCommand(Guid DepartmentId, Guid LocationId) : ICommand<UnitResult<Error>>;
