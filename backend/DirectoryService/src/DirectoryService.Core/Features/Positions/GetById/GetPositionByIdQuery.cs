using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Positions;
using DirectoryService.Core.Abstractions;
using DirectoryService.SharedKernel.Errors;

namespace DirectoryService.Core.Features.Positions.GetById;

public sealed record GetPositionByIdQuery(Guid Id) : IQuery<Result<PositionResponse, Error>>;
