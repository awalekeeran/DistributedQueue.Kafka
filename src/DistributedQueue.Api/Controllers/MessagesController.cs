using DistributedQueue.Api.DTOs;
using DistributedQueue.Api.Services;
using DistributedQueue.Core.Models;
using DistributedQueue.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace DistributedQueue.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IProducerManager _producerManager;
    private readonly IHybridMessageBroker _hybridBroker;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(
        IProducerManager producerManager,
        IHybridMessageBroker hybridBroker,
        ILogger<MessagesController> logger)
    {
        _producerManager = producerManager;
        _hybridBroker = hybridBroker;
        _logger = logger;
    }

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

            return Ok(new 
            { 
                message.Id, 
                message.Content, 
                message.TopicName, 
                message.ProducerId, 
                message.Timestamp,
                Status = "Published to configured backend(s)"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
