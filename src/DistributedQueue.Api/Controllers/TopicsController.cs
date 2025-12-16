using DistributedQueue.Api.DTOs;
using DistributedQueue.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace DistributedQueue.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TopicsController : ControllerBase
{
    private readonly ITopicManager _topicManager;
    private readonly ILogger<TopicsController> _logger;

    public TopicsController(ITopicManager topicManager, ILogger<TopicsController> logger)
    {
        _topicManager = topicManager;
        _logger = logger;
    }

    [HttpPost]
    public IActionResult CreateTopic([FromBody] CreateTopicRequest request)
    {
        try
        {
            var topic = _topicManager.CreateTopic(request.TopicName);
            _logger.LogInformation("Topic '{TopicName}' created", request.TopicName);
            return Ok(new { topic.Name, topic.CreatedAt });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult GetAllTopics()
    {
        var topics = _topicManager.GetAllTopics()
            .Select(t => new { t.Name, t.CreatedAt, MessageCount = t.GetMessageCount() });
        return Ok(topics);
    }

    [HttpGet("{topicName}")]
    public IActionResult GetTopic(string topicName)
    {
        var topic = _topicManager.GetTopic(topicName);
        if (topic == null)
        {
            return NotFound(new { error = $"Topic '{topicName}' not found" });
        }

        return Ok(new { topic.Name, topic.CreatedAt, MessageCount = topic.GetMessageCount() });
    }

    [HttpDelete("{topicName}")]
    public IActionResult DeleteTopic(string topicName)
    {
        var deleted = _topicManager.DeleteTopic(topicName);
        if (!deleted)
        {
            return NotFound(new { error = $"Topic '{topicName}' not found" });
        }

        _logger.LogInformation("Topic '{TopicName}' deleted", topicName);
        return Ok(new { message = $"Topic '{topicName}' deleted" });
    }
}
