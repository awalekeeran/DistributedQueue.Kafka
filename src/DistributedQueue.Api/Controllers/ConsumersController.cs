using DistributedQueue.Api.Configuration;
using DistributedQueue.Api.DTOs;
using DistributedQueue.Core.Services;
using DistributedQueue.Kafka.Configuration;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DistributedQueue.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsumersController : ControllerBase
{
    private readonly IConsumerManager _consumerManager;
    private readonly IConsumerGroupManager _consumerGroupManager;
    private readonly IMessageBroker _messageBroker;
    private readonly QueueModeSettings _queueMode;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<ConsumersController> _logger;

    public ConsumersController(
        IConsumerManager consumerManager,
        IConsumerGroupManager consumerGroupManager,
        IMessageBroker messageBroker,
        IOptions<QueueModeSettings> queueMode,
        IOptions<KafkaSettings> kafkaSettings,
        ILogger<ConsumersController> logger)
    {
        _consumerManager = consumerManager;
        _consumerGroupManager = consumerGroupManager;
        _messageBroker = messageBroker;
        _queueMode = queueMode.Value;
        _kafkaSettings = kafkaSettings.Value;
        _logger = logger;
    }

    [HttpPost]
    public IActionResult CreateConsumer([FromBody] CreateConsumerRequest request)
    {
        try
        {
            var consumer = _consumerManager.CreateConsumer(request.ConsumerId, request.Name, request.ConsumerGroup);
            
            // If consumer group is specified, add consumer to the group
            if (!string.IsNullOrEmpty(request.ConsumerGroup))
            {
                var group = _consumerGroupManager.GetConsumerGroup(request.ConsumerGroup);
                if (group == null)
                {
                    // Create group if it doesn't exist
                    group = _consumerGroupManager.CreateConsumerGroup(request.ConsumerGroup);
                }
                _consumerGroupManager.AddConsumerToGroup(request.ConsumerGroup, consumer);
            }

            _logger.LogInformation("Consumer '{ConsumerId}' created", request.ConsumerId);
            return Ok(new { consumer.Id, consumer.Name, consumer.ConsumerGroup, consumer.CreatedAt });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all consumers from configured sources (in-memory and/or Kafka consumer group members)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllConsumers()
    {
        var inMemoryConsumers = new List<object>();
        var kafkaConsumers = new List<object>();

        // Get in-memory consumers
        if (_queueMode.UseInMemory)
        {
            inMemoryConsumers = _consumerManager.GetAllConsumers()
                .Select(c => new 
                { 
                    Id = c.Id, 
                    Name = c.Name, 
                    ConsumerGroup = c.ConsumerGroup, 
                    IsActive = c.IsActive, 
                    CreatedAt = c.CreatedAt,
                    SubscribedTopics = c.GetSubscribedTopics(),
                    Source = "In-Memory"
                })
                .Cast<object>()
                .ToList();

            _logger.LogInformation("Found {Count} in-memory consumers", inMemoryConsumers.Count);
        }

        // Get Kafka consumers (from consumer groups)
        if (_queueMode.UseKafka && _kafkaSettings.IsValid())
        {
            try
            {
                var adminConfig = _kafkaSettings.GetAdminClientConfig();

                using var adminClient = new AdminClientBuilder(adminConfig).Build();
                var groupsList = adminClient.ListGroups(TimeSpan.FromSeconds(10));

                // Extract all members from all consumer groups
                kafkaConsumers = groupsList
                    .Where(g => g.Members != null && g.Members.Count > 0)
                    .SelectMany(g => g.Members.Select(m => new
                    {
                        Id = m.MemberId,
                        ClientId = m.ClientId,
                        ClientHost = m.ClientHost,
                        ConsumerGroup = g.Group,
                        Protocol = g.ProtocolType,
                        GroupState = g.State,
                        Source = "Kafka"
                    }))
                    .Cast<object>()
                    .ToList();

                _logger.LogInformation("Found {Count} Kafka consumer group members", kafkaConsumers.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Kafka consumers");
            }
        }

        // Combine both sources if hybrid mode
        if (_queueMode.EnableHybridMode)
        {
            var combined = inMemoryConsumers.Concat(kafkaConsumers).ToList();
            return Ok(new
            {
                Mode = _queueMode.GetMode(),
                InMemoryConsumers = inMemoryConsumers,
                KafkaConsumers = kafkaConsumers,
                Combined = combined,
                Summary = new
                {
                    TotalConsumers = combined.Count,
                    InMemoryCount = inMemoryConsumers.Count,
                    KafkaCount = kafkaConsumers.Count
                }
            });
        }

        // Return only the active source
        if (_queueMode.UseKafka)
        {
            return Ok(new
            {
                Mode = _queueMode.GetMode(),
                Consumers = kafkaConsumers,
                Summary = new { TotalConsumers = kafkaConsumers.Count },
                Note = "Kafka consumers are shown as active members of consumer groups"
            });
        }

        return Ok(new
        {
            Mode = _queueMode.GetMode(),
            Consumers = inMemoryConsumers,
            Summary = new { TotalConsumers = inMemoryConsumers.Count }
        });
    }

    [HttpGet("{consumerId}")]
    public IActionResult GetConsumer(string consumerId)
    {
        var consumer = _consumerManager.GetConsumer(consumerId);
        if (consumer == null)
        {
            return NotFound(new { error = $"Consumer '{consumerId}' not found" });
        }

        return Ok(new 
        { 
            consumer.Id, 
            consumer.Name, 
            consumer.ConsumerGroup, 
            consumer.IsActive, 
            consumer.CreatedAt,
            SubscribedTopics = consumer.GetSubscribedTopics()
        });
    }

    [HttpPost("subscribe")]
    public IActionResult Subscribe([FromBody] SubscribeRequest request)
    {
        try
        {
            _consumerManager.SubscribeConsumerToTopic(request.ConsumerId, request.TopicName);
            _logger.LogInformation("Consumer '{ConsumerId}' subscribed to topic '{TopicName}'", 
                request.ConsumerId, request.TopicName);
            return Ok(new { message = $"Consumer '{request.ConsumerId}' subscribed to topic '{request.TopicName}'" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{consumerId}/start")]
    public IActionResult StartConsumer(string consumerId)
    {
        try
        {
            _messageBroker.StartConsumer(consumerId);
            _logger.LogInformation("Consumer '{ConsumerId}' started", consumerId);
            return Ok(new { message = $"Consumer '{consumerId}' started" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{consumerId}/stop")]
    public IActionResult StopConsumer(string consumerId)
    {
        try
        {
            _messageBroker.StopConsumer(consumerId);
            _logger.LogInformation("Consumer '{ConsumerId}' stopped", consumerId);
            return Ok(new { message = $"Consumer '{consumerId}' stopped" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{consumerId}")]
    public IActionResult DeleteConsumer(string consumerId)
    {
        var deleted = _consumerManager.DeleteConsumer(consumerId);
        if (!deleted)
        {
            return NotFound(new { error = $"Consumer '{consumerId}' not found" });
        }

        _logger.LogInformation("Consumer '{ConsumerId}' deleted", consumerId);
        return Ok(new { message = $"Consumer '{consumerId}' deleted" });
    }
}
