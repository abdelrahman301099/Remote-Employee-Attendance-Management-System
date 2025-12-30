using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetBlaze.Domain.Entities;
using NetBlaze.SharedKernel.HelperUtilities.Constants;

namespace NetBlaze.Infrastructure.Data.Configurations.EntitiesConfigurations
{
    internal class UserAttendancePolicyConfiguration : IEntityTypeConfiguration<UserAttendancePolicy>
    {
        public void Configure(EntityTypeBuilder<UserAttendancePolicy> builder)
        {
            
            builder.ToTable("UserAttendancePolicies");

            builder
                .Property(e => e.Notes)
                .HasMaxLength(CommonStringLength.ExtraLongContentText)
                .IsRequired(false);

            builder
                .Property(e => e.EffectiveFrom)
                .IsRequired();

            builder
                .HasOne(uap => uap.User)
                .WithMany(u => u.UserAttendancePolicies)
                .HasForeignKey(uap => uap.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder
                .HasOne(uap => uap.AttendancePolicy)
                .WithMany(ap => ap.UserAttendancePolicies)
                .HasForeignKey(uap => uap.AttendancePolicyId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder
                .HasOne(uap => uap.AssignedByUser)
                .WithMany()
                .HasForeignKey(uap => uap.AssignedByUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            builder
                .HasIndex(uap => new { uap.UserId, uap.EffectiveFrom, uap.EffectiveTo })
                .HasDatabaseName("IX_UserAttendancePolicy_User_EffectiveDates");

            builder
                .HasIndex(uap => new { uap.UserId, uap.IsActive })
                .HasDatabaseName("IX_UserAttendancePolicy_User_IsActive");

            builder
                .HasIndex(uap => uap.AttendancePolicyId)
                .HasDatabaseName("IX_UserAttendancePolicy_AttendancePolicyId");
        }
    }
}

