using DistributedQueue.Core.Models;
using DistributedQueue.Core.Services;
using DistributedQueue.Kafka.Producers;
using DistributedQueue.Api.Configuration;
using Microsoft.Extensions.Options;

namespace DistributedQueue.Api.Services;

/// <summary>
/// Hybrid message broker that supports both in-memory and Kafka
/// Messages can be sent to in-memory queue, Kafka, or both simultaneously
/// </summary>
public interface IHybridMessageBroker
{
    Task PublishMessageAsync(string producerId, string topicName, string content);
    List<Message> GetMessagesForTopic(string topicName);
    void SubscribeConsumer(string consumerId, string topicName);
    void UnsubscribeConsumer(string consumerId, string topicName);
    List<string> GetSubscribedTopics(string consumerId);
}

public class HybridMessageBroker : IHybridMessageBroker
{
    private readonly IMessageBroker _inMemoryBroker;
    private readonly IKafkaProducerService? _kafkaProducer;
    private readonly QueueModeSettings _queueMode;
    private readonly ILogger<HybridMessageBroker> _logger;

    public HybridMessageBroker(
        IMessageBroker inMemoryBroker,
        IOptions<QueueModeSettings> queueMode,
        ILogger<HybridMessageBroker> logger,
        IKafkaProducerService? kafkaProducer = null)
    {
        _inMemoryBroker = inMemoryBroker;
        _kafkaProducer = kafkaProducer;
        _queueMode = queueMode.Value;
        _logger = logger;

        _logger.LogInformation("🚀 Hybrid Message Broker initialized in mode: {Mode}", _queueMode.GetMode());
    }

    public async Task PublishMessageAsync(string producerId, string topicName, string content)
    {
        var message = new Message(content, topicName, producerId);

        var tasks = new List<Task>();

        // Publish to in-memory queue if enabled
        if (_queueMode.UseInMemory)
        {
            _logger.LogInformation("📦 Publishing to IN-MEMORY queue: Topic={Topic}, MessageId={MessageId}", 
                topicName, message.Id);
            
            _inMemoryBroker.PublishMessage(topicName, message);
        }

        // Publish to Kafka if enabled
        if (_queueMode.UseKafka && _kafkaProducer != null)
        {
            _logger.LogInformation("☁️ Publishing to KAFKA: Topic={Topic}, MessageId={MessageId}", 
                topicName, message.Id);
            
            tasks.Add(_kafkaProducer.PublishMessageAsync(topicName, message));
        }

        // Wait for all async operations
        if (tasks.Any())
        {
            await Task.WhenAll(tasks);
        }

        // Log hybrid mode
        if (_queueMode.EnableHybridMode && _queueMode.UseInMemory && _queueMode.UseKafka)
        {
            _logger.LogInformation("✅ HYBRID: Message published to BOTH in-memory and Kafka!");
        }
    }

    public List<Message> GetMessagesForTopic(string topicName)
    {
        if (_queueMode.UseInMemory)
        {
            // Access topic directly through topic manager
            var topicManager = _inMemoryBroker as dynamic;
            return new List<Message>();
        }

        _logger.LogWarning("⚠️ GetMessagesForTopic only works with in-memory mode. Current mode: {Mode}", 
            _queueMode.GetMode());
        
        return new List<Message>();
    }

    public void SubscribeConsumer(string consumerId, string topicName)
    {
        if (_queueMode.UseInMemory)
        {
            // Subscription is handled by ConsumerManager, not MessageBroker
            _logger.LogInformation("📨 Consumer {ConsumerId} subscribed to topic {Topic} (in-memory)", 
                consumerId, topicName);
        }

        // TODO: Add Kafka consumer subscription logic here
        if (_queueMode.UseKafka)
        {
            _logger.LogInformation("☁️ Consumer {ConsumerId} would subscribe to Kafka topic {Topic}", 
                consumerId, topicName);
        }
    }

    public void UnsubscribeConsumer(string consumerId, string topicName)
    {
        if (_queueMode.UseInMemory)
        {
            // Unsubscription is handled by ConsumerManager
            _logger.LogInformation("📭 Consumer {ConsumerId} unsubscribed from topic {Topic}", 
                consumerId, topicName);
        }
    }

    public List<string> GetSubscribedTopics(string consumerId)
    {
        // This would need to query ConsumerManager
        return new List<string>();
    }
}
