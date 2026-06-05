namespace fcs.Campaign.Application.Audit;

public static class AuditActions
{
    public const string CampaignCreated = nameof(CampaignCreated);
    public const string CampaignUpdated = nameof(CampaignUpdated);
    public const string CampaignCompleted = nameof(CampaignCompleted);
    public const string CampaignCanceled = nameof(CampaignCanceled);
    public const string DonationReflected = nameof(DonationReflected);
    public const string DuplicateDonationIgnored = nameof(DuplicateDonationIgnored);
}
