using fcs.Campaign.Domain.Campaigns;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace fcs.Campaign.Infrastructure.SqlServer.Persistence.Configurations;

[ExcludeFromCodeCoverage]

public sealed class CampaignConfiguration : BaseConfiguration<Campaign.Domain.Campaigns.Campaign>
{
    public override void Configure(EntityTypeBuilder<Campaign.Domain.Campaigns.Campaign> builder)
    {
        base.Configure(builder);

        builder.ToTable("Campaigns", table =>
        {
            table.HasCheckConstraint("CK_Campaigns_FinancialGoal_GreaterThanZero", "[FinancialGoal] > 0");
            table.HasCheckConstraint("CK_Campaigns_TotalAmountRaised_GreaterOrEqualZero", "[TotalAmountRaised] >= 0");
        });
        builder.Property(campaign => campaign.Title).HasMaxLength(200).IsRequired();
        builder.Property(campaign => campaign.Description).HasMaxLength(2000).IsRequired();
        builder.Property(campaign => campaign.StartDate).IsRequired();
        builder.Property(campaign => campaign.EndDate).IsRequired();
        builder.Property(campaign => campaign.FinancialGoal).HasPrecision(18, 2).IsRequired();
        builder.Property(campaign => campaign.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(campaign => campaign.TotalAmountRaised).HasPrecision(18, 2).IsRequired();
        builder.Property(campaign => campaign.CreatedByManagerId).IsRequired();

        builder.HasIndex(campaign => campaign.Status);
        builder.HasIndex(campaign => campaign.CreatedByManagerId);
    }
}
