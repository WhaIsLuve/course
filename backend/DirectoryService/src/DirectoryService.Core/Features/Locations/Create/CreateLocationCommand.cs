using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Features.Locations.Create;

public sealed record CreateLocationCommand(CreateLocationDto Dto) : ICommand<Result<Guid, Error>>;
