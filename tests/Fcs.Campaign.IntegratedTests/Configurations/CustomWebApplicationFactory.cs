using System.Security.Claims;
using System.Text.Encodings.Web;
using Fcs.Campaign.Application.Abstractions.Messaging;
using Fcs.Campaign.CommomTestsUtilities.TestDoubles;
using Fcs.Campaign.Domain.Abstractions;
using Fcs.Campaign.Domain.Campaigns;
using Fcs.Campaign.WebApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fcs.Campaign.IntegratedTests.Configurations;

public sealed record RoleClaimValue(string Role);

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string AuthenticationScheme = "Test";

    public InMemoryCampaignRepository Repository { get; } = new();
    public FakeUnitOfWork UnitOfWork { get; } = new();
    public FakeMessagePublisher Publisher { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:SqlServer"] =
                    "Server=invalid-campaign-test-host;Database=CampaignsDb;User Id=sa;Password=Invalid123!;TrustServerCertificate=True;Connect Timeout=1;"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ICampaignRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.RemoveAll<IMessagePublisher>();

            services.AddSingleton<ICampaignRepository>(Repository);
            services.AddSingleton<IUnitOfWork>(UnitOfWork);
            services.AddSingleton<IMessagePublisher>(Publisher);
            services.AddSingleton(new RoleClaimValue("GestorONG"));
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = AuthenticationScheme;
                options.DefaultChallengeScheme = AuthenticationScheme;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(AuthenticationScheme, _ => { });
        });
    }

    public HttpClient CreateClientWithRole(string role)
    {
        return WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<RoleClaimValue>();
                services.AddSingleton(new RoleClaimValue(role));
            });
        }).CreateClient();
    }

    private sealed class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly RoleClaimValue _roleClaim;

        public TestAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            RoleClaimValue roleClaim) : base(options, logger, encoder)
        {
            _roleClaim = roleClaim;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, _roleClaim.Role)
            };
            var identity = new ClaimsIdentity(claims, AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
