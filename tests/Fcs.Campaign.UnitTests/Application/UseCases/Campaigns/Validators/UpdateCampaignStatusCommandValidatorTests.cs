using Fcs.Campaign.Application.UseCases.Campaigns.UpdateCampaignStatus;
using FluentAssertions;

namespace Fcs.Campaign.UnitTests.Application.UseCases.Campaigns.Validators;

public sealed class UpdateCampaignStatusCommandValidatorTests
{
    [Theory]
    [InlineData("Completed")]
    [InlineData("Canceled")]
    [InlineData("completed")]
    public void Given_ValidStatus_When_Validate_Then_ShouldBeValid(string status)
    {
        var sut = new UpdateCampaignStatusCommandValidator();

        var result = sut.Validate(new UpdateCampaignStatusCommand(Guid.NewGuid(), status));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Active")]
    [InlineData("Unknown")]
    public void Given_InvalidStatus_When_Validate_Then_ShouldDescribeValidStatuses(string status)
    {
        var sut = new UpdateCampaignStatusCommandValidator();

        var result = sut.Validate(new UpdateCampaignStatusCommand(Guid.NewGuid(), status));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error =>
            error.ErrorMessage.Contains("Completed") &&
            error.ErrorMessage.Contains("Canceled"));
    }
}
