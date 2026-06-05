using fcs.Campaign.Domain.Abstractions;
using fcs.Campaign.Domain.Campaigns;
using fcs.Campaign.Infrastructure.SqlServer.Persistence;
using fcs.Campaign.Infrastructure.SqlServer.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace fcs.Campaign.Infrastructure.SqlServer.DependencyInjection;

[ExcludeFromCodeCoverage]

public static class DependencyInjection
{
    public static IServiceCollection AddSqlServerInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FcsCampaignDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("SqlServer")));

        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FcsCampaignDbContext>());
        services.AddHealthChecks().AddDbContextCheck<FcsCampaignDbContext>("sqlserver");

        return services;
    }
}
