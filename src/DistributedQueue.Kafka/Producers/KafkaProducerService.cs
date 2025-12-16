using Confluent.Kafka;
using DistributedQueue.Core.Models;

namespace DistributedQueue.Kafka.Producers;

/// <summary>
/// Kafka producer implementation for Confluent Cloud
/// This is a placeholder for future Kafka integration
/// </summary>
public interface IKafkaProducerService
{
    Task PublishMessageAsync(string topic, Message message);
}

public class KafkaProducerService : IKafkaProducerService, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ProducerConfig _config;

    public KafkaProducerService(ProducerConfig config)
    {
        _config = config;
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishMessageAsync(string topic, Message message)
    {
        try
        {
            var kafkaMessage = new Message<string, string>
            {
                Key = message.Id,
                Value = message.Content
            };

            var result = await _producer.ProduceAsync(topic, kafkaMessage);
            
            Console.WriteLine($"Message delivered to {result.TopicPartitionOffset}");
        }
        catch (ProduceException<string, string> ex)
        {
            Console.WriteLine($"Delivery failed: {ex.Error.Reason}");
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}
