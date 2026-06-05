using fcs.Campaign.Application.DependencyInjection;
using fcs.Campaign.WebApi.DependencyInjection;
using fcs.Campaign.Infrastructure.Auth.DependencyInjection;
using fcs.Campaign.Infrastructure.SqlServer.DependencyInjection;
using fcs.Campaign.Infrastructure.Kafka.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

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
        app.UseWebApiPipeline();
        app.Run();
    }
}
