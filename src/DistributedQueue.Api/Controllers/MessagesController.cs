using DistributedQueue.Api.Configuration;
using DistributedQueue.Api.DTOs;
using DistributedQueue.Api.Services;
using DistributedQueue.Core.Models;
using DistributedQueue.Core.Services;
using DistributedQueue.Kafka.Configuration;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DistributedQueue.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IProducerManager _producerManager;
    private readonly IHybridMessageBroker _hybridBroker;
    private readonly ITopicManager _topicManager;
    private readonly QueueModeSettings _queueMode;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(
        IProducerManager producerManager,
        IHybridMessageBroker hybridBroker,
        ITopicManager topicManager,
        IOptions<QueueModeSettings> queueMode,
        IOptions<KafkaSettings> kafkaSettings,
        ILogger<MessagesController> logger)
    {
        _producerManager = producerManager;
        _hybridBroker = hybridBroker;
        _topicManager = topicManager;
        _queueMode = queueMode.Value;
        _kafkaSettings = kafkaSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Publish message to configured backend(s) (in-memory, Kafka, or both)
    /// </summary>
    [HttpPost("publish")]
    public async Task<IActionResult> PublishMessage([FromBody] PublishMessageRequest request)
    {
        try
        {
            var producer = _producerManager.GetProducer(request.ProducerId);
            if (producer == null)
            {
                return NotFound(new { error = $"Producer '{request.ProducerId}' not found" });
            }

            // Use hybrid broker (supports both in-memory and Kafka)
            await _hybridBroker.PublishMessageAsync(request.ProducerId, request.TopicName, request.Content);

            var message = new Message(request.Content, request.TopicName, request.ProducerId);

            _logger.LogInformation("✅ Message published to topic '{TopicName}' by producer '{ProducerId}'", 
                request.TopicName, request.ProducerId);

            var publishedTo = new List<string>();
            if (_queueMode.UseInMemory) publishedTo.Add("In-Memory");
            if (_queueMode.UseKafka) publishedTo.Add("Kafka");

            return Ok(new 
            { 
                message.Id, 
                message.Content, 
                message.TopicName, 
                message.ProducerId, 
                message.Timestamp,
                Mode = _queueMode.GetMode(),
                PublishedTo = publishedTo,
                Status = "Published successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get messages from a topic (fetches from in-memory or Kafka based on source parameter)
    /// </summary>
    [HttpGet("topic/{topicName}")]
    public async Task<IActionResult> GetMessagesFromTopic(
        string topicName, 
        [FromQuery] string? source = null,
        [FromQuery] int limit = 10,
        [FromQuery] string? offset = "latest")
    {
        source = source?.ToLower();

        // In-memory messages
        if ((source == null || source == "inmemory" || source == "in-memory") && _queueMode.UseInMemory)
        {
            var topic = _topicManager.GetTopic(topicName);
            if (topic == null)
            {
                return NotFound(new { error = $"In-memory topic '{topicName}' not found" });
            }

            return Ok(new
            {
                Source = "In-Memory",
                TopicName = topicName,
                MessageCount = topic.GetMessageCount(),
                HasMessages = topic.HasMessages()
            });
        }

        // Kafka messages - actually fetch them
        if ((source == null || source == "kafka") && _queueMode.UseKafka && _kafkaSettings.IsValid())
        {
            try
            {
                var messages = await FetchKafkaMessagesAsync(topicName, limit, offset);
                
                return Ok(new
                {
                    Source = "Kafka",
                    TopicName = topicName,
                    MessageCount = messages.Count,
                    Messages = messages,
                    Limit = limit,
                    Offset = offset
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching messages from Kafka topic '{TopicName}'", topicName);
                return BadRequest(new { error = $"Failed to fetch messages from Kafka: {ex.Message}" });
            }
        }

        if (source == "kafka" && !_queueMode.UseKafka)
        {
            return BadRequest(new { error = "Kafka is not enabled in current mode" });
        }

        if (source == "inmemory" && !_queueMode.UseInMemory)
        {
            return BadRequest(new { error = "In-memory mode is not enabled in current mode" });
        }

        return BadRequest(new { error = "Invalid source. Use 'inmemory' or 'kafka'" });
    }

    private async Task<List<object>> FetchKafkaMessagesAsync(string topicName, int limit, string? offset)
    {
        var messages = new List<object>();
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            GroupId = $"api-viewer-{Guid.NewGuid()}",
            AutoOffsetReset = offset?.ToLower() == "earliest" ? AutoOffsetReset.Earliest : AutoOffsetReset.Latest,
            EnableAutoCommit = false,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = _kafkaSettings.SaslUsername,
            SaslPassword = _kafkaSettings.SaslPassword
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        
        consumer.Subscribe(topicName);

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(5)); // 5 second timeout

        try
        {
            while (messages.Count < limit && !cts.Token.IsCancellationRequested)
            {
                var consumeResult = consumer.Consume(cts.Token);
                
                if (consumeResult != null && consumeResult.Message != null)
                {
                    messages.Add(new
                    {
                        Key = consumeResult.Message.Key,
                        Value = consumeResult.Message.Value,
                        Partition = consumeResult.Partition.Value,
                        Offset = consumeResult.Offset.Value,
                        Timestamp = consumeResult.Message.Timestamp.UtcDateTime
                    });
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Timeout reached - return whatever we got
            _logger.LogInformation("Reached timeout while fetching messages. Retrieved {Count} messages", messages.Count);
        }
        finally
        {
            consumer.Close();
        }

        return messages;
    }

    /// <summary>
    /// Get message statistics across all topics
    /// </summary>
    [HttpGet("stats")]
    public IActionResult GetMessageStats()
    {
        var stats = new
        {
            Mode = _queueMode.GetMode(),
            InMemory = _queueMode.UseInMemory ? new
            {
                Topics = _topicManager.GetAllTopics().Select(t => new
                {
                    t.Name,
                    MessageCount = t.GetMessageCount(),
                    HasMessages = t.HasMessages()
                })
            } : null,
            Kafka = _queueMode.UseKafka && _kafkaSettings.IsValid() ? new
            {
                Note = "Kafka message statistics require consumer group offset information. Use Confluent Cloud UI or Kafka tools for detailed metrics."
            } : null
        };

        return Ok(stats);
    }
}
