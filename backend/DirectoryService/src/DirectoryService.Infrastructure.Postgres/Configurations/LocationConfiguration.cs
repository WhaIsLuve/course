using DirectoryService.Domain.Locations;
using DirectoryService.Infrastructure.Postgres.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

internal sealed class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");
        builder.ComplexProperty(x => x.Name,
            optBuilder => optBuilder.Property(x => x.Value).HasColumnName("name").IsRequired());
        builder.ComplexProperty(x => x.Address, optBuilder =>
        {
            optBuilder.Property(x => x.City).HasColumnName("city").IsRequired();
            optBuilder.Property(x => x.Building).HasConversion(new MaybeConverter<string>())
                .HasColumnName("building");
            optBuilder.Property(x => x.Street).HasConversion(new MaybeConverter<string>())
                .HasColumnName("street");
            optBuilder.Property(x => x.Country).HasColumnName("country").IsRequired();
        });
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt)
            .HasConversion(new MaybeConverter<DateTime>())
            .HasColumnName("updated_at");
    }
}