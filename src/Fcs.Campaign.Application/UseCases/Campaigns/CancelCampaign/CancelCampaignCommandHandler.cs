using fcs.Campaign.Application.Abstractions.Authentication;
using fcs.Campaign.Application.Abstractions.Messaging;
using fcs.Campaign.Application.Audit;
using fcs.Campaign.Domain.Abstractions;
using fcs.Campaign.Domain.Campaigns;
using Fcs.Campaign.Domain.Results;

namespace fcs.Campaign.Application.UseCases.Campaigns.CancelCampaign;

public sealed class CancelCampaignCommandHandler : ICommandHandler<CancelCampaignCommand, CampaignResponse>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessagePublisher _messagePublisher;

    public CancelCampaignCommandHandler(ICampaignRepository campaignRepository, ICurrentUser currentUser, IUnitOfWork unitOfWork, IMessagePublisher messagePublisher)
    {
        _campaignRepository = campaignRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _messagePublisher = messagePublisher;
    }

    public async Task<Result<CampaignResponse>> Handle(CancelCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _campaignRepository.GetByIdAsync(request.Id, cancellationToken);
        if (campaign is null)
        {
            return Error.NotFound("Campaign.NotFound", "Campaign was not found.");
        }

        var result = campaign.Cancel();
        if (result.IsFailure)
        {
            return result.Error;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        Guid.TryParse(_currentUser.KeycloakUserId, out var managerId);
        _messagePublisher.PublishAuditLogFireAndForget(AuditLogRequestedEvent.Create(AuditActions.CampaignCanceled, "Campaign", managerId == Guid.Empty ? null : managerId, "GestorONG", campaign.Id.ToString()));

        return CampaignResponse.FromDomain(campaign);
    }
}
