using Fcs.Campaign.Application.UseCases.Campaigns.ActiveDonorCampaigns;
using Fcs.Campaign.CommomTestsUtilities.Builders.Campaigns;
using Fcs.Campaign.CommomTestsUtilities.TestDoubles;
using FluentAssertions;

namespace Fcs.Campaign.UnitTests.Application.UseCases.Campaigns.ActiveDonorCampaigns;

public sealed class GetActiveDonorCampaignsQueryHandlerTests
{
    [Fact]
    public async Task Given_ActiveCampaigns_When_Handle_Then_ShouldReturnOnlyActive()
    {
        var repository = new InMemoryCampaignRepository();
        var activeCampaign = new CampaignBuilder().Build();
        var completedCampaign = new CampaignBuilder().WithFinancialGoal(2000).Build();
        completedCampaign.Complete();
        await repository.AddAsync(activeCampaign);
        await repository.AddAsync(completedCampaign);
        var sut = new GetActiveDonorCampaignsQueryHandler(repository);

        var result = await sut.Handle(new GetActiveDonorCampaignsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(activeCampaign.Id);
    }

    [Fact]
    public async Task Given_NoActiveCampaigns_When_Handle_Then_ShouldReturnEmpty()
    {
        var repository = new InMemoryCampaignRepository();
        var completedCampaign = new CampaignBuilder().Build();
        completedCampaign.Complete();
        await repository.AddAsync(completedCampaign);
        var sut = new GetActiveDonorCampaignsQueryHandler(repository);

        var result = await sut.Handle(new GetActiveDonorCampaignsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_Pagination_When_Handle_Then_ShouldRespectPageSize()
    {
        var repository = new InMemoryCampaignRepository();
        for (var i = 0; i < 3; i++)
            await repository.AddAsync(new CampaignBuilder().WithFinancialGoal(1000 * (i + 1)).Build());
        var sut = new GetActiveDonorCampaignsQueryHandler(repository);

        var result = await sut.Handle(new GetActiveDonorCampaignsQuery(PageSize: 2), CancellationToken.None);

        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public void Given_DefaultConstructor_When_CreateQuery_Then_ShouldSetDefaults()
    {
        var query = new GetActiveDonorCampaignsQuery();

        query.Page.Should().Be(1);
        query.PageSize.Should().Be(10);
    }

    [Fact]
    public void Given_Values_When_CreateResponse_Then_ShouldSetProperties()
    {
        var id = Guid.NewGuid();
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(10);

        var response = new ActiveDonorCampaignResponse(id, "Test Campaign", 10000m, 2500m, startDate, endDate);

        response.Id.Should().Be(id);
        response.Title.Should().Be("Test Campaign");
        response.FinancialGoal.Should().Be(10000m);
        response.TotalAmountRaised.Should().Be(2500m);
        response.StartDate.Should().Be(startDate);
        response.EndDate.Should().Be(endDate);
    }
}
