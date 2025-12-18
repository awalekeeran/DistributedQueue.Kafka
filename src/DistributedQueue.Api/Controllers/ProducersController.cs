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
public class ProducersController : ControllerBase
{
    private readonly IProducerManager _producerManager;
    private readonly QueueModeSettings _queueMode;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<ProducersController> _logger;

    public ProducersController(
        IProducerManager producerManager,
        IOptions<QueueModeSettings> queueMode,
        IOptions<KafkaSettings> kafkaSettings,
        ILogger<ProducersController> logger)
    {
        _producerManager = producerManager;
        _queueMode = queueMode.Value;
        _kafkaSettings = kafkaSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Create/Register a producer
    /// </summary>
    [HttpPost]
    public IActionResult CreateProducer([FromBody] CreateProducerRequest request)
    {
        try
        {
            var producer = _producerManager.CreateProducer(request.ProducerId, request.Name);
            _logger.LogInformation("Producer '{ProducerId}' created", request.ProducerId);
            return Ok(new 
            { 
                producer.Id, 
                producer.Name, 
                producer.CreatedAt,
                Mode = _queueMode.GetMode()
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all producers from configured sources (in-memory registered producers and active Kafka producers)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllProducers()
    {
        var inMemoryProducers = new List<object>();
        var kafkaProducers = new List<object>();

        // Get in-memory registered producers
        if (_queueMode.UseInMemory)
        {
            inMemoryProducers = _producerManager.GetAllProducers()
                .Select(p => new 
                { 
                    Id = p.Id, 
                    Name = p.Name, 
                    CreatedAt = p.CreatedAt, 
                    Source = "In-Memory" 
                })
                .Cast<object>()
                .ToList();

            _logger.LogInformation("Found {Count} in-memory registered producers", inMemoryProducers.Count);
        }

        // Get Kafka active producers (from topic metadata - shows which clients are producing)
        if (_queueMode.UseKafka && _kafkaSettings.IsValid())
        {
            try
            {
                var adminConfig = _kafkaSettings.GetAdminClientConfig();

                using var adminClient = new AdminClientBuilder(adminConfig).Build();
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));

                // Get brokers which act as producer endpoints
                kafkaProducers = metadata.Brokers
                    .Select(b => new
                    {
                        Id = $"kafka-broker-{b.BrokerId}",
                        Name = b.Host,
                        BrokerId = b.BrokerId,
                        Host = b.Host,
                        Port = b.Port,
                        Source = "Kafka",
                        Type = "Broker (Producer Endpoint)"
                    })
                    .Cast<object>()
                    .ToList();

                _logger.LogInformation("Found {Count} Kafka broker endpoints (producer targets)", kafkaProducers.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Kafka producer information");
            }
        }

        // Combine both sources if hybrid mode
        if (_queueMode.EnableHybridMode)
        {
            var combined = inMemoryProducers.Concat(kafkaProducers).ToList();
            return Ok(new
            {
                Mode = _queueMode.GetMode(),
                InMemoryProducers = inMemoryProducers,
                KafkaProducers = kafkaProducers,
                Combined = combined,
                Summary = new
                {
                    TotalProducers = combined.Count,
                    InMemoryCount = inMemoryProducers.Count,
                    KafkaCount = kafkaProducers.Count
                },
                Note = "In-memory shows registered producers. Kafka shows broker endpoints where producers connect."
            });
        }

        // Return only the active source
        if (_queueMode.UseKafka)
        {
            return Ok(new
            {
                Mode = _queueMode.GetMode(),
                Producers = kafkaProducers,
                Summary = new { TotalProducers = kafkaProducers.Count },
                Note = "Kafka producers shown as broker endpoints. Any client can produce to Kafka topics."
            });
        }

        return Ok(new
        {
            Mode = _queueMode.GetMode(),
            Producers = inMemoryProducers,
            Summary = new { TotalProducers = inMemoryProducers.Count }
        });
    }

    [HttpGet("{producerId}")]
    public IActionResult GetProducer(string producerId)
    {
        var producer = _producerManager.GetProducer(producerId);
        if (producer == null)
        {
            return NotFound(new { error = $"Producer '{producerId}' not found" });
        }

        return Ok(new { producer.Id, producer.Name, producer.CreatedAt });
    }

    [HttpDelete("{producerId}")]
    public IActionResult DeleteProducer(string producerId)
    {
        var deleted = _producerManager.DeleteProducer(producerId);
        if (!deleted)
        {
            return NotFound(new { error = $"Producer '{producerId}' not found" });
        }

        _logger.LogInformation("Producer '{ProducerId}' deleted", producerId);
        return Ok(new { message = $"Producer '{producerId}' deleted" });
    }
}
