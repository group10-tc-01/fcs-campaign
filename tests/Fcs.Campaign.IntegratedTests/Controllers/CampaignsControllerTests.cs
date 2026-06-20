using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fcs.Campaign.Application.UseCases.Campaigns;
using Fcs.Campaign.Application.UseCases.Internal.ProcessDonation;
using Fcs.Campaign.Application.UseCases.Transparency.GetTransparencyCampaigns;
using Fcs.Campaign.CommomTestsUtilities.Builders.Campaigns;
using Fcs.Campaign.Domain.Campaigns;
using Fcs.Campaign.IntegratedTests.Configurations;
using Fcs.Campaign.WebApi.Models;
using FluentAssertions;

namespace Fcs.Campaign.IntegratedTests.Controllers;

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
    public async Task Given_ValidRequest_When_CreateCampaignIsCalled_Then_ShouldReturnCreatedCampaign()
    {
        var request = new
        {
            Title = "Food basket",
            Description = "Monthly food support",
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(10),
            FinancialGoal = 500m
        };

        var response = await _client.PostAsJsonAsync("/api/v1/campaigns", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.Data!.Title.Should().Be(request.Title);
        payload.Data.Status.Should().Be(CampaignStatus.Active);
    }

    [Fact]
    public async Task Given_Campaigns_When_GetAllCampaignsIsCalled_Then_ShouldReturnCampaigns()
    {
        var campaign = new CampaignBuilder().Build();
        await _factory.Repository.AddAsync(campaign);

        var response = await _client.GetAsync("/api/v1/campaigns");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<CampaignResponse>>>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.Data.Should().Contain(item => item.Id == campaign.Id);
    }

    [Fact]
    public async Task Given_ExistingCampaign_When_UpdateCampaignIsCalled_Then_ShouldReturnUpdatedCampaign()
    {
        var campaign = new CampaignBuilder().Build();
        await _factory.Repository.AddAsync(campaign);
        var request = new
        {
            Title = "Updated campaign",
            Description = "Updated campaign description",
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(20),
            FinancialGoal = 1500m
        };

        var response = await _client.PutAsJsonAsync($"/api/v1/campaigns/{campaign.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.Data!.Title.Should().Be(request.Title);
        payload.Data.FinancialGoal.Should().Be(request.FinancialGoal);
    }

    [Fact]
    public async Task Given_ActiveCampaign_When_CompleteCampaignIsCalled_Then_ShouldReturnCompletedCampaign()
    {
        var campaign = new CampaignBuilder().Build();
        await _factory.Repository.AddAsync(campaign);

        var response = await _client.PatchAsync($"/api/v1/campaigns/{campaign.Id}/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.Data!.Status.Should().Be(CampaignStatus.Completed);
    }

    [Fact]
    public async Task Given_ActiveCampaign_When_CancelCampaignIsCalled_Then_ShouldReturnCanceledCampaign()
    {
        var campaign = new CampaignBuilder().Build();
        await _factory.Repository.AddAsync(campaign);

        var response = await _client.PatchAsync($"/api/v1/campaigns/{campaign.Id}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.Data!.Status.Should().Be(CampaignStatus.Canceled);
    }

    [Fact]
    public async Task Given_ActiveCampaign_When_TransparencyEndpointIsCalled_Then_ShouldReturnCampaign()
    {
        var campaign = new CampaignBuilder().Build();
        await _factory.Repository.AddAsync(campaign);

        var response = await _client.GetAsync("/api/v1/transparency/campaigns");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<TransparencyCampaignResponse>>>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.Data.Should().ContainSingle(item =>
            item.Title == campaign.Title &&
            item.FinancialGoal == campaign.FinancialGoal &&
            item.TotalAmountRaised == campaign.TotalAmountRaised);

        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotContain("\"id\"");
        json.Should().NotContain("\"description\"");
        json.Should().NotContain("\"status\"");
    }

    [Fact]
    public async Task Given_ActiveCampaign_When_GetDonationEligibilityIsCalled_Then_ShouldReturnEligibleCampaign()
    {
        var campaign = new CampaignBuilder().Build();
        await _factory.Repository.AddAsync(campaign);

        var response = await _client.GetAsync($"/internal/campaigns/{campaign.Id}/donation-eligibility");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("\"eligible\":true");
        json.Should().Contain(campaign.Id.ToString());
    }

    [Fact]
    public async Task Given_UnknownCampaign_When_GetDonationEligibilityIsCalled_Then_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/internal/campaigns/{Guid.NewGuid()}/donation-eligibility");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
