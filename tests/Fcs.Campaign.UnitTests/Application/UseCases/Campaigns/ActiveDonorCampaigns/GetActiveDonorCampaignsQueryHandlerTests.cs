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
}
