using System.Collections.Concurrent;

namespace DistributedQueue.Core.Models;

public class Consumer
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? ConsumerGroup { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    
    private readonly ConcurrentBag<string> _subscribedTopics;
    private CancellationTokenSource? _cancellationTokenSource;

    public Consumer(string id, string name, string? consumerGroup = null)
    {
        Id = id;
        Name = name;
        ConsumerGroup = consumerGroup;
        CreatedAt = DateTime.UtcNow;
        IsActive = false;
        _subscribedTopics = new ConcurrentBag<string>();
    }

    public void SubscribeToTopic(string topicName)
    {
        if (!_subscribedTopics.Contains(topicName))
        {
            _subscribedTopics.Add(topicName);
        }
    }

    public void UnsubscribeFromTopic(string topicName)
    {
        _subscribedTopics.TryTake(out _);
    }

    public IEnumerable<string> GetSubscribedTopics()
    {
        return _subscribedTopics.ToList();
    }

    public bool IsSubscribedTo(string topicName)
    {
        return _subscribedTopics.Contains(topicName);
    }

    public void OnMessageReceived(Message message)
    {
        Console.WriteLine($"{Id} received {message.Content}");
    }

    public CancellationTokenSource GetCancellationTokenSource()
    {
        if (_cancellationTokenSource == null)
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }
        return _cancellationTokenSource;
    }

    public void Stop()
    {
        IsActive = false;
        _cancellationTokenSource?.Cancel();
    }
}
