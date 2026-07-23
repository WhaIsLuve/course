using DirectoryService.Domain.Locations;
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
			optBuilder => optBuilder.Property(x => x.Value)
				.HasMaxLength(LocationName.MaxLength)
				.HasColumnName("name")
				.IsRequired());
		builder.ComplexProperty(x => x.Address, optBuilder =>
		{
			optBuilder.Property(x => x.City)
				.HasMaxLength(Address.CityMaxLength)
				.HasColumnName("city")
				.IsRequired();
			optBuilder.Property(x => x.Building)
				.HasMaxLength(Address.BuildingMaxLength)
				.HasColumnName("building");
			optBuilder.Property(x => x.Street)
				.HasMaxLength(Address.StreetMaxLength)
				.HasColumnName("street");
			optBuilder.Property(x => x.Country)
				.HasMaxLength(Address.CountryMaxLength)
				.HasColumnName("country")
				.IsRequired();
		});
		builder.Property(x => x.CreatedAt)
			.HasColumnName("created_at");
		builder.Property(x => x.UpdatedAt)
			.HasColumnName("updated_at");
	}
}