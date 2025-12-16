namespace DistributedQueue.Core.Models;

public class Topic
{
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    private readonly object _lock = new object();
    private readonly Queue<Message> _messages;

    public Topic(string name)
    {
        Name = name;
        CreatedAt = DateTime.UtcNow;
        _messages = new Queue<Message>();
    }

    public void AddMessage(Message message)
    {
        lock (_lock)
        {
            _messages.Enqueue(message);
        }
    }

    public Message? TryDequeueMessage()
    {
        lock (_lock)
        {
            return _messages.Count > 0 ? _messages.Dequeue() : null;
        }
    }

    public int GetMessageCount()
    {
        lock (_lock)
        {
            return _messages.Count;
        }
    }

    public bool HasMessages()
    {
        lock (_lock)
        {
            return _messages.Count > 0;
        }
    }
}
