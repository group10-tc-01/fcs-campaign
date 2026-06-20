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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fcs.Campaign.IntegratedTests.Configurations;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string AuthenticationScheme = "Test";

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

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = AuthenticationScheme;
                options.DefaultChallengeScheme = AuthenticationScheme;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(AuthenticationScheme, _ => { });
        });
    }

    private sealed class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder) : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "GestorONG")
            };
            var identity = new ClaimsIdentity(claims, AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
