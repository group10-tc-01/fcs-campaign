namespace Fcs.Campaign.Domain.Campaigns;

public interface ICampaignRepository
{
    Task AddAsync(Campaign campaign, CancellationToken cancellationToken = default);
    Task<Campaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Campaign>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Campaign>> GetAllActiveAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> ExistsDonationEntryAsync(Guid campaignId, Guid donationId, CancellationToken cancellationToken = default);
    Task AddDonationEntryAsync(CampaignDonationEntry entry, CancellationToken cancellationToken = default);
}
