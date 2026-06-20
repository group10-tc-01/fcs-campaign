using fcs.Campaign.Application.UseCases.Campaigns.CreateCampaign;
using fcs.Campaign.CommomTestsUtilities.TestDoubles;
using fcs.Campaign.Domain.Campaigns;
using Fcs.Campaign.Domain.Results;
using FluentAssertions;

namespace fcs.Campaign.UnitTests.Application.UseCases.Campaigns.CreateCampaign;

public sealed class CreateCampaignCommandHandlerTests
{
    [Fact]
    public async Task Given_ValidCommand_When_Handle_Then_ShouldCreateCampaign()
    {
        var repository = new InMemoryCampaignRepository();
        var currentUser = new FakeCurrentUser();
        var unitOfWork = new FakeUnitOfWork();
        var publisher = new FakeMessagePublisher();
        var sut = new CreateCampaignCommandHandler(repository, currentUser, unitOfWork, publisher);
        var request = new CreateCampaignCommand("Food basket", "Monthly food support", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(10), 500);

        var result = await sut.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be(request.Title);
        result.Value.Status.Should().Be(CampaignStatus.Active);
        repository.Campaigns.Should().ContainSingle();
        unitOfWork.SaveChangesCalls.Should().Be(1);
    }

    [Fact]
    public async Task Given_InvalidCurrentUser_When_Handle_Then_ShouldReturnValidationError()
    {
        var sut = new CreateCampaignCommandHandler(
            new InMemoryCampaignRepository(),
            new FakeCurrentUser { KeycloakUserId = "invalid-guid" },
            new FakeUnitOfWork(),
            new FakeMessagePublisher());
        var request = new CreateCampaignCommand("Food basket", "Monthly food support", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(10), 500);

        var result = await sut.Handle(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Given_InvalidCampaignData_When_Handle_Then_ShouldReturnDomainError()
    {
        var repository = new InMemoryCampaignRepository();
        var unitOfWork = new FakeUnitOfWork();
        var sut = new CreateCampaignCommandHandler(repository, new FakeCurrentUser(), unitOfWork, new FakeMessagePublisher());
        var request = new CreateCampaignCommand("", "Monthly food support", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(10), 500);

        var result = await sut.Handle(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        repository.Campaigns.Should().BeEmpty();
        unitOfWork.SaveChangesCalls.Should().Be(0);
    }
}
