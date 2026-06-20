using System.Diagnostics.CodeAnalysis;

namespace Fcs.Campaign.WebApi.Observability;

[ExcludeFromCodeCoverage]

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public string ServiceName { get; set; } = "fcs.Campaign";

    public bool EnableOtlpExporter { get; set; }

    public string OtlpEndpoint { get; set; } = string.Empty;

    public string OtlpAuthHeader { get; set; } = string.Empty;
}
