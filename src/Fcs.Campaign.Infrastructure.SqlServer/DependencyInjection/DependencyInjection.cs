using System.Diagnostics.CodeAnalysis;
using Fcs.Campaign.Domain.Abstractions;
using Fcs.Campaign.Domain.Campaigns;
using Fcs.Campaign.Infrastructure.SqlServer.Persistence;
using Fcs.Campaign.Infrastructure.SqlServer.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fcs.Campaign.Infrastructure.SqlServer.DependencyInjection;

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
