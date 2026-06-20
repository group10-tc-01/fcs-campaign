using Fcs.Campaign.Domain.Campaigns;

namespace Fcs.Campaign.CommomTestsUtilities.TestDoubles;

public sealed class InMemoryCampaignRepository : ICampaignRepository
{
    private readonly List<Domain.Campaigns.Campaign> _campaigns = [];
    private readonly List<CampaignDonationEntry> _donationEntries = [];

    public IReadOnlyList<Domain.Campaigns.Campaign> Campaigns => _campaigns;
    public IReadOnlyList<CampaignDonationEntry> DonationEntries => _donationEntries;

    public Task AddAsync(Domain.Campaigns.Campaign campaign, CancellationToken cancellationToken = default)
    {
        _campaigns.Add(campaign);
        return Task.CompletedTask;
    }

    public Task<Domain.Campaigns.Campaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_campaigns.FirstOrDefault(campaign => campaign.Id == id));
    }

    public Task<IReadOnlyList<Domain.Campaigns.Campaign>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Domain.Campaigns.Campaign>>(
            _campaigns
                .OrderByDescending(campaign => campaign.CreatedAt)
                .Skip((NormalizePage(page) - 1) * NormalizePageSize(pageSize))
                .Take(NormalizePageSize(pageSize))
                .ToArray());
    }

    public Task<IReadOnlyList<Domain.Campaigns.Campaign>> GetAllActiveAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Domain.Campaigns.Campaign>>(
            _campaigns
                .Where(campaign => campaign.Status == CampaignStatus.Active)
                .OrderByDescending(campaign => campaign.CreatedAt)
                .Skip((NormalizePage(page) - 1) * NormalizePageSize(pageSize))
                .Take(NormalizePageSize(pageSize))
                .ToArray());
    }

    public Task<bool> ExistsDonationEntryAsync(Guid campaignId, Guid donationId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_donationEntries.Any(entry => entry.CampaignId == campaignId && entry.DonationId == donationId));
    }

    public Task AddDonationEntryAsync(CampaignDonationEntry entry, CancellationToken cancellationToken = default)
    {
        _donationEntries.Add(entry);
        return Task.CompletedTask;
    }

    public void Reset()
    {
        _campaigns.Clear();
        _donationEntries.Clear();
    }

    private static int NormalizePage(int page) => page < 1 ? 1 : page;
    private static int NormalizePageSize(int pageSize) => pageSize is < 1 or > 100 ? 10 : pageSize;
}
