using Fcg.Campaign.Application.Abstractions.Authentication;
using Fcg.Campaign.Application.Abstractions.Messaging;
using Fcg.Campaign.Application.Audit;
using Fcg.Campaign.Domain;
using Fcg.Campaign.Domain.Abstractions;
using Fcg.Campaign.Domain.Campaigns;

namespace Fcg.Campaign.Application.UseCases.Campaigns.CreateCampaign;

public sealed class CreateCampaignCommandHandler : ICommandHandler<CreateCampaignCommand, CampaignResponse>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessagePublisher _messagePublisher;

    public CreateCampaignCommandHandler(
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

    public async Task<Result<CampaignResponse>> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUser.KeycloakUserId, out var managerId))
        {
            return Error.Validation("CurrentUser.InvalidManagerId", "Authenticated manager id is invalid.");
        }

        var campaignResult = Domain.Campaigns.Campaign.Create(
            request.Title,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.FinancialGoal,
            managerId);

        if (campaignResult.IsFailure)
        {
            return campaignResult.Error;
        }

        await _campaignRepository.AddAsync(campaignResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _messagePublisher.PublishAuditLogFireAndForget(AuditLogRequestedEvent.Create(
            AuditActions.CampaignCreated,
            "Campaign",
            managerId,
            "GestorONG",
            campaignResult.Value.Id.ToString()));

        return CampaignResponse.FromDomain(campaignResult.Value);
    }
}
