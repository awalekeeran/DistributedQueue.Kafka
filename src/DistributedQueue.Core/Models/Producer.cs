namespace DistributedQueue.Core.Models;

public class Producer
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }

    public Producer(string id, string name)
    {
        Id = id;
        Name = name;
        CreatedAt = DateTime.UtcNow;
    }
}
