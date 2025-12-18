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
public class TopicsController : ControllerBase
{
    private readonly ITopicManager _topicManager;
    private readonly QueueModeSettings _queueMode;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<TopicsController> _logger;

    public TopicsController(
        ITopicManager topicManager,
        IOptions<QueueModeSettings> queueMode,
        IOptions<KafkaSettings> kafkaSettings,
        ILogger<TopicsController> logger)
    {
        _topicManager = topicManager;
        _queueMode = queueMode.Value;
        _kafkaSettings = kafkaSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Get all topics from configured sources (in-memory, Kafka, or both)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllTopics()
    {
        var inMemoryTopics = new List<object>();
        var kafkaTopics = new List<object>();

        // Get in-memory topics
        if (_queueMode.UseInMemory)
        {
            inMemoryTopics = _topicManager.GetAllTopics()
                .Select(t => new
                {
                    Name = t.Name,
                    CreatedAt = t.CreatedAt,
                    MessageCount = t.GetMessageCount(),
                    Source = "In-Memory"
                })
                .Cast<object>()
                .ToList();

            _logger.LogInformation("Found {Count} in-memory topics", inMemoryTopics.Count);
        }

        // Get Kafka topics
        if (_queueMode.UseKafka && _kafkaSettings.IsValid())
        {
            try
            {
                var adminConfig = _kafkaSettings.GetAdminClientConfig();

                using var adminClient = new AdminClientBuilder(adminConfig).Build();
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));

                kafkaTopics = metadata.Topics
                    .Where(t => !t.Topic.StartsWith("_")) // Exclude internal topics
                    .Select(t => new
                    {
                        Name = t.Topic,
                        Partitions = t.Partitions.Count,
                        Source = "Kafka",
                        Error = t.Error?.Reason
                    })
                    .Cast<object>()
                    .ToList();

                _logger.LogInformation("Found {Count} Kafka topics", kafkaTopics.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Kafka topics");
            }
        }

        // Combine both sources if hybrid mode
        if (_queueMode.EnableHybridMode)
        {
            var combined = inMemoryTopics.Concat(kafkaTopics).ToList();
            return Ok(new
            {
                Mode = _queueMode.GetMode(),
                InMemoryTopics = inMemoryTopics,
                KafkaTopics = kafkaTopics,
                Combined = combined,
                Summary = new
                {
                    TotalTopics = combined.Count,
                    InMemoryCount = inMemoryTopics.Count,
                    KafkaCount = kafkaTopics.Count
                }
            });
        }

        // Return only the active source
        if (_queueMode.UseKafka)
        {
            return Ok(new
            {
                Mode = _queueMode.GetMode(),
                Topics = kafkaTopics,
                Summary = new { TotalTopics = kafkaTopics.Count }
            });
        }

        return Ok(new
        {
            Mode = _queueMode.GetMode(),
            Topics = inMemoryTopics,
            Summary = new { TotalTopics = inMemoryTopics.Count }
        });
    }

    /// <summary>
    /// Create topic in configured sources (in-memory, Kafka, or both)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTopic([FromBody] CreateTopicRequest request)
    {
        var results = new List<string>();
        var success = true;

        // Create in-memory topic
        if (_queueMode.UseInMemory)
        {
            try
            {
                var topic = _topicManager.CreateTopic(request.TopicName);
                results.Add($"✅ Created in-memory topic: {topic.Name}");
                _logger.LogInformation("In-memory topic '{TopicName}' created", request.TopicName);
            }
            catch (InvalidOperationException ex)
            {
                results.Add($"⚠️ In-memory: {ex.Message}");
                _logger.LogWarning("In-memory topic creation failed: {Message}", ex.Message);
            }
        }

        // Create Kafka topic
        if (_queueMode.UseKafka && _kafkaSettings.IsValid())
        {
            try
            {
                var adminConfig = _kafkaSettings.GetAdminClientConfig();

                using var adminClient = new AdminClientBuilder(adminConfig).Build();

                await adminClient.CreateTopicsAsync(new[]
                {
                    new TopicSpecification
                    {
                        Name = request.TopicName,
                        NumPartitions = 3,
                        ReplicationFactor = 3
                    }
                });

                results.Add($"✅ Created Kafka topic: {request.TopicName} (3 partitions, RF=3)");
                _logger.LogInformation("Kafka topic '{TopicName}' created", request.TopicName);
            }
            catch (CreateTopicsException ex)
            {
                foreach (var topicResult in ex.Results)
                {
                    if (topicResult.Error.Code == ErrorCode.TopicAlreadyExists)
                    {
                        results.Add($"⚠️ Kafka topic already exists: {topicResult.Topic}");
                    }
                    else
                    {
                        results.Add($"❌ Kafka error: {topicResult.Error.Reason}");
                        success = false;
                    }
                }
            }
            catch (Exception ex)
            {
                results.Add($"❌ Kafka connection error: {ex.Message}");
                success = false;
            }
        }

        if (_queueMode.EnableHybridMode && _queueMode.UseInMemory && _queueMode.UseKafka)
        {
            results.Add("🔄 Hybrid mode: Topic created in BOTH systems!");
        }

        return success ? Ok(new { topicName = request.TopicName, mode = _queueMode.GetMode(), results }) 
                       : BadRequest(new { topicName = request.TopicName, mode = _queueMode.GetMode(), results });
    }

