using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Features.Locations.Top;

public record GetTopLocationQuery() : IQuery<Result<LocationTopResponse[], Error>>;