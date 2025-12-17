using DistributedQueue.Api.Configuration;
using DistributedQueue.Api.Services;
using DistributedQueue.Core.Models;
using DistributedQueue.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DistributedQueue.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemoController : ControllerBase
{
    private readonly ITopicManager _topicManager;
    private readonly IProducerManager _producerManager;
    private readonly IConsumerManager _consumerManager;
    private readonly IMessageBroker _messageBroker;
    private readonly IHybridMessageBroker _hybridBroker;
    private readonly QueueModeSettings _queueMode;
    private readonly ILogger<DemoController> _logger;

    public DemoController(
        ITopicManager topicManager,
        IProducerManager producerManager,
        IConsumerManager consumerManager,
        IMessageBroker messageBroker,
        IHybridMessageBroker hybridBroker,
        IOptions<QueueModeSettings> queueMode,
        ILogger<DemoController> logger)
    {
        _topicManager = topicManager;
        _producerManager = producerManager;
        _consumerManager = consumerManager;
        _messageBroker = messageBroker;
        _hybridBroker = hybridBroker;
        _queueMode = queueMode.Value;
        _logger = logger;
    }

    /// <summary>
    /// Run a demo scenario showing producer-consumer interaction
    /// Works with configured mode (In-Memory, Kafka, or Hybrid)
    /// </summary>
    [HttpPost("run-scenario")]
    public async Task<IActionResult> RunScenario()
    {
        try
        {
            var results = new List<string>();
            results.Add($"🚀 Running demo in {_queueMode.GetMode()} mode...");
            results.Add("");

            // Create 2 topics: topic1 and topic2
            _topicManager.CreateTopic("demo-topic1");
            _topicManager.CreateTopic("demo-topic2");
            results.Add("✓ Created topics: demo-topic1, demo-topic2");

            // Create 2 producers: producer1 and producer2
            _producerManager.CreateProducer("demo-producer1", "Demo Producer 1");
            _producerManager.CreateProducer("demo-producer2", "Demo Producer 2");
            results.Add("✓ Created producers: demo-producer1, demo-producer2");

            // Only create consumers in in-memory mode (Kafka consumers are managed differently)
            if (_queueMode.UseInMemory)
            {
                // Create 5 consumers
                _consumerManager.CreateConsumer("demo-consumer1", "Demo Consumer 1");
                _consumerManager.CreateConsumer("demo-consumer2", "Demo Consumer 2");
                _consumerManager.CreateConsumer("demo-consumer3", "Demo Consumer 3");
                _consumerManager.CreateConsumer("demo-consumer4", "Demo Consumer 4");
                _consumerManager.CreateConsumer("demo-consumer5", "Demo Consumer 5");
                results.Add("✓ Created consumers: demo-consumer1 through demo-consumer5 (in-memory)");

                // Make all 5 consumers subscribe to topic1
                for (int i = 1; i <= 5; i++)
                {
                    _consumerManager.SubscribeConsumerToTopic($"demo-consumer{i}", "demo-topic1");
                }
                results.Add("✓ All 5 consumers subscribed to demo-topic1");

                // Make consumers 1, 3, and 4 subscribe to topic2
                _consumerManager.SubscribeConsumerToTopic("demo-consumer1", "demo-topic2");
                _consumerManager.SubscribeConsumerToTopic("demo-consumer3", "demo-topic2");
                _consumerManager.SubscribeConsumerToTopic("demo-consumer4", "demo-topic2");
                results.Add("✓ Consumers 1, 3, and 4 subscribed to demo-topic2");

                // Start all consumers
                for (int i = 1; i <= 5; i++)
                {
                    _messageBroker.StartConsumer($"demo-consumer{i}");
                }
                results.Add("✓ Started all consumers");

                // Allow consumers to start processing
                await Task.Delay(500);
            }

            // Publish messages using HybridMessageBroker (works with all modes)
            await _hybridBroker.PublishMessageAsync("demo-producer1", "demo-topic1", "Demo Message 1");
            results.Add("✓ demo-producer1 published 'Demo Message 1' to demo-topic1");

            await Task.Delay(200);

            await _hybridBroker.PublishMessageAsync("demo-producer1", "demo-topic1", "Demo Message 2");
            results.Add("✓ demo-producer1 published 'Demo Message 2' to demo-topic1");

            await Task.Delay(200);

            await _hybridBroker.PublishMessageAsync("demo-producer2", "demo-topic1", "Demo Message 3");
            results.Add("✓ demo-producer2 published 'Demo Message 3' to demo-topic1");

            await Task.Delay(200);

            await _hybridBroker.PublishMessageAsync("demo-producer1", "demo-topic2", "Demo Message 4");
            results.Add("✓ demo-producer1 published 'Demo Message 4' to demo-topic2");

            await Task.Delay(200);

            await _hybridBroker.PublishMessageAsync("demo-producer2", "demo-topic2", "Demo Message 5");
            results.Add("✓ demo-producer2 published 'Demo Message 5' to demo-topic2");

            await Task.Delay(1000); // Wait for messages to be consumed

            results.Add("");
            results.Add("✅ Demo scenario completed successfully!");
            
            if (_queueMode.UseInMemory)
            {
                results.Add("📝 Check the console output for in-memory message consumption logs");
            }
            
            if (_queueMode.UseKafka)
            {
                results.Add("☁️ Messages published to Kafka topics. Use Confluent Cloud UI or Kafka consumers to view.");
            }

            if (_queueMode.EnableHybridMode)
            {
                results.Add("🔄 Messages published to BOTH in-memory and Kafka!");
            }

            return Ok(new 
            { 
                success = true, 
                mode = _queueMode.GetMode(),
                steps = results 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running demo scenario");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Cleanup demo resources (in-memory only)
    /// </summary>
    [HttpPost("cleanup")]
    public IActionResult Cleanup()
    {
        try
        {
            var results = new List<string>();
            
            if (_queueMode.UseInMemory)
            {
                _messageBroker.StopAllConsumers();
                results.Add("✓ Stopped all consumers");

                var consumers = _consumerManager.GetAllConsumers()
                    .Where(c => c.Id.StartsWith("demo-"))
                    .ToList();
                foreach (var consumer in consumers)
                {
                    _consumerManager.DeleteConsumer(consumer.Id);
                }
                results.Add($"✓ Deleted {consumers.Count} demo consumers");
            }

            var producers = _producerManager.GetAllProducers()
                .Where(p => p.Id.StartsWith("demo-"))
                .ToList();
            foreach (var producer in producers)
            {
                _producerManager.DeleteProducer(producer.Id);
            }
            results.Add($"✓ Deleted {producers.Count} demo producers");

            var topics = _topicManager.GetAllTopics()
                .Where(t => t.Name.StartsWith("demo-"))
                .ToList();
            foreach (var topic in topics)
            {
                _topicManager.DeleteTopic(topic.Name);
            }
            results.Add($"✓ Deleted {topics.Count} demo topics (in-memory)");

            results.Add("");
            results.Add("✅ Demo resources cleaned up successfully!");
            
            if (_queueMode.UseKafka)
            {
                results.Add("⚠️ Note: Kafka topics must be deleted manually via Confluent Cloud UI or Kafka admin tools");
            }

            return Ok(new 
            { 
                success = true,
                mode = _queueMode.GetMode(),
                results = results 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cleanup");
            return BadRequest(new { error = ex.Message });
        }
    }
}
