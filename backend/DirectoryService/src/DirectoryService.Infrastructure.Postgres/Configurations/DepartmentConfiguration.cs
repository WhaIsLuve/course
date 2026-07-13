using DirectoryService.Domain.Departments;
using DirectoryService.Infrastructure.Postgres.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

internal sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever()
            .HasColumnName("id");
        builder.ComplexProperty(x => x.Name,
            optBuilder => optBuilder.Property(x => x.Value).HasColumnName("name").IsRequired());
        builder.ComplexProperty(x => x.Slug,
            optBuilder => optBuilder.Property(x => x.Value).HasColumnName("slug").IsRequired());
        builder.ComplexProperty(x => x.Path,
            optBuilder => optBuilder.Property(x => x.Value).HasColumnName("path").IsRequired());
        builder.Property(x => x.ParentId).HasColumnName("parent_id");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt)
            .HasConversion(new MaybeConverter<DateTime>())
            .HasColumnName("updated_at");
        builder.HasMany<Department>()
            .WithOne()
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}