using fcs.Campaign.Application.UseCases.Campaigns.CreateCampaign;
using FluentAssertions;

namespace fcs.Campaign.UnitTests.Application.UseCases.Campaigns.Validators;

public sealed class CreateCampaignCommandValidatorTests
{
    [Fact]
    public void Given_ValidCommand_When_Validate_Then_ShouldBeValid()
    {
        var sut = new CreateCampaignCommandValidator();
        var command = new CreateCampaignCommand("Food basket", "Monthly food support", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(10), 500);

        var result = sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidCommand_When_Validate_Then_ShouldReturnValidationErrors()
    {
        var sut = new CreateCampaignCommandValidator();
        var command = new CreateCampaignCommand("", "", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(-1), 0);

        var result = sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(error => error.PropertyName).Should().Contain([
            nameof(CreateCampaignCommand.Title),
            nameof(CreateCampaignCommand.Description),
            nameof(CreateCampaignCommand.EndDate),
            nameof(CreateCampaignCommand.FinancialGoal)
        ]);
    }
}
