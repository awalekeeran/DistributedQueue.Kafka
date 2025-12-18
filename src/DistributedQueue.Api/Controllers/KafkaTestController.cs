using Confluent.Kafka;
using Confluent.Kafka.Admin;
using DistributedQueue.Kafka.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DistributedQueue.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KafkaTestController : ControllerBase
{
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<KafkaTestController> _logger;

    public KafkaTestController(
        IOptions<KafkaSettings> kafkaSettings,
        ILogger<KafkaTestController> logger)
    {
        _kafkaSettings = kafkaSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Test connection to Confluent Cloud Kafka
    /// </summary>
    [HttpPost("test-connection")]
    public async Task<IActionResult> TestConnection()
    {
        var results = new List<string>();
        
        results.Add("🔍 Testing Kafka Connection...");
        results.Add("");
        
        // Check configuration
        results.Add("📋 Configuration:");
        results.Add($"   Enabled: {_kafkaSettings.Enabled}");
        results.Add($"   Bootstrap Servers: {_kafkaSettings.BootstrapServers}");
        results.Add($"   SASL Username: {_kafkaSettings.SaslUsername}");
        results.Add($"   SASL Password: {new string('*', _kafkaSettings.SaslPassword.Length)} ({_kafkaSettings.SaslPassword.Length} chars)");
        results.Add($"   Security Protocol: {_kafkaSettings.SecurityProtocol}");
        results.Add($"   SASL Mechanism: {_kafkaSettings.SaslMechanism}");
        results.Add($"   Group ID: {_kafkaSettings.GroupId}");
        results.Add($"   Is Valid: {_kafkaSettings.IsValid()}");
        results.Add("");

        if (!_kafkaSettings.IsValid())
        {
            results.Add("❌ Configuration is INVALID!");
            results.Add("   Please update appsettings.Development.json with valid Kafka credentials.");
            return BadRequest(new { results });
        }

        // Test Admin Client (metadata query)
        results.Add("🔌 Testing Admin Connection (Metadata Query)...");
        try
        {
            var adminConfig = _kafkaSettings.GetAdminClientConfig();

            using var adminClient = new AdminClientBuilder(adminConfig).Build();
            
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(30));
            
            results.Add($"✅ Connected successfully!");
            results.Add($"   Cluster ID: {metadata.OriginatingBrokerId}");
            results.Add($"   Brokers: {metadata.Brokers.Count}");
            results.Add("");
            
            results.Add("📊 Broker Information:");
            foreach (var broker in metadata.Brokers)
            {
                results.Add($"   - Broker {broker.BrokerId}: {broker.Host}:{broker.Port}");
            }
            results.Add("");
            
            results.Add($"📑 Topics Found: {metadata.Topics.Count}");
            foreach (var topic in metadata.Topics.Take(10))
            {
                results.Add($"   - {topic.Topic} ({topic.Partitions.Count} partitions)");
            }
            
            if (metadata.Topics.Count > 10)
            {
                results.Add($"   ... and {metadata.Topics.Count - 10} more topics");
            }
        }
        catch (Exception ex)
        {
            results.Add($"❌ Connection FAILED!");
            results.Add($"   Error Type: {ex.GetType().Name}");
            results.Add($"   Error Message: {ex.Message}");
            
            if (ex.InnerException != null)
            {
                results.Add($"   Inner Exception: {ex.InnerException.Message}");
            }
            
            results.Add("");
            results.Add("💡 Common Issues:");
            results.Add("   1. Check Bootstrap Servers URL (should be pkc-xxxxx.region.provider.confluent.cloud:9092)");
            results.Add("   2. Verify API Key (SaslUsername) is correct");
            results.Add("   3. Verify API Secret (SaslPassword) is correct");
            results.Add("   4. Check network/firewall (port 9092 must be accessible)");
            results.Add("   5. Ensure cluster is active in Confluent Cloud");
            
            return BadRequest(new { results, error = ex.Message });
        }

        // Test Producer
        results.Add("");
        results.Add("📤 Testing Producer...");
        try
        {
            var producerConfig = _kafkaSettings.GetProducerConfig();
            
            using var producer = new ProducerBuilder<string, string>(producerConfig).Build();
            
            results.Add("✅ Producer created successfully!");
            results.Add($"   Name: {producer.Name}");
        }
        catch (Exception ex)
        {
            results.Add($"❌ Producer creation FAILED: {ex.Message}");
        }

        results.Add("");
        results.Add("🎉 Connection test complete!");
        
        return Ok(new { success = true, results });
    }

    /// <summary>
    /// Create a test topic in Confluent Cloud
    /// </summary>
    [HttpPost("create-test-topic")]
    public async Task<IActionResult> CreateTestTopic([FromQuery] string topicName = "test-topic-dotnet")
    {
        var results = new List<string>();
        
        try
        {
            var adminConfig = _kafkaSettings.GetAdminClientConfig();

            using var adminClient = new AdminClientBuilder(adminConfig).Build();
            
            results.Add($"📝 Creating topic '{topicName}'...");
            
            await adminClient.CreateTopicsAsync(new[]
            {
                new TopicSpecification
                {
                    Name = topicName,
                    NumPartitions = 3,
                    ReplicationFactor = 3
                }
            });
            
            results.Add($"✅ Topic '{topicName}' created successfully!");
            results.Add("   Partitions: 3");
            results.Add("   Replication Factor: 3");
            
            return Ok(new { success = true, topicName, results });
        }
        catch (CreateTopicsException ex)
        {
            results.Add($"❌ Failed to create topic:");
            foreach (var result in ex.Results)
            {
                results.Add($"   {result.Topic}: {result.Error.Reason}");
                
                if (result.Error.Code == ErrorCode.TopicAlreadyExists)
                {
                    results.Add($"   💡 Topic already exists - this is OK!");
                }
            }
            
            return BadRequest(new { success = false, results, error = ex.Message });
        }
        catch (Exception ex)
        {
            results.Add($"❌ Error: {ex.Message}");
            return BadRequest(new { success = false, results, error = ex.Message });
        }
    }

    /// <summary>
    /// Send a test message to Kafka
    /// </summary>
    [HttpPost("send-test-message")]
    public async Task<IActionResult> SendTestMessage(
        [FromQuery] string topicName = "test-topic-dotnet",
        [FromQuery] string message = "Hello from .NET!")
    {
        var results = new List<string>();
        
        try
        {
            results.Add($"📤 Sending test message to topic '{topicName}'...");
            results.Add($"   Message: \"{message}\"");
            results.Add("");
            
            var producerConfig = _kafkaSettings.GetProducerConfig();
            
            using var producer = new ProducerBuilder<string, string>(producerConfig)
                .SetErrorHandler((_, e) =>
                {
                    results.Add($"⚠️ Producer Error: {e.Reason}");
                })
                .Build();

            var kafkaMessage = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = message,
                Timestamp = new Timestamp(DateTime.UtcNow)
            };

            var deliveryResult = await producer.ProduceAsync(topicName, kafkaMessage);
            
            results.Add($"✅ Message delivered successfully!");
            results.Add($"   Topic: {deliveryResult.Topic}");
            results.Add($"   Partition: {deliveryResult.Partition}");
            results.Add($"   Offset: {deliveryResult.Offset}");
            results.Add($"   Timestamp: {deliveryResult.Timestamp.UtcDateTime}");
            results.Add("");
            results.Add("🎉 Check Confluent Cloud console to see your message!");
            
            return Ok(new { success = true, results, deliveryResult = new {
                topic = deliveryResult.Topic,
                partition = deliveryResult.Partition.Value,
                offset = deliveryResult.Offset.Value
            }});
        }
        catch (ProduceException<string, string> ex)
        {
            results.Add($"❌ Produce failed!");
            results.Add($"   Error Code: {ex.Error.Code}");
            results.Add($"   Error Reason: {ex.Error.Reason}");
            
            return BadRequest(new { success = false, results, error = ex.Error.Reason });
        }
        catch (Exception ex)
        {
            results.Add($"❌ Error: {ex.Message}");
            return BadRequest(new { success = false, results, error = ex.Message });
        }
    }
}
