using Fcs.Campaign.Domain.Abstractions;
using Fcs.Campaign.Domain.Results;

namespace Fcs.Campaign.Domain.Campaigns;

public sealed class Campaign : BaseEntity
{
    private Campaign()
    {
        Title = string.Empty;
        Description = string.Empty;
    }

    private Campaign(
        Guid id,
        string title,
        string description,
        DateTime startDate,
        DateTime endDate,
        decimal financialGoal,
        Guid createdByManagerId) : base(id)
    {
        Title = title.Trim();
        Description = description.Trim();
        StartDate = startDate;
        EndDate = endDate;
        FinancialGoal = financialGoal;
        CreatedByManagerId = createdByManagerId;
        Status = CampaignStatus.Active;
        TotalAmountRaised = 0;
    }

    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public decimal FinancialGoal { get; private set; }
    public CampaignStatus Status { get; private set; }
    public decimal TotalAmountRaised { get; private set; }
    public Guid CreatedByManagerId { get; private set; }

    public static Result<Campaign> Create(
        string title,
        string description,
        DateTime startDate,
        DateTime endDate,
        decimal financialGoal,
        Guid createdByManagerId)
    {
        var validation = ValidateEditableFields(title, description, startDate, endDate, financialGoal);
        if (validation.IsFailure)
        {
            return validation.Error;
        }

        if (createdByManagerId == Guid.Empty)
        {
            return Error.Validation("Campaign.ManagerRequired", "Manager id is required.");
        }

        return new Campaign(Guid.NewGuid(), title, description, startDate, endDate, financialGoal, createdByManagerId);
    }

    public Result<bool> Update(
        string title,
        string description,
        DateTime startDate,
        DateTime endDate,
        decimal financialGoal)
    {
        if (Status != CampaignStatus.Active)
        {
            return Error.Conflict("Campaign.NotEditable", "Only active campaigns can be updated.");
        }

        var validation = ValidateEditableFields(title, description, startDate, endDate, financialGoal);
        if (validation.IsFailure)
        {
            return validation.Error;
        }

        Title = title.Trim();
        Description = description.Trim();
        StartDate = startDate;
        EndDate = endDate;
        FinancialGoal = financialGoal;
        Touch();

        return true;
    }

    public Result<bool> Complete()
    {
        if (Status != CampaignStatus.Active)
        {
            return Error.Conflict("Campaign.InvalidStatusTransition", "Only active campaigns can be completed.");
        }

        Status = CampaignStatus.Completed;
        Touch();
        return true;
    }

    public Result<bool> Cancel()
    {
        if (Status != CampaignStatus.Active)
        {
            return Error.Conflict("Campaign.InvalidStatusTransition", "Only active campaigns can be canceled.");
        }

        Status = CampaignStatus.Canceled;
        Touch();
        return true;
    }

    public Result<bool> ApplyDonation(decimal amount)
    {
        if (Status != CampaignStatus.Active)
        {
            return Error.Conflict("Campaign.NotEligibleForDonation", "Campaign is not eligible to receive donations.");
        }

        if (amount <= 0)
        {
            return Error.Validation("Campaign.InvalidDonationAmount", "Donation amount must be greater than zero.");
        }

        TotalAmountRaised += amount;
        Touch();
        return true;
    }

    private static Result<bool> ValidateEditableFields(
        string title,
        string description,
        DateTime startDate,
        DateTime endDate,
        decimal financialGoal)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Error.Validation("Campaign.TitleRequired", "Title is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Error.Validation("Campaign.DescriptionRequired", "Description is required.");
        }

        if (endDate.Date < DateTime.UtcNow.Date)
        {
            return Error.Validation("Campaign.EndDateInPast", "End date cannot be in the past.");
        }

        if (endDate.Date < startDate.Date)
        {
            return Error.Validation("Campaign.InvalidDateRange", "End date cannot be before start date.");
        }

        if (financialGoal <= 0)
        {
            return Error.Validation("Campaign.InvalidFinancialGoal", "Financial goal must be greater than zero.");
        }

        return true;
    }
}
