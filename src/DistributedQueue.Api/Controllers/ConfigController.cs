using DistributedQueue.Api.Configuration;
using DistributedQueue.Kafka.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DistributedQueue.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly QueueModeSettings _queueMode;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<ConfigController> _logger;

    public ConfigController(
        IOptions<QueueModeSettings> queueMode,
        IOptions<KafkaSettings> kafkaSettings,
        ILogger<ConfigController> logger)
    {
        _queueMode = queueMode.Value;
        _kafkaSettings = kafkaSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Get current queue configuration and status
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            Mode = _queueMode.GetMode(),
            Configuration = new
            {
                InMemory = new
                {
                    Enabled = _queueMode.UseInMemory,
                    Description = "Fast, ephemeral, in-process queue"
                },
                Kafka = new
                {
                    Enabled = _queueMode.UseKafka,
                    Configured = _kafkaSettings.IsValid(),
                    BootstrapServers = _kafkaSettings.BootstrapServers,
                    GroupId = _kafkaSettings.GroupId,
                    Description = "Persistent, distributed, Confluent Cloud"
                },
                Hybrid = new
                {
                    Enabled = _queueMode.EnableHybridMode,
                    Description = "Messages sent to BOTH in-memory and Kafka simultaneously"
                }
            },
            Recommendations = GetRecommendations()
        });
    }

    /// <summary>
    /// Get configuration recommendations
    /// </summary>
    [HttpGet("recommendations")]
    public IActionResult GetRecommendations()
    {
        var recommendations = new List<string>();

        if (!_queueMode.UseInMemory && !_queueMode.UseKafka)
        {
            recommendations.Add("⚠️ Both in-memory and Kafka are disabled! Enable at least one.");
        }

        if (_queueMode.EnableHybridMode && (!_queueMode.UseInMemory || !_queueMode.UseKafka))
        {
            recommendations.Add("⚠️ Hybrid mode requires BOTH UseInMemory=true AND UseKafka=true");
        }

        if (_queueMode.UseKafka && !_kafkaSettings.IsValid())
        {
            recommendations.Add("⚠️ Kafka is enabled but not configured. Update appsettings.json with valid credentials.");
        }

        if (_queueMode.UseInMemory && !_queueMode.UseKafka)
        {
            recommendations.Add("✅ In-Memory mode: Perfect for development and testing");
            recommendations.Add("💡 Enable Kafka for persistence and distribution");
        }

        if (_queueMode.UseKafka && !_queueMode.UseInMemory)
        {
            recommendations.Add("✅ Kafka-only mode: Production-ready, persistent");
            recommendations.Add("💡 Enable in-memory for faster local testing");
        }

        if (_queueMode.EnableHybridMode && _queueMode.UseInMemory && _queueMode.UseKafka && _kafkaSettings.IsValid())
        {
            recommendations.Add("✅ Hybrid mode: Messages stored in BOTH systems");
            recommendations.Add("💡 Great for migration or redundancy scenarios");
        }

        if (recommendations.Count == 0)
        {
            recommendations.Add("✅ Configuration looks good!");
        }

        return Ok(recommendations);
    }

    /// <summary>
    /// Get configuration examples
    /// </summary>
    [HttpGet("examples")]
    public IActionResult GetExamples()
    {
        var examples = new List<object>
        {
            new
            {
                Name = "Development Mode (In-Memory Only)",
                Description = "Fast, no Kafka required",
                Config = new
                {
                    QueueMode = new { UseInMemory = true, UseKafka = false, EnableHybridMode = false }
                }
            },
            new
            {
                Name = "Production Mode (Kafka Only)",
                Description = "Persistent, distributed",
                Config = new
                {
                    QueueMode = new { UseInMemory = false, UseKafka = true, EnableHybridMode = false },
                    KafkaSettings = new { Enabled = true }
                }
            },
            new
            {
                Name = "Hybrid Mode (Both)",
                Description = "Messages sent to both systems",
                Config = new
                {
                    QueueMode = new { UseInMemory = true, UseKafka = true, EnableHybridMode = true },
                    KafkaSettings = new { Enabled = true }
                }
            },
            new
            {
                Name = "Testing Mode (In-Memory + Kafka Ready)",
                Description = "Use in-memory, but Kafka configured for easy switch",
                Config = new
                {
                    QueueMode = new { UseInMemory = true, UseKafka = false, EnableHybridMode = false },
                    KafkaSettings = new { Enabled = true }
                }
            }
        };

        return Ok(new { Examples = examples });
    }
}
