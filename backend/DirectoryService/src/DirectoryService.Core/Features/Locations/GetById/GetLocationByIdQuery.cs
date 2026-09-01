using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Features.Locations.GetById;

public sealed record GetLocationByIdQuery(Guid Id) : IQuery<Result<LocationResponse, Error>>;
