using BackendTemplate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendTemplate.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.PhoneNumber)
            .HasMaxLength(50);

        builder.HasIndex(s => s.Email);

        // Soft delete global query filter
        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
