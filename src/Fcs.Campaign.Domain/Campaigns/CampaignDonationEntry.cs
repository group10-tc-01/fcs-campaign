using Fcs.Campaign.Domain.Abstractions;
using Fcs.Campaign.Domain.Results;

namespace Fcs.Campaign.Domain.Campaigns;

public sealed class CampaignDonationEntry : BaseEntity
{
    private CampaignDonationEntry()
    {
    }

    private CampaignDonationEntry(Guid id, Guid campaignId, Guid donationId, decimal amount, DateTime processedAt) : base(id)
    {
        CampaignId = campaignId;
        DonationId = donationId;
        Amount = amount;
        ProcessedAt = processedAt;
    }

    public Guid CampaignId { get; private set; }
    public Guid DonationId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime ProcessedAt { get; private set; }

    public Campaign Campaign { get; private set; } = null!;

    public static Result<CampaignDonationEntry> Create(Guid campaignId, Guid donationId, decimal amount, DateTime processedAt)
    {
        if (campaignId == Guid.Empty)
        {
            return Error.Validation("CampaignDonationEntry.CampaignRequired", "Campaign id is required.");
        }

        if (donationId == Guid.Empty)
        {
            return Error.Validation("CampaignDonationEntry.DonationRequired", "Donation id is required.");
        }

        if (amount <= 0)
        {
            return Error.Validation("CampaignDonationEntry.InvalidAmount", "Donation amount must be greater than zero.");
        }

        return new CampaignDonationEntry(Guid.NewGuid(), campaignId, donationId, amount, processedAt);
    }
}
