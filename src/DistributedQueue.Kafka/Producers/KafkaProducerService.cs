using Confluent.Kafka;
using DistributedQueue.Core.Models;

namespace DistributedQueue.Kafka.Producers;

/// <summary>
/// Kafka producer implementation for Confluent Cloud
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
        
        Console.WriteLine("🔧 Initializing Kafka Producer...");
        Console.WriteLine($"   Bootstrap Servers: {config.BootstrapServers}");
        Console.WriteLine($"   Security Protocol: {config.SecurityProtocol}");
        Console.WriteLine($"   SASL Mechanism: {config.SaslMechanism}");
        Console.WriteLine($"   SASL Username: {config.SaslUsername}");
        
        try
        {
            _producer = new ProducerBuilder<string, string>(config)
                .SetErrorHandler((_, e) =>
                {
                    Console.WriteLine($"❌ Kafka Error: {e.Reason} (Code: {e.Code})");
                })
                .SetLogHandler((_, logMessage) =>
                {
                    Console.WriteLine($"📝 Kafka Log [{logMessage.Level}]: {logMessage.Message}");
                })
                .Build();
            
            Console.WriteLine("✅ Kafka Producer initialized successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to initialize Kafka Producer: {ex.Message}");
            throw;
        }
    }

    public async Task PublishMessageAsync(string topic, Message message)
    {
        try
        {
            Console.WriteLine($"📤 Attempting to send message to Kafka topic '{topic}'...");
            
            var kafkaMessage = new Message<string, string>
            {
                Key = message.Id,
                Value = message.Content
            };

            var deliveryResult = await _producer.ProduceAsync(topic, kafkaMessage);
            
            Console.WriteLine($"✅ Message delivered to {deliveryResult.TopicPartitionOffset}");
            Console.WriteLine($"   Topic: {deliveryResult.Topic}");
            Console.WriteLine($"   Partition: {deliveryResult.Partition}");
            Console.WriteLine($"   Offset: {deliveryResult.Offset}");
        }
        catch (ProduceException<string, string> ex)
        {
            Console.WriteLine($"❌ Kafka Delivery failed!");
            Console.WriteLine($"   Error Code: {ex.Error.Code}");
            Console.WriteLine($"   Error Reason: {ex.Error.Reason}");
            Console.WriteLine($"   Is Fatal: {ex.Error.IsFatal}");
            Console.WriteLine($"   Is Broker Error: {ex.Error.IsBrokerError}");
            
            // Don't throw - let it fail gracefully for hybrid mode
            // throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Unexpected error sending to Kafka: {ex.GetType().Name}");
            Console.WriteLine($"   Message: {ex.Message}");
            Console.WriteLine($"   Stack: {ex.StackTrace}");
            
            // Don't throw - let it fail gracefully for hybrid mode
            // throw;
        }
    }

    public void Dispose()
    {
        try
        {
            Console.WriteLine("🔄 Flushing and disposing Kafka Producer...");
            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
            Console.WriteLine("✅ Kafka Producer disposed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error disposing Kafka Producer: {ex.Message}");
        }
    }
}
