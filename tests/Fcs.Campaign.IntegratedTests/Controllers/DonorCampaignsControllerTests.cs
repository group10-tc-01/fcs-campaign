using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fcs.Campaign.Application.UseCases.Campaigns.ActiveDonorCampaigns;
using Fcs.Campaign.CommomTestsUtilities.Builders.Campaigns;
using Fcs.Campaign.IntegratedTests.Configurations;
using Fcs.Campaign.WebApi.Models;
using FluentAssertions;

namespace Fcs.Campaign.IntegratedTests.Controllers;

public sealed class DonorCampaignsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _gestorClient;

    public DonorCampaignsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.Repository.Reset();
        _gestorClient = factory.CreateClient();
    }

    [Fact]
    public async Task Given_ActiveCampaigns_When_GetActiveWithDoadorRole_Then_ShouldReturnOnlyActive()
    {
        using var doadorClient = _factory.CreateClientWithRole("Doador");
        var active = new CampaignBuilder().Build();
        var completed = new CampaignBuilder().WithFinancialGoal(2000).Build();
        completed.Complete();
        await _factory.Repository.AddAsync(active);
        await _factory.Repository.AddAsync(completed);

        var response = await doadorClient.GetAsync("/api/v1/campaigns/active");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<ActiveDonorCampaignResponse>>>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.Data.Should().HaveCount(1);
        payload.Data.First().Id.Should().Be(active.Id);
    }

    [Fact]
    public async Task Given_NoActiveCampaigns_When_GetActiveWithDoadorRole_Then_ShouldReturnEmpty()
    {
        using var doadorClient = _factory.CreateClientWithRole("Doador");
        var completed = new CampaignBuilder().Build();
        completed.Complete();
        await _factory.Repository.AddAsync(completed);

        var response = await doadorClient.GetAsync("/api/v1/campaigns/active");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<ActiveDonorCampaignResponse>>>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_GestorRole_When_GetActive_Then_ShouldReturnForbidden()
    {
        var response = await _gestorClient.GetAsync("/api/v1/campaigns/active");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
