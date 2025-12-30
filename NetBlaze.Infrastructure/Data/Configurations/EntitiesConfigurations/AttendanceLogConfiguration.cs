using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetBlaze.Domain.Entities;
using NetBlaze.SharedKernel.HelperUtilities.Constants;

namespace NetBlaze.Infrastructure.Data.Configurations.EntitiesConfigurations
{
    internal class AttendanceLogConfiguration : IEntityTypeConfiguration<AttendanceLog>
    {
        public void Configure(EntityTypeBuilder<AttendanceLog> builder)
        {
          
            builder.ToTable("AttendanceLogs");

            builder
                .Property(e => e.AttendanceDate)
                .IsRequired();

            builder
                .Property(e => e.CheckInTime)
                .IsRequired();

            builder
                .Property(e => e.CheckOutTime)
                .IsRequired(false);

            builder
                .Property(e => e.TotalMinutsWorked)
                .IsRequired();

            builder
                .Property(e => e.LateMinutes)
                .IsRequired(false);

            builder
                .Property(e => e.EarlyLeaveMinutes)
                .IsRequired(false);

            builder
                .HasOne(al => al.Employee)
                .WithMany(u => u.AttendanceLogs)
                .HasForeignKey(al => al.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder
                .HasOne(al => al.AttendancePolicy)
                .WithMany()
                .HasForeignKey(al => al.AttendancePolicyId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            

            builder
                .HasIndex(al => new { al.EmployeeId, al.AttendanceDate })
                .IsUnique()
                .HasDatabaseName("IX_AttendanceLog_Employee_Date");

            builder
                .HasIndex(al => al.AttendanceDate)
                .HasDatabaseName("IX_AttendanceLog_AttendanceDate");

            builder
                .HasIndex(al => al.AttendancePolicyId)
                .HasDatabaseName("IX_AttendanceLog_AttendancePolicyId");
        }
    }
}

