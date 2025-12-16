using DistributedQueue.Api.DTOs;
using DistributedQueue.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace DistributedQueue.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsumersController : ControllerBase
{
    private readonly IConsumerManager _consumerManager;
    private readonly IConsumerGroupManager _consumerGroupManager;
    private readonly IMessageBroker _messageBroker;
    private readonly ILogger<ConsumersController> _logger;

    public ConsumersController(
        IConsumerManager consumerManager,
        IConsumerGroupManager consumerGroupManager,
        IMessageBroker messageBroker,
        ILogger<ConsumersController> logger)
    {
        _consumerManager = consumerManager;
        _consumerGroupManager = consumerGroupManager;
        _messageBroker = messageBroker;
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

    [HttpGet]
    public IActionResult GetAllConsumers()
    {
        var consumers = _consumerManager.GetAllConsumers()
            .Select(c => new 
            { 
                c.Id, 
                c.Name, 
                c.ConsumerGroup, 
                c.IsActive, 
                c.CreatedAt,
                SubscribedTopics = c.GetSubscribedTopics()
            });
        return Ok(consumers);
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
