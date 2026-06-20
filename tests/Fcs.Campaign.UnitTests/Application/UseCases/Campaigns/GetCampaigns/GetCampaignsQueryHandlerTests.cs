using Fcs.Campaign.Application.UseCases.Campaigns.GetCampaigns;
using Fcs.Campaign.CommomTestsUtilities.Builders.Campaigns;
using Fcs.Campaign.CommomTestsUtilities.TestDoubles;
using FluentAssertions;

namespace Fcs.Campaign.UnitTests.Application.UseCases.Campaigns.GetCampaigns;

public sealed class GetCampaignsQueryHandlerTests
{
    [Fact]
    public async Task Given_Campaigns_When_Handle_Then_ShouldReturnCampaignResponses()
    {
        var repository = new InMemoryCampaignRepository();
        var firstCampaign = new CampaignBuilder().Build();
        var secondCampaign = new CampaignBuilder().WithFinancialGoal(2000).Build();
        await repository.AddAsync(firstCampaign);
        await repository.AddAsync(secondCampaign);
        var sut = new GetCampaignsQueryHandler(repository);

        var result = await sut.Handle(new GetCampaignsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(campaign => campaign.Id).Should().Contain([firstCampaign.Id, secondCampaign.Id]);
    }

    [Fact]
    public async Task Given_PageSizeOne_When_Handle_Then_ShouldReturnOnlyOneCampaign()
    {
        var repository = new InMemoryCampaignRepository();
        await repository.AddAsync(new CampaignBuilder().Build());
        await repository.AddAsync(new CampaignBuilder().WithFinancialGoal(2000).Build());
        var sut = new GetCampaignsQueryHandler(repository);

        var result = await sut.Handle(new GetCampaignsQuery(PageSize: 1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
    }
}
