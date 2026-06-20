using System.Diagnostics.CodeAnalysis;
using Fcs.Campaign.Infrastructure.SqlServer.Persistence;
using Fcs.Campaign.WebApi.Middlewares;
using Microsoft.EntityFrameworkCore;

namespace Fcs.Campaign.WebApi.Extensions;

[ExcludeFromCodeCoverage]
public static class ApiBuilderExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<FcsCampaignDbContext>();

        dbContext.Database.Migrate();
    }

    public static void UseCustomerExceptionHandler(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
    }

    public static void UseRequestFlowLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestFlowLoggingMiddleware>();
    }

    public static void UseGlobalCorrelationId(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalCorrelationIdMiddleware>();
    }
}
