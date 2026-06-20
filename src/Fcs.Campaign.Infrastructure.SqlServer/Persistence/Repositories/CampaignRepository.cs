using Fcs.Campaign.Domain.Campaigns;
using Microsoft.EntityFrameworkCore;

namespace Fcs.Campaign.Infrastructure.SqlServer.Persistence.Repositories;

public sealed class CampaignRepository : ICampaignRepository
{
    private readonly FcsCampaignDbContext _dbContext;

    public CampaignRepository(FcsCampaignDbContext dbContext)
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

    public async Task<IReadOnlyList<Campaign.Domain.Campaigns.Campaign>> GetAllAsync(
        int page,
        int pageSize,
        IReadOnlyCollection<CampaignStatus>? statuses = null,
        CancellationToken cancellationToken = default)
    {
        return await ApplyStatusFilter(_dbContext.Campaigns, statuses)
            .OrderByDescending(campaign => campaign.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(
        IReadOnlyCollection<CampaignStatus>? statuses = null,
        CancellationToken cancellationToken = default)
    {
        return ApplyStatusFilter(_dbContext.Campaigns, statuses).CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Campaign.Domain.Campaigns.Campaign>> GetAllActiveAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Campaigns
            .Where(campaign => campaign.Status == CampaignStatus.Active)
            .OrderByDescending(campaign => campaign.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountActiveAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Campaigns.CountAsync(
            campaign => campaign.Status == CampaignStatus.Active,
            cancellationToken);
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

    private static IQueryable<Campaign.Domain.Campaigns.Campaign> ApplyStatusFilter(
        IQueryable<Campaign.Domain.Campaigns.Campaign> query,
        IReadOnlyCollection<CampaignStatus>? statuses)
    {
        return statuses is { Count: > 0 }
            ? query.Where(campaign => statuses.Contains(campaign.Status))
            : query;
    }
}
