using DistributedQueue.Core.Models;
using System.Collections.Concurrent;

namespace DistributedQueue.Core.Services;

public interface IConsumerGroupManager
{
    ConsumerGroup CreateConsumerGroup(string groupName);
    ConsumerGroup? GetConsumerGroup(string groupName);
    IEnumerable<ConsumerGroup> GetAllConsumerGroups();
    bool DeleteConsumerGroup(string groupName);
    void AddConsumerToGroup(string groupName, Consumer consumer);
}

public class ConsumerGroupManager : IConsumerGroupManager
{
    private readonly ConcurrentDictionary<string, ConsumerGroup> _consumerGroups;

    public ConsumerGroupManager()
    {
        _consumerGroups = new ConcurrentDictionary<string, ConsumerGroup>();
    }

    public ConsumerGroup CreateConsumerGroup(string groupName)
    {
        if (_consumerGroups.ContainsKey(groupName))
        {
            throw new InvalidOperationException($"Consumer group '{groupName}' already exists");
        }

        var group = new ConsumerGroup(groupName);
        _consumerGroups[groupName] = group;
        return group;
    }

    public ConsumerGroup? GetConsumerGroup(string groupName)
    {
        _consumerGroups.TryGetValue(groupName, out var group);
        return group;
    }

    public IEnumerable<ConsumerGroup> GetAllConsumerGroups()
    {
        return _consumerGroups.Values;
    }

    public bool DeleteConsumerGroup(string groupName)
    {
        return _consumerGroups.TryRemove(groupName, out _);
    }

    public void AddConsumerToGroup(string groupName, Consumer consumer)
    {
        var group = GetConsumerGroup(groupName);
        if (group == null)
        {
            throw new InvalidOperationException($"Consumer group '{groupName}' does not exist");
        }

        group.AddConsumer(consumer);
    }
}
