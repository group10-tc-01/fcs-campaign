using System.Diagnostics.CodeAnalysis;
using fcs.Campaign.Application.DependencyInjection;
using fcs.Campaign.Infrastructure.Auth.DependencyInjection;
using fcs.Campaign.Infrastructure.Kafka.DependencyInjection;
using fcs.Campaign.Infrastructure.SqlServer.DependencyInjection;
using fcs.Campaign.Infrastructure.SqlServer.Persistence;
using fcs.Campaign.WebApi.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace fcs.Campaign.WebApi;

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
