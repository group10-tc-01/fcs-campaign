using Fcs.Campaign.Application.Abstractions.Messaging;
using Fcs.Campaign.Application.Audit;
using Fcs.Campaign.Domain.Abstractions;
using Fcs.Campaign.Domain.Campaigns;
using Fcs.Campaign.Domain.Results;

namespace Fcs.Campaign.Application.UseCases.Internal.ProcessDonation;

public sealed class ProcessDonationCommandHandler : ICommandHandler<ProcessDonationCommand, ProcessDonationResponse>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessagePublisher _messagePublisher;

    public ProcessDonationCommandHandler(ICampaignRepository campaignRepository, IUnitOfWork unitOfWork, IMessagePublisher messagePublisher)
    {
        _campaignRepository = campaignRepository;
        _unitOfWork = unitOfWork;
        _messagePublisher = messagePublisher;
    }

    public async Task<Result<ProcessDonationResponse>> Handle(ProcessDonationCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _campaignRepository.GetByIdAsync(request.CampaignId, cancellationToken);
        if (campaign is null)
        {
            return Error.NotFound("Campaign.NotFound", "Campaign was not found.");
        }

        if (await _campaignRepository.ExistsDonationEntryAsync(request.CampaignId, request.DonationId, cancellationToken))
        {
            _messagePublisher.PublishAuditLogFireAndForget(AuditLogRequestedEvent.Create(
                AuditActions.DuplicateDonationIgnored,
                "CampaignDonationEntry",
                actorType: "System",
                entityId: request.DonationId.ToString(),
                metadata: new Dictionary<string, object?> { ["campaignId"] = request.CampaignId }));

            return new ProcessDonationResponse(request.CampaignId, request.DonationId, false, true);
        }

        var donationResult = campaign.ApplyDonation(request.Amount);
        if (donationResult.IsFailure)
        {
            return donationResult.Error;
        }

        var entryResult = CampaignDonationEntry.Create(request.CampaignId, request.DonationId, request.Amount, request.ProcessedAt);
        if (entryResult.IsFailure)
        {
            return entryResult.Error;
        }

        await _campaignRepository.AddDonationEntryAsync(entryResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _messagePublisher.PublishAuditLogFireAndForget(AuditLogRequestedEvent.Create(
            AuditActions.DonationReflected,
            "CampaignDonationEntry",
            actorType: "System",
            entityId: entryResult.Value.Id.ToString(),
            metadata: new Dictionary<string, object?>
            {
                ["campaignId"] = request.CampaignId,
                ["donationId"] = request.DonationId,
                ["amount"] = request.Amount
            }));

        return new ProcessDonationResponse(request.CampaignId, request.DonationId, true, false);
    }
}
