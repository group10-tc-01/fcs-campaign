using System.Diagnostics.CodeAnalysis;
using Fcs.Campaign.Application.Abstractions.Messaging;
using Fcs.Campaign.Infrastructure.Kafka.Messaging;
using Fcs.Campaign.Infrastructure.Kafka.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fcs.Campaign.Infrastructure.Kafka.DependencyInjection;

[ExcludeFromCodeCoverage]

public static class DependencyInjection
{
    public static IServiceCollection AddKafkaInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaSettings>(configuration.GetSection(KafkaSettings.SectionName));
        services.AddSingleton<IMessagePublisher, KafkaMessagePublisher>();
        return services;
    }
}
