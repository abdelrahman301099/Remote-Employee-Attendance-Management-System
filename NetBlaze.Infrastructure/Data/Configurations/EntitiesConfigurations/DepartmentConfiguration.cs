using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetBlaze.Domain.Entities;
using NetBlaze.SharedKernel.HelperUtilities.Constants;

namespace NetBlaze.Infrastructure.Data.Configurations.EntitiesConfigurations
{
    internal class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
          
            builder.ToTable("Departments");

            builder
                .Property(e => e.DepartmentName)
                .HasMaxLength(CommonStringLength.StandardText)
                .IsRequired();

            builder
                .Property(e => e.Description)
                .HasMaxLength(CommonStringLength.LongText)
                .IsRequired(false);

            builder
                .Property(e => e.ManagerId)
                .IsRequired(false);


            builder
                .HasOne(d => d.Manager)
                .WithMany()
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            builder
                .HasMany(d => d.Employees)
                .WithOne(u => u.Department)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);


            builder
                .HasIndex(d => d.DepartmentName)
                .IsUnique()
                .HasDatabaseName("IX_Department_DepartmentName");

            builder
                .HasIndex(d => d.ManagerId)
                .HasDatabaseName("IX_Department_ManagerId");
        }
    }
}

