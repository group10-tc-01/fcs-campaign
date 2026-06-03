using Fcg.Campaign.Domain.Campaigns;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Campaign.Infrastructure.SqlServer.Persistence.Repositories;

public sealed class CampaignRepository : ICampaignRepository
{
    private readonly FcgCampaignDbContext _dbContext;

    public CampaignRepository(FcgCampaignDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Campaign.Domain.Campaigns.Campaign campaign, CancellationToken cancellationToken = default)
    {
        await _dbContext.Campaigns.AddAsync(campaign, cancellationToken);
    }

    public Task<Campaign.Domain.Campaigns.Campaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Campaigns.FirstOrDefaultAsync(campaign => campaign.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Campaign.Domain.Campaigns.Campaign>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Campaigns
            .OrderByDescending(campaign => campaign.CreatedAt)
            .Skip((NormalizePage(page) - 1) * NormalizePageSize(pageSize))
            .Take(NormalizePageSize(pageSize))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Campaign.Domain.Campaigns.Campaign>> GetAllActiveAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Campaigns
            .Where(campaign => campaign.Status == CampaignStatus.Active)
            .OrderByDescending(campaign => campaign.CreatedAt)
            .Skip((NormalizePage(page) - 1) * NormalizePageSize(pageSize))
            .Take(NormalizePageSize(pageSize))
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsDonationEntryAsync(Guid campaignId, Guid donationId, CancellationToken cancellationToken = default)
    {
        return _dbContext.CampaignDonationEntries
            .AnyAsync(entry => entry.CampaignId == campaignId && entry.DonationId == donationId, cancellationToken);
    }

    public async Task AddDonationEntryAsync(CampaignDonationEntry entry, CancellationToken cancellationToken = default)
    {
        await _dbContext.CampaignDonationEntries.AddAsync(entry, cancellationToken);
    }

    private static int NormalizePage(int page) => page < 1 ? 1 : page;
    private static int NormalizePageSize(int pageSize) => pageSize is < 1 or > 100 ? 10 : pageSize;
}
