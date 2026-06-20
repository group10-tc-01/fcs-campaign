using System.Diagnostics.CodeAnalysis;
using Fcs.Campaign.Application.DependencyInjection;
using Fcs.Campaign.Infrastructure.Auth.DependencyInjection;
using Fcs.Campaign.Infrastructure.Kafka.DependencyInjection;
using Fcs.Campaign.Infrastructure.SqlServer.DependencyInjection;
using Fcs.Campaign.Infrastructure.SqlServer.Persistence;
using Fcs.Campaign.WebApi.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Fcs.Campaign.WebApi;

[ExcludeFromCodeCoverage]

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddWebApi(builder.Configuration);
        builder.Services.AddApplication();
        builder.Services.AddSqlServerInfrastructure(builder.Configuration);
        builder.Services.AddKafkaInfrastructure(builder.Configuration);
        builder.Services.AddAuthInfrastructure(builder.Configuration);

        var app = builder.Build();

        if (!app.Environment.IsEnvironment("Test"))
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FcsCampaignDbContext>();
            context.Database.Migrate();
        }

        app.UseWebApiPipeline();
        app.Run();
    }
}
