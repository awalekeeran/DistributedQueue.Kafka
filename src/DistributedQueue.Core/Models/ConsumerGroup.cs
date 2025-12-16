using System.Collections.Concurrent;

namespace DistributedQueue.Core.Models;

public class ConsumerGroup
{
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    private readonly ConcurrentBag<Consumer> _consumers;
    private readonly ConcurrentDictionary<string, int> _topicOffsets;

    public ConsumerGroup(string name)
    {
        Name = name;
        CreatedAt = DateTime.UtcNow;
        _consumers = new ConcurrentBag<Consumer>();
        _topicOffsets = new ConcurrentDictionary<string, int>();
    }

    public void AddConsumer(Consumer consumer)
    {
        _consumers.Add(consumer);
    }

    public IEnumerable<Consumer> GetConsumers()
    {
        return _consumers.ToList();
    }

    public Consumer? GetNextConsumer(string topicName)
    {
        var activeConsumers = _consumers.Where(c => c.IsActive && c.IsSubscribedTo(topicName)).ToList();
        if (!activeConsumers.Any())
            return null;

        // Round-robin selection
        var currentOffset = _topicOffsets.GetOrAdd(topicName, 0);
        var selectedConsumer = activeConsumers[currentOffset % activeConsumers.Count];
        _topicOffsets[topicName] = (currentOffset + 1) % activeConsumers.Count;

        return selectedConsumer;
    }

    public int GetConsumerCount()
    {
        return _consumers.Count;
    }
}
