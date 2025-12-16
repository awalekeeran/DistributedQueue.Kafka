using DistributedQueue.Core.Models;
using System.Collections.Concurrent;

namespace DistributedQueue.Core.Services;

public interface IConsumerManager
{
    Consumer CreateConsumer(string consumerId, string name, string? consumerGroup = null);
    Consumer? GetConsumer(string consumerId);
    IEnumerable<Consumer> GetAllConsumers();
    bool DeleteConsumer(string consumerId);
    void SubscribeConsumerToTopic(string consumerId, string topicName);
    void UnsubscribeConsumerFromTopic(string consumerId, string topicName);
}

public class ConsumerManager : IConsumerManager
{
    private readonly ConcurrentDictionary<string, Consumer> _consumers;
    private readonly ITopicManager _topicManager;

    public ConsumerManager(ITopicManager topicManager)
    {
        _consumers = new ConcurrentDictionary<string, Consumer>();
        _topicManager = topicManager;
    }

    public Consumer CreateConsumer(string consumerId, string name, string? consumerGroup = null)
    {
        if (_consumers.ContainsKey(consumerId))
        {
            throw new InvalidOperationException($"Consumer '{consumerId}' already exists");
        }

        var consumer = new Consumer(consumerId, name, consumerGroup);
        _consumers[consumerId] = consumer;
        return consumer;
    }

    public Consumer? GetConsumer(string consumerId)
    {
        _consumers.TryGetValue(consumerId, out var consumer);
        return consumer;
    }

    public IEnumerable<Consumer> GetAllConsumers()
    {
        return _consumers.Values;
    }

    public bool DeleteConsumer(string consumerId)
    {
        if (_consumers.TryGetValue(consumerId, out var consumer))
        {
            consumer.Stop();
        }
        return _consumers.TryRemove(consumerId, out _);
    }

    public void SubscribeConsumerToTopic(string consumerId, string topicName)
    {
        var consumer = GetConsumer(consumerId);
        if (consumer == null)
        {
            throw new InvalidOperationException($"Consumer '{consumerId}' does not exist");
        }

        var topic = _topicManager.GetTopic(topicName);
        if (topic == null)
        {
            throw new InvalidOperationException($"Topic '{topicName}' does not exist");
        }

        consumer.SubscribeToTopic(topicName);
    }

    public void UnsubscribeConsumerFromTopic(string consumerId, string topicName)
    {
        var consumer = GetConsumer(consumerId);
        if (consumer == null)
        {
            throw new InvalidOperationException($"Consumer '{consumerId}' does not exist");
        }

        consumer.UnsubscribeFromTopic(topicName);
    }
}
