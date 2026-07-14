using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DirectoryService.Infrastructure.Postgres.Converters;

public class MaybeConverter<T>() : ValueConverter<Maybe<T>, T?>(x => x.GetValueOrDefault(), v => Maybe<T>.From(v));