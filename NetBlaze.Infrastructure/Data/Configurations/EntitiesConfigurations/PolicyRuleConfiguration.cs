using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetBlaze.Domain.Entities;
using NetBlaze.SharedKernel.HelperUtilities.Constants;
using NetBlaze.SharedKernel.Enums;


namespace NetBlaze.Infrastructure.Data.Configurations.EntitiesConfigurations
{
    internal class PolicyRuleConfiguration : IEntityTypeConfiguration<PolicyRule>
    {
        public void Configure(EntityTypeBuilder<PolicyRule> builder)
        {
            
            builder.ToTable("PolicyRules");

            builder
                .Property(e => e.RuleType)
                .HasMaxLength(CommonStringLength.StandardText)
                .IsRequired(false);

            builder
                .Property(e => e.Condition)
                .HasMaxLength(CommonStringLength.LongText)
                .IsRequired();

            builder
                .Property(e => e.Action)
                .HasMaxLength(CommonStringLength.LongText)
                .IsRequired();

            builder
                .Property(e => e.PenaltyValue)
                .IsRequired();

            builder
                .Property(e => e.Description)
                .HasMaxLength(CommonStringLength.LongText)
                .IsRequired(false);

            builder
                .Property(e => e.RuleType)
                .IsRequired();

            builder
                .HasOne(pr => pr.AttendancePolicy)
                .WithMany(ap => ap.PolicyRules)
                .HasForeignKey(pr => pr.AttendancePolicyId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder
                .HasIndex(pr => pr.AttendancePolicyId)
                .HasDatabaseName("IX_PolicyRule_AttendancePolicyId");

            builder
                .HasIndex(pr => pr.RuleType)
                .HasDatabaseName("IX_PolicyRule_RuleType");

            builder
                .HasIndex(pr => new { pr.AttendancePolicyId, pr.RuleType })
                .HasDatabaseName("IX_PolicyRule_Policy_RuleType");
        }
    }
}

