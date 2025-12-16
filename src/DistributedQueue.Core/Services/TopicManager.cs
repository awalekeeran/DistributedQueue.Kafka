using DistributedQueue.Core.Models;
using System.Collections.Concurrent;

namespace DistributedQueue.Core.Services;

public interface ITopicManager
{
    Topic CreateTopic(string topicName);
    Topic? GetTopic(string topicName);
    IEnumerable<Topic> GetAllTopics();
    bool DeleteTopic(string topicName);
}

public class TopicManager : ITopicManager
{
    private readonly ConcurrentDictionary<string, Topic> _topics;

    public TopicManager()
    {
        _topics = new ConcurrentDictionary<string, Topic>();
    }

    public Topic CreateTopic(string topicName)
    {
        if (_topics.ContainsKey(topicName))
        {
            throw new InvalidOperationException($"Topic '{topicName}' already exists");
        }

        var topic = new Topic(topicName);
        _topics[topicName] = topic;
        return topic;
    }

    public Topic? GetTopic(string topicName)
    {
        _topics.TryGetValue(topicName, out var topic);
        return topic;
    }

    public IEnumerable<Topic> GetAllTopics()
    {
        return _topics.Values;
    }

    public bool DeleteTopic(string topicName)
    {
        return _topics.TryRemove(topicName, out _);
    }
}
