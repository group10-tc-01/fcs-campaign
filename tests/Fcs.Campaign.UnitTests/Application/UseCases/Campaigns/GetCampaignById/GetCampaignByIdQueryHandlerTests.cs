using Fcs.Campaign.Application.UseCases.Campaigns.GetCampaignById;
using Fcs.Campaign.CommomTestsUtilities.Builders.Campaigns;
using Fcs.Campaign.CommomTestsUtilities.TestDoubles;
using Fcs.Campaign.Domain.Results;
using FluentAssertions;

namespace Fcs.Campaign.UnitTests.Application.UseCases.Campaigns.GetCampaignById;

public sealed class GetCampaignByIdQueryHandlerTests
{
    [Fact]
    public async Task Given_ExistingCampaign_When_Handle_Then_ShouldReturnCampaign()
    {
        var repository = new InMemoryCampaignRepository();
        var campaign = new CampaignBuilder().Build();
        await repository.AddAsync(campaign);
        var sut = new GetCampaignByIdQueryHandler(repository);

        var result = await sut.Handle(new GetCampaignByIdQuery(campaign.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(campaign.Id);
    }

    [Fact]
    public async Task Given_UnknownCampaign_When_Handle_Then_ShouldReturnNotFound()
    {
        var sut = new GetCampaignByIdQueryHandler(new InMemoryCampaignRepository());

        var result = await sut.Handle(new GetCampaignByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }
}
