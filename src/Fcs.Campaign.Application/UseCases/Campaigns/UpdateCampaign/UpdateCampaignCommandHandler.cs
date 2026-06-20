using Fcs.Campaign.Application.Abstractions.Authentication;
using Fcs.Campaign.Application.Abstractions.Messaging;
using Fcs.Campaign.Application.Audit;
using Fcs.Campaign.Domain.Abstractions;
using Fcs.Campaign.Domain.Campaigns;
using Fcs.Campaign.Domain.Results;

namespace Fcs.Campaign.Application.UseCases.Campaigns.UpdateCampaign;

public sealed class UpdateCampaignCommandHandler : ICommandHandler<UpdateCampaignCommand, CampaignResponse>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessagePublisher _messagePublisher;

    public UpdateCampaignCommandHandler(ICampaignRepository campaignRepository, ICurrentUser currentUser, IUnitOfWork unitOfWork, IMessagePublisher messagePublisher)
    {
        _campaignRepository = campaignRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _messagePublisher = messagePublisher;
    }

    public async Task<Result<CampaignResponse>> Handle(UpdateCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _campaignRepository.GetByIdAsync(request.Id, cancellationToken);
        if (campaign is null)
        {
            return Error.NotFound("Campaign.NotFound", "Campaign was not found.");
        }

        var updateResult = campaign.Update(request.Title, request.Description, request.StartDate, request.EndDate, request.FinancialGoal);
        if (updateResult.IsFailure)
        {
            return updateResult.Error;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        Guid.TryParse(_currentUser.KeycloakUserId, out var managerId);
        _messagePublisher.PublishAuditLogFireAndForget(AuditLogRequestedEvent.Create(
            AuditActions.CampaignUpdated,
            "Campaign",
            managerId == Guid.Empty ? null : managerId,
            "GestorONG",
            campaign.Id.ToString()));

        return CampaignResponse.FromDomain(campaign);
    }
}
