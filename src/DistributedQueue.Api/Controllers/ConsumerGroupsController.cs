using DistributedQueue.Api.DTOs;
using DistributedQueue.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace DistributedQueue.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsumerGroupsController : ControllerBase
{
    private readonly IConsumerGroupManager _consumerGroupManager;
    private readonly ILogger<ConsumerGroupsController> _logger;

    public ConsumerGroupsController(
        IConsumerGroupManager consumerGroupManager,
        ILogger<ConsumerGroupsController> logger)
    {
        _consumerGroupManager = consumerGroupManager;
        _logger = logger;
    }

    [HttpPost]
    public IActionResult CreateConsumerGroup([FromBody] CreateConsumerGroupRequest request)
    {
        try
        {
            var group = _consumerGroupManager.CreateConsumerGroup(request.GroupName);
            _logger.LogInformation("Consumer group '{GroupName}' created", request.GroupName);
            return Ok(new { group.Name, group.CreatedAt });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult GetAllConsumerGroups()
    {
        var groups = _consumerGroupManager.GetAllConsumerGroups()
            .Select(g => new 
            { 
                g.Name, 
                g.CreatedAt,
                ConsumerCount = g.GetConsumerCount(),
                Consumers = g.GetConsumers().Select(c => new { c.Id, c.Name })
            });
        return Ok(groups);
    }

    [HttpGet("{groupName}")]
    public IActionResult GetConsumerGroup(string groupName)
    {
        var group = _consumerGroupManager.GetConsumerGroup(groupName);
        if (group == null)
        {
            return NotFound(new { error = $"Consumer group '{groupName}' not found" });
        }

        return Ok(new 
        { 
            group.Name, 
            group.CreatedAt,
            ConsumerCount = group.GetConsumerCount(),
            Consumers = group.GetConsumers().Select(c => new { c.Id, c.Name })
        });
    }

    [HttpDelete("{groupName}")]
    public IActionResult DeleteConsumerGroup(string groupName)
    {
        var deleted = _consumerGroupManager.DeleteConsumerGroup(groupName);
        if (!deleted)
        {
            return NotFound(new { error = $"Consumer group '{groupName}' not found" });
        }

        _logger.LogInformation("Consumer group '{GroupName}' deleted", groupName);
        return Ok(new { message = $"Consumer group '{groupName}' deleted" });
    }
}
