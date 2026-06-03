namespace Fcg.Campaign.CommomTestsUtilities.Builders.Campaigns;

public sealed class CampaignBuilder
{
    private string _title = "Winter blankets";
    private string _description = "Campaign to collect resources for winter blankets.";
    private DateTime _startDate = DateTime.UtcNow.Date;
    private DateTime _endDate = DateTime.UtcNow.Date.AddDays(30);
    private decimal _financialGoal = 1000;
    private Guid _createdByManagerId = Guid.NewGuid();

    public CampaignBuilder WithEndDate(DateTime endDate)
    {
        _endDate = endDate;
        return this;
    }

    public CampaignBuilder WithFinancialGoal(decimal financialGoal)
    {
        _financialGoal = financialGoal;
        return this;
    }

    public CampaignBuilder WithManagerId(Guid managerId)
    {
        _createdByManagerId = managerId;
        return this;
    }

    public Domain.Campaigns.Campaign Build()
    {
        return Domain.Campaigns.Campaign.Create(_title, _description, _startDate, _endDate, _financialGoal, _createdByManagerId).Value;
    }
}
