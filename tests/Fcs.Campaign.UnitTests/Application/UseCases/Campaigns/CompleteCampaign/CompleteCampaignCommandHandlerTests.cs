using fcs.Campaign.Application.UseCases.Campaigns.CompleteCampaign;
using fcs.Campaign.CommomTestsUtilities.Builders.Campaigns;
using fcs.Campaign.CommomTestsUtilities.TestDoubles;
using fcs.Campaign.Domain.Campaigns;
using Fcs.Campaign.Domain.Results;
using FluentAssertions;

namespace fcs.Campaign.UnitTests.Application.UseCases.Campaigns.CompleteCampaign;

public sealed class CompleteCampaignCommandHandlerTests
{
    [Fact]
    public async Task Given_ActiveCampaign_When_Handle_Then_ShouldCompleteCampaign()
    {
        var repository = new InMemoryCampaignRepository();
        var campaign = new CampaignBuilder().Build();
        await repository.AddAsync(campaign);
        var unitOfWork = new FakeUnitOfWork();
        var publisher = new FakeMessagePublisher();
        var sut = new CompleteCampaignCommandHandler(repository, new FakeCurrentUser(), unitOfWork, publisher);

        var result = await sut.Handle(new CompleteCampaignCommand(campaign.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(CampaignStatus.Completed);
        unitOfWork.SaveChangesCalls.Should().Be(1);
    }

    [Fact]
    public async Task Given_UnknownCampaign_When_Handle_Then_ShouldReturnNotFound()
    {
        var sut = new CompleteCampaignCommandHandler(new InMemoryCampaignRepository(), new FakeCurrentUser(), new FakeUnitOfWork(), new FakeMessagePublisher());

        var result = await sut.Handle(new CompleteCampaignCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_CanceledCampaign_When_Handle_Then_ShouldReturnConflict()
    {
        var repository = new InMemoryCampaignRepository();
        var campaign = new CampaignBuilder().Build();
        campaign.Cancel();
        await repository.AddAsync(campaign);
        var unitOfWork = new FakeUnitOfWork();
        var sut = new CompleteCampaignCommandHandler(repository, new FakeCurrentUser(), unitOfWork, new FakeMessagePublisher());

        var result = await sut.Handle(new CompleteCampaignCommand(campaign.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        unitOfWork.SaveChangesCalls.Should().Be(0);
    }
}
