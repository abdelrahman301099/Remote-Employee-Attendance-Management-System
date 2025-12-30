using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetBlaze.Domain.Entities;
using NetBlaze.SharedKernel.HelperUtilities.Constants;

namespace NetBlaze.Infrastructure.Data.Configurations.EntitiesConfigurations
{
    internal class AttendancePolicyConfiguration : IEntityTypeConfiguration<AttendancePolicy>
    {
        public void Configure(EntityTypeBuilder<AttendancePolicy> builder)
        {
            
            builder.ToTable("AttendancePolicies");

            builder
                .Property(e => e.PolicyName)
                .HasMaxLength(CommonStringLength.StandardText)
                .IsRequired();

            builder
                .Property(e => e.WorkingHoursStart)
                .IsRequired();

            builder
                .Property(e => e.WorkingHoursEnd)
                .IsRequired();

            builder
                .Property(e => e.GracePeriodInMinutes)
                .IsRequired();

            builder
                .Property(e => e.latePenaltyInMinutes)
                .IsRequired();

            builder
                .Property(e => e.MinRandomCheckInPerDay)
                .IsRequired();

            builder
                .Property(e => e.MaxRandomCheckInPerDay)
                .IsRequired();

            builder
                .Property(e => e.EffectiveFrom)
                .IsRequired();

            builder
                .Property(e => e.EffectiveTo)
                .IsRequired();

            builder
                .HasIndex(ap => new { ap.EffectiveFrom, ap.EffectiveTo })
                .HasDatabaseName("IX_AttendancePolicy_EffectiveDates");

            builder
                .HasIndex(ap => ap.PolicyName)
                .HasDatabaseName("IX_AttendancePolicy_PolicyName");
        }
    }
}

