using fcs.Campaign.Domain.Abstractions;
using fcs.Campaign.Domain.Campaigns;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace fcs.Campaign.Infrastructure.SqlServer.Persistence;

[ExcludeFromCodeCoverage]

public sealed class FcsCampaignDbContext : DbContext, IUnitOfWork
{
    public FcsCampaignDbContext(DbContextOptions<FcsCampaignDbContext> options) : base(options)
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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FcsCampaignDbContext).Assembly);
    }
}
