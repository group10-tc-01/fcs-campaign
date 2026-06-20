using Fcs.Campaign.Application.UseCases.Campaigns.GetCampaigns;
using Fcs.Campaign.CommomTestsUtilities.Builders.Campaigns;
using Fcs.Campaign.CommomTestsUtilities.TestDoubles;
using Fcs.Campaign.Domain.Campaigns;
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
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.Select(campaign => campaign.Id).Should().Contain([firstCampaign.Id, secondCampaign.Id]);
        result.Value.TotalCount.Should().Be(2);
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
        result.Value.Items.Should().ContainSingle();
        result.Value.PageSize.Should().Be(1);
        result.Value.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Given_DuplicateStatusFilters_When_Handle_Then_ShouldFilterAndCountCampaigns()
    {
        var repository = new InMemoryCampaignRepository();
        var active = new CampaignBuilder().Build();
        var completed = new CampaignBuilder().WithFinancialGoal(2000).Build();
        completed.Complete();
        var canceled = new CampaignBuilder().WithFinancialGoal(3000).Build();
        canceled.Cancel();
        await repository.AddAsync(active);
        await repository.AddAsync(completed);
        await repository.AddAsync(canceled);
        var sut = new GetCampaignsQueryHandler(repository);

        var result = await sut.Handle(
            new GetCampaignsQuery(
                Statuses: [CampaignStatus.Active, CampaignStatus.Completed, CampaignStatus.Active]),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.Items.Select(item => item.Status)
            .Should().BeSubsetOf([CampaignStatus.Active, CampaignStatus.Completed]);
    }

    [Fact]
    public async Task Given_InvalidPagination_When_Handle_Then_ShouldNormalizeValues()
    {
        var repository = new InMemoryCampaignRepository();
        await repository.AddAsync(new CampaignBuilder().Build());
        var sut = new GetCampaignsQueryHandler(repository);

        var result = await sut.Handle(new GetCampaignsQuery(Page: 0, PageSize: 101), CancellationToken.None);

        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
    }
}
