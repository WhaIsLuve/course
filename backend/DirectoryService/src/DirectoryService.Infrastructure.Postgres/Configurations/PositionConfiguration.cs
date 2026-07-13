using DirectoryService.Domain.Positions;
using DirectoryService.Infrastructure.Postgres.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

internal sealed class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever()
            .HasColumnName("position_id");
        builder.ComplexProperty(x => x.Name,
            optBuilder => optBuilder.Property(x => x.Value).HasColumnName("name").IsRequired());
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt)
            .HasConversion(new MaybeConverter<DateTime>())
            .HasColumnName("updated_at");
    }
}