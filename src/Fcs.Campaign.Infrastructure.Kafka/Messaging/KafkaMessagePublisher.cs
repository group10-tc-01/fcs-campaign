using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Fcs.Campaign.Application.Abstractions.Messaging;
using Fcs.Campaign.Infrastructure.Kafka.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fcs.Campaign.Infrastructure.Kafka.Messaging;

[ExcludeFromCodeCoverage]
public sealed class KafkaMessagePublisher : IMessagePublisher
{
    private readonly KafkaSettings _settings;
    private readonly ILogger<KafkaMessagePublisher> _logger;

    public KafkaMessagePublisher(IOptions<KafkaSettings> options, ILogger<KafkaMessagePublisher> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            Acks = Acks.All
        };

        using var producer = new ProducerBuilder<Null, string>(config).Build();
        var payload = JsonSerializer.Serialize(message);
        await producer.ProduceAsync(_settings.TopicName, CreateMessage(payload), cancellationToken);
        _logger.LogInformation("Published message to topic {TopicName}", _settings.TopicName);
    }

    private static Message<Null, string> CreateMessage(string payload)
    {
        var headers = new Headers();
        var activity = Activity.Current;

        if (activity?.Id is { } traceParent)
        {
            headers.Add("traceparent", Encoding.UTF8.GetBytes(traceParent));
        }

        if (!string.IsNullOrWhiteSpace(activity?.TraceStateString))
        {
            headers.Add("tracestate", Encoding.UTF8.GetBytes(activity.TraceStateString));
        }

        return new Message<Null, string> { Value = payload, Headers = headers };
    }
}
