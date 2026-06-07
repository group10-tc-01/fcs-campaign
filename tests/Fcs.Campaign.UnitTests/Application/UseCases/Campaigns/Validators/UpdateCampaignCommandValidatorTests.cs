using fcs.Campaign.Application.UseCases.Campaigns.UpdateCampaign;
using FluentAssertions;

namespace fcs.Campaign.UnitTests.Application.UseCases.Campaigns.Validators;

public sealed class UpdateCampaignCommandValidatorTests
{
    [Fact]
    public void Given_ValidCommand_When_Validate_Then_ShouldBeValid()
    {
        var sut = new UpdateCampaignCommandValidator();
        var command = new UpdateCampaignCommand(Guid.NewGuid(), "Food basket", "Monthly food support", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(10), 500);

        var result = sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidCommand_When_Validate_Then_ShouldReturnValidationErrors()
    {
        var sut = new UpdateCampaignCommandValidator();
        var command = new UpdateCampaignCommand(Guid.Empty, "", "", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(-1), 0);

        var result = sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(error => error.PropertyName).Should().Contain([
            nameof(UpdateCampaignCommand.Id),
            nameof(UpdateCampaignCommand.Title),
            nameof(UpdateCampaignCommand.Description),
            nameof(UpdateCampaignCommand.EndDate),
            nameof(UpdateCampaignCommand.FinancialGoal)
        ]);
    }
}
