using Confluent.Kafka;

namespace DistributedQueue.Kafka.Consumers;

/// <summary>
/// Kafka consumer implementation for Confluent Cloud
/// This is a placeholder for future Kafka integration
/// </summary>
public interface IKafkaConsumerService
{
    void StartConsuming(string topic, Action<string> messageHandler, CancellationToken cancellationToken);
}

public class KafkaConsumerService : IKafkaConsumerService, IDisposable
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ConsumerConfig _config;

    public KafkaConsumerService(ConsumerConfig config)
    {
        _config = config;
        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    public void StartConsuming(string topic, Action<string> messageHandler, CancellationToken cancellationToken)
    {
        _consumer.Subscribe(topic);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken);
                    
                    if (consumeResult != null && consumeResult.Message != null)
                    {
                        messageHandler(consumeResult.Message.Value);
                        
                        // Commit the offset
                        _consumer.Commit(consumeResult);
                    }
                }
                catch (ConsumeException ex)
                {
                    Console.WriteLine($"Error consuming message: {ex.Error.Reason}");
                }
            }
        }
        finally
        {
            _consumer.Close();
        }
    }

    public void Dispose()
    {
        _consumer?.Dispose();
    }
}
