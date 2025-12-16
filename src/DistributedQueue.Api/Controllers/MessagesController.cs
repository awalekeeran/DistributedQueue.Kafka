using DistributedQueue.Api.DTOs;
using DistributedQueue.Core.Models;
using DistributedQueue.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace DistributedQueue.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IProducerManager _producerManager;
    private readonly IMessageBroker _messageBroker;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(
        IProducerManager producerManager,
        IMessageBroker messageBroker,
        ILogger<MessagesController> logger)
    {
        _producerManager = producerManager;
        _messageBroker = messageBroker;
        _logger = logger;
    }

    [HttpPost("publish")]
    public IActionResult PublishMessage([FromBody] PublishMessageRequest request)
    {
        try
        {
            var producer = _producerManager.GetProducer(request.ProducerId);
            if (producer == null)
            {
                return NotFound(new { error = $"Producer '{request.ProducerId}' not found" });
            }

            var message = new Message(request.Content, request.TopicName, request.ProducerId);
            _messageBroker.PublishMessage(request.TopicName, message);

            _logger.LogInformation("Message published to topic '{TopicName}' by producer '{ProducerId}'", 
                request.TopicName, request.ProducerId);

            return Ok(new 
            { 
                message.Id, 
                message.Content, 
                message.TopicName, 
                message.ProducerId, 
                message.Timestamp 
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
