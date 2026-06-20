using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Fcs.Campaign.WebApi.Observability;

[ExcludeFromCodeCoverage]
public sealed class ObservabilitySettings
{
    public const string SectionName = "Observability";

    [Required]
    public string ServiceName { get; set; } = "Fcs.Campaign";

    public bool EnableOtlpExporter { get; set; }

    public string OtlpEndpoint { get; set; } = string.Empty;

    public string OtlpAuthHeader { get; set; } = string.Empty;
}
