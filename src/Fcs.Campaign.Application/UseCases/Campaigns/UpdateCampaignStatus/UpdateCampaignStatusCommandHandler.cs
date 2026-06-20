using Fcs.Campaign.Application.Abstractions.Authentication;
using Fcs.Campaign.Application.Abstractions.Messaging;
using Fcs.Campaign.Application.Audit;
using Fcs.Campaign.Domain.Abstractions;
using Fcs.Campaign.Domain.Campaigns;
using Fcs.Campaign.Domain.Results;

namespace Fcs.Campaign.Application.UseCases.Campaigns.UpdateCampaignStatus;

public sealed class UpdateCampaignStatusCommandHandler : ICommandHandler<UpdateCampaignStatusCommand, CampaignStatusResponse>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessagePublisher _messagePublisher;

    public UpdateCampaignStatusCommandHandler(
        ICampaignRepository campaignRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        IMessagePublisher messagePublisher)
    {
        _campaignRepository = campaignRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _messagePublisher = messagePublisher;
    }

    public async Task<Result<CampaignStatusResponse>> Handle(
        UpdateCampaignStatusCommand request,
        CancellationToken cancellationToken)
    {
        var campaign = await _campaignRepository.GetByIdAsync(request.Id, cancellationToken);
        if (campaign is null)
        {
            return Error.NotFound("Campaign.NotFound", "Campaign was not found.");
        }

        var status = Enum.Parse<CampaignStatus>(request.Status, ignoreCase: true);
        var transition = status switch
        {
            CampaignStatus.Completed => campaign.Complete(),
            CampaignStatus.Canceled => campaign.Cancel(),
            _ => Error.Validation(
                "Campaign.InvalidStatusTransition",
                "Campaign can only transition to Completed or Canceled.")
        };

        if (transition.IsFailure)
        {
            return transition.Error;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        Guid.TryParse(_currentUser.KeycloakUserId, out var managerId);
        var action = status == CampaignStatus.Completed
            ? AuditActions.CampaignCompleted
            : AuditActions.CampaignCanceled;

        _messagePublisher.PublishAuditLogFireAndForget(AuditLogRequestedEvent.Create(
            action,
            "Campaign",
            managerId == Guid.Empty ? null : managerId,
            "GestorONG",
            campaign.Id.ToString()));

        return new CampaignStatusResponse(campaign.Id, campaign.Status, campaign.UpdatedAt);
    }
}
