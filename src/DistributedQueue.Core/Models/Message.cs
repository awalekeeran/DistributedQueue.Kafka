namespace DistributedQueue.Core.Models;

public class Message
{
    public string Id { get; set; }
    public string Content { get; set; }
    public string TopicName { get; set; }
    public DateTime Timestamp { get; set; }
    public string ProducerId { get; set; }

    public Message(string content, string topicName, string producerId)
    {
        Id = Guid.NewGuid().ToString();
        Content = content;
        TopicName = topicName;
        ProducerId = producerId;
        Timestamp = DateTime.UtcNow;
    }
}
