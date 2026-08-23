using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Features.Departments.AttachLocation;

public sealed record AttachLocationCommand(Guid DepartmentId, Guid LocationId) : ICommand<UnitResult<Error>>;
