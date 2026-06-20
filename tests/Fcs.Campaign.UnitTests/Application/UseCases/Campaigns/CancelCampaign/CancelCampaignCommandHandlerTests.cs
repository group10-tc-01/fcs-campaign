using Fcs.Campaign.Application.UseCases.Campaigns.CancelCampaign;
using Fcs.Campaign.CommomTestsUtilities.Builders.Campaigns;
using Fcs.Campaign.CommomTestsUtilities.TestDoubles;
using Fcs.Campaign.Domain.Campaigns;
using Fcs.Campaign.Domain.Results;
using FluentAssertions;

namespace Fcs.Campaign.UnitTests.Application.UseCases.Campaigns.CancelCampaign;

public sealed class CancelCampaignCommandHandlerTests
{
    [Fact]
    public async Task Given_ActiveCampaign_When_Handle_Then_ShouldCancelCampaign()
    {
        var repository = new InMemoryCampaignRepository();
        var campaign = new CampaignBuilder().Build();
        await repository.AddAsync(campaign);
        var unitOfWork = new FakeUnitOfWork();
        var publisher = new FakeMessagePublisher();
        var sut = new CancelCampaignCommandHandler(repository, new FakeCurrentUser(), unitOfWork, publisher);

        var result = await sut.Handle(new CancelCampaignCommand(campaign.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(CampaignStatus.Canceled);
        unitOfWork.SaveChangesCalls.Should().Be(1);
    }

    [Fact]
    public async Task Given_UnknownCampaign_When_Handle_Then_ShouldReturnNotFound()
    {
        var sut = new CancelCampaignCommandHandler(new InMemoryCampaignRepository(), new FakeCurrentUser(), new FakeUnitOfWork(), new FakeMessagePublisher());

        var result = await sut.Handle(new CancelCampaignCommand(Guid.NewGuid()), CancellationToken.None);

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
        var sut = new CancelCampaignCommandHandler(repository, new FakeCurrentUser(), unitOfWork, new FakeMessagePublisher());

        var result = await sut.Handle(new CancelCampaignCommand(campaign.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        unitOfWork.SaveChangesCalls.Should().Be(0);
    }
}
