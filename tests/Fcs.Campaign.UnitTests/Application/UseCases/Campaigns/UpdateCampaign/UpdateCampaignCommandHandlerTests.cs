using fcs.Campaign.Application.UseCases.Campaigns.UpdateCampaign;
using fcs.Campaign.CommomTestsUtilities.Builders.Campaigns;
using fcs.Campaign.CommomTestsUtilities.TestDoubles;
using Fcs.Campaign.Domain.Results;
using FluentAssertions;

namespace fcs.Campaign.UnitTests.Application.UseCases.Campaigns.UpdateCampaign;

public sealed class UpdateCampaignCommandHandlerTests
{
    [Fact]
    public async Task Given_ExistingCampaign_When_Handle_Then_ShouldUpdateCampaign()
    {
        var repository = new InMemoryCampaignRepository();
        var campaign = new CampaignBuilder().Build();
        await repository.AddAsync(campaign);
        var unitOfWork = new FakeUnitOfWork();
        var publisher = new FakeMessagePublisher();
        var sut = new UpdateCampaignCommandHandler(repository, new FakeCurrentUser(), unitOfWork, publisher);
        var request = new UpdateCampaignCommand(campaign.Id, "Updated title", "Updated description", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(20), 1500);

        var result = await sut.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be(request.Title);
        result.Value.Description.Should().Be(request.Description);
        result.Value.FinancialGoal.Should().Be(request.FinancialGoal);
        unitOfWork.SaveChangesCalls.Should().Be(1);
    }

    [Fact]
    public async Task Given_UnknownCampaign_When_Handle_Then_ShouldReturnNotFound()
    {
        var sut = new UpdateCampaignCommandHandler(new InMemoryCampaignRepository(), new FakeCurrentUser(), new FakeUnitOfWork(), new FakeMessagePublisher());
        var request = new UpdateCampaignCommand(Guid.NewGuid(), "Updated title", "Updated description", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(20), 1500);

        var result = await sut.Handle(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_CompletedCampaign_When_Handle_Then_ShouldReturnConflict()
    {
        var repository = new InMemoryCampaignRepository();
        var campaign = new CampaignBuilder().Build();
        campaign.Complete();
        await repository.AddAsync(campaign);
        var unitOfWork = new FakeUnitOfWork();
        var sut = new UpdateCampaignCommandHandler(repository, new FakeCurrentUser(), unitOfWork, new FakeMessagePublisher());
        var request = new UpdateCampaignCommand(campaign.Id, "Updated title", "Updated description", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(20), 1500);

        var result = await sut.Handle(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        unitOfWork.SaveChangesCalls.Should().Be(0);
    }
}
