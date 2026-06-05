namespace fcs.Campaign.WebApi.Observability;

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public string ServiceName { get; set; } = "fcs.Campaign";
}
