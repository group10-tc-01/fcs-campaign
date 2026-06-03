using Fcg.Campaign.Application.DependencyInjection;
using Fcg.Campaign.WebApi.DependencyInjection;
using Fcg.Campaign.Infrastructure.Auth.DependencyInjection;
using Fcg.Campaign.Infrastructure.SqlServer.DependencyInjection;
using Fcg.Campaign.Infrastructure.Kafka.DependencyInjection;

namespace Fcg.Campaign.WebApi;

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
