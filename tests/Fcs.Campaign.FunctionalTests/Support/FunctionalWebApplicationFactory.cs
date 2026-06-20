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

namespace Fcs.Campaign.FunctionalTests.Support;

public sealed class FunctionalWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string AuthenticationScheme = "FunctionalTest";

    public InMemoryCampaignRepository Repository { get; } = new();
    public FakeUnitOfWork UnitOfWork { get; } = new();
    public FakeMessagePublisher Publisher { get; } = new();
    public AuthenticationState Authentication { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:SqlServer"] =
                    "Server=functional-tests-invalid-host;Database=CampaignsDb;User Id=sa;Password=Invalid123!;TrustServerCertificate=True;Connect Timeout=1;"
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
            services.AddSingleton(Authentication);
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = AuthenticationScheme;
                options.DefaultChallengeScheme = AuthenticationScheme;
                options.DefaultForbidScheme = AuthenticationScheme;
            }).AddScheme<AuthenticationSchemeOptions, FunctionalAuthenticationHandler>(AuthenticationScheme, _ => { });
        });
    }

    public sealed class AuthenticationState
    {
        public bool IsAuthenticated { get; set; } = true;
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string Role { get; set; } = "GestorONG";
    }

    private sealed class FunctionalAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly AuthenticationState _state;

        public FunctionalAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            AuthenticationState state) : base(options, logger, encoder)
        {
            _state = state;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!_state.IsAuthenticated)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, _state.UserId.ToString()),
                new Claim(ClaimTypes.Role, _state.Role)
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, AuthenticationScheme));
            var ticket = new AuthenticationTicket(principal, AuthenticationScheme);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
