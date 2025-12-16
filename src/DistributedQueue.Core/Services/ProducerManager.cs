using DistributedQueue.Core.Models;
using System.Collections.Concurrent;

namespace DistributedQueue.Core.Services;

public interface IProducerManager
{
    Producer CreateProducer(string producerId, string name);
    Producer? GetProducer(string producerId);
    IEnumerable<Producer> GetAllProducers();
    bool DeleteProducer(string producerId);
}

public class ProducerManager : IProducerManager
{
    private readonly ConcurrentDictionary<string, Producer> _producers;

    public ProducerManager()
    {
        _producers = new ConcurrentDictionary<string, Producer>();
    }

    public Producer CreateProducer(string producerId, string name)
    {
        if (_producers.ContainsKey(producerId))
        {
            throw new InvalidOperationException($"Producer '{producerId}' already exists");
        }

        var producer = new Producer(producerId, name);
        _producers[producerId] = producer;
        return producer;
    }

    public Producer? GetProducer(string producerId)
    {
        _producers.TryGetValue(producerId, out var producer);
        return producer;
    }

    public IEnumerable<Producer> GetAllProducers()
    {
        return _producers.Values;
    }

    public bool DeleteProducer(string producerId)
    {
        return _producers.TryRemove(producerId, out _);
    }
}
