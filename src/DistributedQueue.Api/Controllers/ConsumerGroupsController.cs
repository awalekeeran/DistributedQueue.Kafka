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
public class ConsumerGroupsController : ControllerBase
{
    private readonly IConsumerGroupManager _consumerGroupManager;
    private readonly QueueModeSettings _queueMode;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<ConsumerGroupsController> _logger;

    public ConsumerGroupsController(
        IConsumerGroupManager consumerGroupManager,
        IOptions<QueueModeSettings> queueMode,
        IOptions<KafkaSettings> kafkaSettings,
        ILogger<ConsumerGroupsController> logger)
    {
        _consumerGroupManager = consumerGroupManager;
        _queueMode = queueMode.Value;
        _kafkaSettings = kafkaSettings.Value;
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

    /// <summary>
    /// Get all consumer groups from configured sources (in-memory, Kafka, or both)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllConsumerGroups()
    {
        var inMemoryGroups = new List<object>();
        var kafkaGroups = new List<object>();

        // Get in-memory consumer groups
        if (_queueMode.UseInMemory)
        {
            inMemoryGroups = _consumerGroupManager.GetAllConsumerGroups()
                .Select(g => new 
                { 
                    Name = g.Name, 
                    CreatedAt = g.CreatedAt,
                    ConsumerCount = g.GetConsumerCount(),
                    Consumers = g.GetConsumers().Select(c => new { c.Id, c.Name }),
                    Source = "In-Memory"
                })
                .Cast<object>()
                .ToList();

            _logger.LogInformation("Found {Count} in-memory consumer groups", inMemoryGroups.Count);
        }

        // Get Kafka consumer groups
        if (_queueMode.UseKafka && _kafkaSettings.IsValid())
        {
            try
            {
                var adminConfig = _kafkaSettings.GetAdminClientConfig();

                using var adminClient = new AdminClientBuilder(adminConfig).Build();
                var groupsList = adminClient.ListGroups(TimeSpan.FromSeconds(10));

                kafkaGroups = groupsList
                    .Where(g => !string.IsNullOrEmpty(g.Group)) // Exclude empty group names
                    .Select(g => new
                    {
                        Name = g.Group,
                        Protocol = g.ProtocolType,
                        State = g.State,
                        MemberCount = g.Members?.Count ?? 0,
                        Source = "Kafka"
                    })
                    .Cast<object>()
                    .ToList();

                _logger.LogInformation("Found {Count} Kafka consumer groups", kafkaGroups.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Kafka consumer groups");
            }
        }

        // Combine both sources if hybrid mode
        if (_queueMode.EnableHybridMode)
        {
            var combined = inMemoryGroups.Concat(kafkaGroups).ToList();
            return Ok(new
            {
                Mode = _queueMode.GetMode(),
                InMemoryGroups = inMemoryGroups,
                KafkaGroups = kafkaGroups,
                Combined = combined,
                Summary = new
                {
                    TotalGroups = combined.Count,
                    InMemoryCount = inMemoryGroups.Count,
                    KafkaCount = kafkaGroups.Count
                }
            });
        }

        // Return only the active source
        if (_queueMode.UseKafka)
        {
            return Ok(new
            {
                Mode = _queueMode.GetMode(),
                Groups = kafkaGroups,
                Summary = new { TotalGroups = kafkaGroups.Count }
            });
        }

        return Ok(new
        {
            Mode = _queueMode.GetMode(),
            Groups = inMemoryGroups,
            Summary = new { TotalGroups = inMemoryGroups.Count }
        });
    }

    /// <summary>
    /// Get consumer group details with optional source filter (?source=inmemory or ?source=kafka)
    /// </summary>
    [HttpGet("{groupName}")]
    public async Task<IActionResult> GetConsumerGroup(string groupName, [FromQuery] string? source = null)
    {
        source = source?.ToLower();

        // If source is specified, get from that specific source
        if (!string.IsNullOrEmpty(source))
        {
            if (source == "inmemory" || source == "in-memory")
            {
                var group = _consumerGroupManager.GetConsumerGroup(groupName);
                if (group == null)
                {
                    return NotFound(new { error = $"In-memory consumer group '{groupName}' not found" });
                }

                return Ok(new 
                { 
                    Source = "In-Memory",
                    group.Name, 
                    group.CreatedAt,
                    ConsumerCount = group.GetConsumerCount(),
                    Consumers = group.GetConsumers().Select(c => new { c.Id, c.Name })
                });
            }

            if (source == "kafka")
            {
                if (!_queueMode.UseKafka || !_kafkaSettings.IsValid())
                {
                    return BadRequest(new { error = "Kafka is not enabled or configured" });
                }

                return await GetKafkaConsumerGroupDetails(groupName);
            }

            return BadRequest(new { error = "Invalid source. Use 'inmemory' or 'kafka'" });
        }

        // No source specified - get from configured sources
        var responses = new List<object>();

        if (_queueMode.UseInMemory)
        {
            var group = _consumerGroupManager.GetConsumerGroup(groupName);
            if (group != null)
            {
                responses.Add(new 
                { 
                    Source = "In-Memory",
                    group.Name, 
                    group.CreatedAt,
                    ConsumerCount = group.GetConsumerCount(),
                    Consumers = group.GetConsumers().Select(c => new { c.Id, c.Name })
                });
            }
        }

        if (_queueMode.UseKafka && _kafkaSettings.IsValid())
        {
            var kafkaResult = await GetKafkaConsumerGroupDetails(groupName);
            if (kafkaResult is OkObjectResult okResult)
            {
                responses.Add(okResult.Value!);
            }
        }

        if (responses.Count == 0)
        {
            return NotFound(new { error = $"Consumer group '{groupName}' not found in any configured source" });
        }

        if (_queueMode.EnableHybridMode && responses.Count > 1)
        {
            return Ok(new
            {
                Mode = "Hybrid",
                GroupName = groupName,
                Sources = responses
            });
        }

        return Ok(responses.First());
    }

    private async Task<IActionResult> GetKafkaConsumerGroupDetails(string groupName)
    {
        try
        {
            var adminConfig = _kafkaSettings.GetAdminClientConfig();

            using var adminClient = new AdminClientBuilder(adminConfig).Build();
            
            // List consumer group offsets to get details
            var groupsList = adminClient.ListGroups(TimeSpan.FromSeconds(10));
            var groupInfo = groupsList.FirstOrDefault(g => g.Group == groupName);

            if (groupInfo == null)
            {
                return NotFound(new { error = $"Kafka consumer group '{groupName}' not found" });
            }

            return Ok(new
            {
                Source = "Kafka",
                Name = groupInfo.Group,
                Protocol = groupInfo.ProtocolType,
                State = groupInfo.State,
                Members = groupInfo.Members?.Select(m => new
                {
                    MemberId = m.MemberId,
                    ClientId = m.ClientId,
                    ClientHost = m.ClientHost
                }),
                MemberCount = groupInfo.Members?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Kafka consumer group details");
            return BadRequest(new { error = ex.Message });
        }
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
