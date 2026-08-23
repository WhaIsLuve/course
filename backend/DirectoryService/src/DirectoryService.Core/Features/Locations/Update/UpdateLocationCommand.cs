using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Features.Locations.Update;

public sealed record UpdateLocationCommand(Guid Id, UpdateLocationDto Dto) : ICommand<UnitResult<Error>>;
