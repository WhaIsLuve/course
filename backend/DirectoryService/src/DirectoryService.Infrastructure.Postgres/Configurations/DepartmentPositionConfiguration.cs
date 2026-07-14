using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

internal sealed class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_positions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
               .ValueGeneratedNever()
               .HasColumnName("id");
        builder.Property(x => x.DepartmentId)
               .HasColumnName("department_id");
        builder.Property(x => x.PositionId)
               .HasColumnName("position_id");
        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at");
        builder.HasOne<Department>()
               .WithMany()
               .HasForeignKey(x => x.DepartmentId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Position>()
               .WithMany()
               .HasForeignKey(x => x.PositionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}