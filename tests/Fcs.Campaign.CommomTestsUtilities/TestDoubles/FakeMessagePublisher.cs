using Fcs.Campaign.Application.Abstractions.Messaging;

namespace Fcs.Campaign.CommomTestsUtilities.TestDoubles;

public sealed class FakeMessagePublisher : IMessagePublisher
{
    private readonly object _sync = new();

    public List<object> PublishedMessages { get; } = new();

    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            PublishedMessages.Add(message!);
        }

        return Task.CompletedTask;
    }

    public async Task<TMessage?> WaitForMessageAsync<TMessage>(
        Func<TMessage, bool> predicate,
        TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow.Add(timeout);
        while (DateTime.UtcNow < deadline)
        {
            lock (_sync)
            {
                var message = PublishedMessages.OfType<TMessage>().FirstOrDefault(predicate);
                if (message is not null)
                {
                    return message;
                }
            }

            await Task.Delay(10);
        }

        return default;
    }

    public void Reset()
    {
        lock (_sync)
        {
            PublishedMessages.Clear();
        }
    }
}