    /// <summary>
    /// Get topic details with optional source filter (?source=inmemory or ?source=kafka)
    /// </summary>
    [HttpGet("{topicName}")]
    public async Task<IActionResult> GetTopic(string topicName, [FromQuery] string? source = null)
    {
        source = source?.ToLower();

        // If source is specified, get from that specific source
        if (!string.IsNullOrEmpty(source))
        {
            if (source == "inmemory" || source == "in-memory")
            {
                var topic = _topicManager.GetTopic(topicName);
                if (topic == null)
                {
                    return NotFound(new { error = $"In-memory topic '{topicName}' not found" });
                }

                return Ok(new
                {
                    Source = "In-Memory",
                    topic.Name,
                    topic.CreatedAt,
                    MessageCount = topic.GetMessageCount(),
                    HasMessages = topic.HasMessages()
                });
            }

            if (source == "kafka")
            {
                if (!_queueMode.UseKafka || !_kafkaSettings.IsValid())
                {
                    return BadRequest(new { error = "Kafka is not enabled or configured" });
                }

                return await GetKafkaTopicDetails(topicName);
            }

            return BadRequest(new { error = "Invalid source. Use 'inmemory' or 'kafka'" });
        }

        // No source specified - get from configured sources
        var responses = new List<object>();

        if (_queueMode.UseInMemory)
        {
            var topic = _topicManager.GetTopic(topicName);
            if (topic != null)
            {
                responses.Add(new
                {
                    Source = "In-Memory",
                    topic.Name,
                    topic.CreatedAt,
                    MessageCount = topic.GetMessageCount(),
                    HasMessages = topic.HasMessages()
                });
            }
        }

        if (_queueMode.UseKafka && _kafkaSettings.IsValid())
        {
            var kafkaResult = await GetKafkaTopicDetails(topicName);
            if (kafkaResult is OkObjectResult okResult)
            {
                responses.Add(okResult.Value!);
            }
        }

        if (responses.Count == 0)
        {
            return NotFound(new { error = $"Topic '{topicName}' not found in any configured source" });
        }

        if (_queueMode.EnableHybridMode && responses.Count > 1)
        {
            return Ok(new
            {
                Mode = "Hybrid",
                TopicName = topicName,
                Sources = responses
            });
        }

        return Ok(responses.First());
    }

    /// <summary>
    /// Delete topic from configured sources (in-memory, Kafka, or both)
    /// </summary>
    [HttpDelete("{topicName}")]
    public async Task<IActionResult> DeleteTopic(string topicName)
    {
        var results = new List<string>();
        var foundInAnySource = false;

        // Delete from in-memory
        if (_queueMode.UseInMemory)
        {
            if (_topicManager.DeleteTopic(topicName))
            {
                results.Add($"✅ Deleted from in-memory: {topicName}");
                _logger.LogInformation("In-memory topic '{TopicName}' deleted", topicName);
                foundInAnySource = true;
            }
            else
            {
                results.Add($"⚠️ In-memory topic not found: {topicName}");
            }
        }

        // Delete from Kafka
        if (_queueMode.UseKafka && _kafkaSettings.IsValid())
        {
            try
            {
                var adminConfig = _kafkaSettings.GetAdminClientConfig();

                using var adminClient = new AdminClientBuilder(adminConfig).Build();
                await adminClient.DeleteTopicsAsync(new[] { topicName });

                results.Add($"✅ Deleted from Kafka: {topicName}");
                _logger.LogInformation("Kafka topic '{TopicName}' deleted", topicName);
                foundInAnySource = true;
            }
            catch (DeleteTopicsException ex)
            {
                foreach (var topicResult in ex.Results)
                {
                    results.Add($"❌ Kafka delete error: {topicResult.Error.Reason}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"❌ Kafka connection error: {ex.Message}");
            }
        }

        if (!foundInAnySource)
        {
            return NotFound(new { topicName, mode = _queueMode.GetMode(), results });
        }

        return Ok(new { topicName, mode = _queueMode.GetMode(), results });
    }

    private async Task<IActionResult> GetKafkaTopicDetails(string topicName)
    {
        try
        {
            var adminConfig = _kafkaSettings.GetAdminClientConfig();

            using var adminClient = new AdminClientBuilder(adminConfig).Build();
            var metadata = adminClient.GetMetadata(topicName, TimeSpan.FromSeconds(10));

            var topicMetadata = metadata.Topics.FirstOrDefault(t => t.Topic == topicName);
            if (topicMetadata == null)
            {
                return NotFound(new { error = $"Kafka topic '{topicName}' not found" });
            }

            return Ok(new
            {
                Source = "Kafka",
                Name = topicMetadata.Topic,
                Partitions = topicMetadata.Partitions.Select(p => new
                {
                    PartitionId = p.PartitionId,
                    Leader = p.Leader,
                    Replicas = p.Replicas.Length,
                    InSyncReplicas = p.InSyncReplicas.Length
                }),
                PartitionCount = topicMetadata.Partitions.Count
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
