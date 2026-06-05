using System.Diagnostics.CodeAnalysis;

namespace fcs.Campaign.WebApi.Observability;

[ExcludeFromCodeCoverage]

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public string ServiceName { get; set; } = "fcs.Campaign";
}
