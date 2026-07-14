using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

internal sealed class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>
{
    public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        builder.ToTable("department_locations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
               .ValueGeneratedNever()
               .HasColumnName("id");
        builder.Property(x => x.IsPrimary)
               .HasColumnName("is_primary");
        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at");
        builder.Property(x => x.LocationId)
               .HasColumnName("location_id");
        builder.Property(x => x.DepartmentId)
               .HasColumnName("department_id");
        builder.HasOne<Department>()
               .WithMany()
               .HasForeignKey(x => x.DepartmentId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Location>()
               .WithMany()
               .HasForeignKey(x => x.LocationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}