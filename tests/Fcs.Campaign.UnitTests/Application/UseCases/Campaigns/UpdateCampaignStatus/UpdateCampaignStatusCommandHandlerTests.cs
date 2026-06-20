using Fcs.Campaign.Application.UseCases.Campaigns.UpdateCampaignStatus;
using Fcs.Campaign.CommomTestsUtilities.Builders.Campaigns;
using Fcs.Campaign.CommomTestsUtilities.TestDoubles;
using Fcs.Campaign.Domain.Campaigns;
using Fcs.Campaign.Domain.Results;
using FluentAssertions;

namespace Fcs.Campaign.UnitTests.Application.UseCases.Campaigns.UpdateCampaignStatus;

public sealed class UpdateCampaignStatusCommandHandlerTests
{
    [Theory]
    [InlineData(CampaignStatus.Completed)]
    [InlineData(CampaignStatus.Canceled)]
    public async Task Given_ActiveCampaign_When_Handle_Then_ShouldUpdateStatus(CampaignStatus status)
    {
        var repository = new InMemoryCampaignRepository();
        var campaign = new CampaignBuilder().Build();
        await repository.AddAsync(campaign);
        var unitOfWork = new FakeUnitOfWork();
        var sut = new UpdateCampaignStatusCommandHandler(
            repository,
            new FakeCurrentUser(),
            unitOfWork,
            new FakeMessagePublisher());

        var result = await sut.Handle(
            new UpdateCampaignStatusCommand(campaign.Id, status.ToString()),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(status);
        result.Value.UpdatedAt.Should().NotBeNull();
        unitOfWork.SaveChangesCalls.Should().Be(1);
    }

    [Fact]
    public async Task Given_UnknownCampaign_When_Handle_Then_ShouldReturnNotFound()
    {
        var sut = new UpdateCampaignStatusCommandHandler(
            new InMemoryCampaignRepository(),
            new FakeCurrentUser(),
            new FakeUnitOfWork(),
            new FakeMessagePublisher());

        var result = await sut.Handle(
            new UpdateCampaignStatusCommand(Guid.NewGuid(), CampaignStatus.Completed.ToString()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ActiveStatus_When_Handle_Then_ShouldReturnValidationError()
    {
        var repository = new InMemoryCampaignRepository();
        var campaign = new CampaignBuilder().Build();
        await repository.AddAsync(campaign);
        var unitOfWork = new FakeUnitOfWork();
        var sut = new UpdateCampaignStatusCommandHandler(
            repository,
            new FakeCurrentUser(),
            unitOfWork,
            new FakeMessagePublisher());

        var result = await sut.Handle(
            new UpdateCampaignStatusCommand(campaign.Id, CampaignStatus.Active.ToString()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        unitOfWork.SaveChangesCalls.Should().Be(0);
    }

    [Fact]
    public async Task Given_ClosedCampaign_When_Handle_Then_ShouldReturnValidationError()
    {
        var repository = new InMemoryCampaignRepository();
        var campaign = new CampaignBuilder().Build();
        campaign.Complete();
        await repository.AddAsync(campaign);
        var unitOfWork = new FakeUnitOfWork();
        var sut = new UpdateCampaignStatusCommandHandler(
            repository,
            new FakeCurrentUser(),
            unitOfWork,
            new FakeMessagePublisher());

        var result = await sut.Handle(
            new UpdateCampaignStatusCommand(campaign.Id, CampaignStatus.Canceled.ToString()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        unitOfWork.SaveChangesCalls.Should().Be(0);
    }
}
