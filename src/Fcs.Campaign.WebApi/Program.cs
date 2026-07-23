using System.Diagnostics.CodeAnalysis;
using Fcs.Campaign.Application.DependencyInjection;
using Fcs.Campaign.Infrastructure.Auth.DependencyInjection;
using Fcs.Campaign.Infrastructure.Kafka.DependencyInjection;
using Fcs.Campaign.Infrastructure.SqlServer.DependencyInjection;
using Fcs.Campaign.WebApi.DependencyInjection;

namespace Fcs.Campaign.WebApi;

// trigger CI/CD demo build
[ExcludeFromCodeCoverage]

public class Program
{
    protected Program() { }

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddWebApi(builder.Configuration);
        builder.Services.AddApplication();
        builder.Services.AddSqlServerInfrastructure(builder.Configuration);
        builder.Services.AddKafkaInfrastructure(builder.Configuration);
        builder.Services.AddAuthInfrastructure(builder.Configuration);

        var app = builder.Build();
        app.UseWebApiPipeline();
        app.Run();
    }
}
