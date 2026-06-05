using fcs.Campaign.Application.Abstractions.Messaging;
using fcs.Campaign.CommomTestsUtilities.TestDoubles;
using fcs.Campaign.Domain.Abstractions;
using fcs.Campaign.Domain.Campaigns;
using fcs.Campaign.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace fcs.Campaign.IntegratedTests.Configurations;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public InMemoryCampaignRepository Repository { get; } = new();
    public FakeUnitOfWork UnitOfWork { get; } = new();
    public FakeMessagePublisher Publisher { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ICampaignRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.RemoveAll<IMessagePublisher>();

            services.AddSingleton<ICampaignRepository>(Repository);
            services.AddSingleton<IUnitOfWork>(UnitOfWork);
            services.AddSingleton<IMessagePublisher>(Publisher);
        });
    }
}
