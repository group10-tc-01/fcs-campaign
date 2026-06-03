using Fcg.Campaign.Domain.Campaigns;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fcg.Campaign.Infrastructure.SqlServer.Persistence.Configurations;

public sealed class CampaignDonationEntryConfiguration : BaseConfiguration<CampaignDonationEntry>
{
    public override void Configure(EntityTypeBuilder<CampaignDonationEntry> builder)
    {
        base.Configure(builder);

        builder.ToTable("CampaignDonationEntries", table =>
        {
            table.HasCheckConstraint("CK_CampaignDonationEntries_Amount_GreaterThanZero", "[Amount] > 0");
        });
        builder.Property(entry => entry.CampaignId).IsRequired();
        builder.Property(entry => entry.DonationId).IsRequired();
        builder.Property(entry => entry.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(entry => entry.ProcessedAt).IsRequired();

        builder.HasOne(entry => entry.Campaign)
            .WithMany()
            .HasForeignKey(entry => entry.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entry => new { entry.CampaignId, entry.DonationId }).IsUnique();
        builder.HasIndex(entry => entry.DonationId);
    }
}
