using Fcg.Campaign.Application.UseCases.Campaigns;
using Fcg.Campaign.Application.UseCases.Internal.ProcessDonation;
using Fcg.Campaign.CommomTestsUtilities.Builders.Campaigns;
using Fcg.Campaign.IntegratedTests.Configurations;
using Fcg.Campaign.WebApi.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fcg.Campaign.IntegratedTests.Controllers;

public sealed class CampaignsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CampaignsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Given_ActiveCampaign_When_TransparencyEndpointIsCalled_Then_ShouldReturnCampaign()
    {
        var campaign = new CampaignBuilder().Build();
        await _factory.Repository.AddAsync(campaign);

        var response = await _client.GetAsync("/api/v1/transparency/campaigns");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<CampaignResponse>>>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.Data.Should().ContainSingle(item => item.Id == campaign.Id);
    }

    [Fact]
    public async Task Given_NewDonation_When_InternalProcessDonationIsCalledTwice_Then_ShouldBeIdempotent()
    {
        var campaign = new CampaignBuilder().Build();
        await _factory.Repository.AddAsync(campaign);
        var donationId = Guid.NewGuid();
        var request = new { DonationId = donationId, Amount = 100m, ProcessedAt = DateTime.UtcNow };

        var firstResponse = await _client.PostAsJsonAsync($"/internal/campaigns/{campaign.Id}/donation-processed", request);
        var secondResponse = await _client.PostAsJsonAsync($"/internal/campaigns/{campaign.Id}/donation-processed", request);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        campaign.TotalAmountRaised.Should().Be(100);

        var secondPayload = await secondResponse.Content.ReadFromJsonAsync<ApiResponse<ProcessDonationResponse>>();
        secondPayload!.Data!.Duplicate.Should().BeTrue();
    }
}
