using Fcs.Campaign.CommomTestsUtilities.Builders.Campaigns;
using FluentAssertions;

namespace Fcs.Campaign.FunctionalTests.Scenarios;

public sealed class CampaignDonationFlowTests
{
    [Fact]
    public void Given_ActiveCampaign_When_DonationIsApplied_Then_TransparencyTotalShouldBeUpdated()
    {
        var campaign = new CampaignBuilder().Build();

        var result = campaign.ApplyDonation(250);

        result.IsSuccess.Should().BeTrue();
        campaign.TotalAmountRaised.Should().Be(250);
    }
}
