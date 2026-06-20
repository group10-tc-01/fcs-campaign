using System.Text.Json;
using Confluent.Kafka;
using fcs.Campaign.Application.Abstractions.Messaging;
using fcs.Campaign.Infrastructure.Kafka.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace fcs.Campaign.Infrastructure.Kafka.Messaging;

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
        await producer.ProduceAsync(_settings.TopicName, new Message<Null, string> { Value = payload }, cancellationToken);
        _logger.LogInformation("Published message to topic {TopicName}", _settings.TopicName);
    }
}
