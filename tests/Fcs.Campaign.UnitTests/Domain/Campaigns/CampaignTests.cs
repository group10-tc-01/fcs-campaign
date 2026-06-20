using Fcs.Campaign.CommomTestsUtilities.Builders.Campaigns;
using Fcs.Campaign.Domain.Campaigns;
using Fcs.Campaign.Domain.Results;
using FluentAssertions;
using DomainCampaign = Fcs.Campaign.Domain.Campaigns.Campaign;

namespace Fcs.Campaign.UnitTests.Domain.Campaigns;

public sealed class CampaignTests
{
    [Fact]
    public void Given_ValidData_When_Create_Then_ShouldReturnActiveCampaign()
    {
        var managerId = Guid.NewGuid();

        var result = DomainCampaign.Create(
            "Food basket",
            "Monthly food basket campaign",
            DateTime.UtcNow.Date,
            DateTime.UtcNow.Date.AddDays(10),
            500,
            managerId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(CampaignStatus.Active);
        result.Value.TotalAmountRaised.Should().Be(0);
        result.Value.CreatedByManagerId.Should().Be(managerId);
    }

    [Fact]
    public void Given_EndDateInPast_When_Create_Then_ShouldReturnValidationError()
    {
        var result = DomainCampaign.Create(
            "Food basket",
            "Monthly food basket campaign",
            DateTime.UtcNow.Date.AddDays(-10),
            DateTime.UtcNow.Date.AddDays(-1),
            500,
            Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Given_ActiveCampaign_When_ApplyDonation_Then_ShouldIncreaseTotalAmountRaised()
    {
        var campaign = new CampaignBuilder().Build();

        var result = campaign.ApplyDonation(150);

        result.IsSuccess.Should().BeTrue();
        campaign.TotalAmountRaised.Should().Be(150);
    }

    [Fact]
    public void Given_CompletedCampaign_When_ApplyDonation_Then_ShouldReturnValidationError()
    {
        var campaign = new CampaignBuilder().Build();
        campaign.Complete();

        var result = campaign.ApplyDonation(150);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Given_CanceledCampaign_When_Complete_Then_ShouldReturnValidationError()
    {
        var campaign = new CampaignBuilder().Build();
        campaign.Cancel();

        var result = campaign.Complete();

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }
}
