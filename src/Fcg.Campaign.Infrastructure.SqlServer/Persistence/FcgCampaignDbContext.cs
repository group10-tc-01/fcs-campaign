using Fcg.Campaign.Domain.Abstractions;
using Fcg.Campaign.Domain.Campaigns;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Campaign.Infrastructure.SqlServer.Persistence;

public sealed class FcgCampaignDbContext : DbContext, IUnitOfWork
{
    public FcgCampaignDbContext(DbContextOptions<FcgCampaignDbContext> options) : base(options)
    {
    }

    public DbSet<Campaign.Domain.Campaigns.Campaign> Campaigns => Set<Campaign.Domain.Campaigns.Campaign>();
    public DbSet<CampaignDonationEntry> CampaignDonationEntries => Set<CampaignDonationEntry>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FcgCampaignDbContext).Assembly);
    }
}
