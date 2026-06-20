using Fcs.Campaign.Application.UseCases.Internal.GetDonationEligibility;
using Fcs.Campaign.CommomTestsUtilities.Builders.Campaigns;
using Fcs.Campaign.CommomTestsUtilities.TestDoubles;
using Fcs.Campaign.Domain.Results;
using FluentAssertions;

namespace Fcs.Campaign.UnitTests.Application.UseCases.Internal.GetDonationEligibility;

public sealed class GetDonationEligibilityQueryHandlerTests
{
    [Fact]
    public async Task Given_ActiveCampaign_When_Handle_Then_ShouldReturnEligible()
    {
        var repository = new InMemoryCampaignRepository();
        var campaign = new CampaignBuilder().Build();
        await repository.AddAsync(campaign);
        var sut = new GetDonationEligibilityQueryHandler(repository);

        var result = await sut.Handle(new GetDonationEligibilityQuery(campaign.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CampaignId.Should().Be(campaign.Id);
        result.Value.Eligible.Should().BeTrue();
        result.Value.Reason.Should().BeNull();
    }

    [Fact]
    public async Task Given_CanceledCampaign_When_Handle_Then_ShouldReturnIneligible()
    {
        var repository = new InMemoryCampaignRepository();
        var campaign = new CampaignBuilder().Build();
        campaign.Cancel();
        await repository.AddAsync(campaign);
        var sut = new GetDonationEligibilityQueryHandler(repository);

        var result = await sut.Handle(new GetDonationEligibilityQuery(campaign.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Eligible.Should().BeFalse();
        result.Value.Reason.Should().Be("Campaign status is Canceled.");
    }

    [Fact]
    public async Task Given_UnknownCampaign_When_Handle_Then_ShouldReturnNotFound()
    {
        var sut = new GetDonationEligibilityQueryHandler(new InMemoryCampaignRepository());

        var result = await sut.Handle(new GetDonationEligibilityQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }
}
