using Fcg.Campaign.Domain.Abstractions;
using Fcg.Campaign.Domain.Campaigns;
using Fcg.Campaign.Infrastructure.SqlServer.Persistence;
using Fcg.Campaign.Infrastructure.SqlServer.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Campaign.Infrastructure.SqlServer.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddSqlServerInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FcgCampaignDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("SqlServer")));

        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FcgCampaignDbContext>());
        services.AddHealthChecks().AddDbContextCheck<FcgCampaignDbContext>("sqlserver");

        return services;
    }
}
