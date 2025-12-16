using DistributedQueue.Core.Models;
using DistributedQueue.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace DistributedQueue.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemoController : ControllerBase
{
    private readonly ITopicManager _topicManager;
    private readonly IProducerManager _producerManager;
    private readonly IConsumerManager _consumerManager;
    private readonly IMessageBroker _messageBroker;
    private readonly ILogger<DemoController> _logger;

    public DemoController(
        ITopicManager topicManager,
        IProducerManager producerManager,
        IConsumerManager consumerManager,
        IMessageBroker messageBroker,
        ILogger<DemoController> logger)
    {
        _topicManager = topicManager;
        _producerManager = producerManager;
        _consumerManager = consumerManager;
        _messageBroker = messageBroker;
        _logger = logger;
    }

    [HttpPost("run-scenario")]
    public async Task<IActionResult> RunScenario()
    {
        try
        {
            var results = new List<string>();

            // Create 2 topics: topic1 and topic2
            _topicManager.CreateTopic("topic1");
            _topicManager.CreateTopic("topic2");
            results.Add("✓ Created topics: topic1, topic2");

            // Create 2 producers: producer1 and producer2
            _producerManager.CreateProducer("producer1", "Producer 1");
            _producerManager.CreateProducer("producer2", "Producer 2");
            results.Add("✓ Created producers: producer1, producer2");

            // Create 5 consumers
            _consumerManager.CreateConsumer("consumer1", "Consumer 1");
            _consumerManager.CreateConsumer("consumer2", "Consumer 2");
            _consumerManager.CreateConsumer("consumer3", "Consumer 3");
            _consumerManager.CreateConsumer("consumer4", "Consumer 4");
            _consumerManager.CreateConsumer("consumer5", "Consumer 5");
            results.Add("✓ Created consumers: consumer1, consumer2, consumer3, consumer4, consumer5");

            // Make all 5 consumers subscribe to topic1
            for (int i = 1; i <= 5; i++)
            {
                _consumerManager.SubscribeConsumerToTopic($"consumer{i}", "topic1");
            }
            results.Add("✓ All 5 consumers subscribed to topic1");

            // Make consumers 1, 3, and 4 subscribe to topic2
            _consumerManager.SubscribeConsumerToTopic("consumer1", "topic2");
            _consumerManager.SubscribeConsumerToTopic("consumer3", "topic2");
            _consumerManager.SubscribeConsumerToTopic("consumer4", "topic2");
            results.Add("✓ Consumers 1, 3, and 4 subscribed to topic2");

            // Start all consumers
            for (int i = 1; i <= 5; i++)
            {
                _messageBroker.StartConsumer($"consumer{i}");
            }
            results.Add("✓ Started all consumers");

            // Allow consumers to start processing
            await Task.Delay(500);

            // Make producer1 publish message "Message 1" to topic1
            var msg1 = new Message("Message 1", "topic1", "producer1");
            _messageBroker.PublishMessage("topic1", msg1);
            results.Add("✓ Producer1 published 'Message 1' to topic1");

            await Task.Delay(200);

            // Make producer1 publish message "Message 2" to topic1
            var msg2 = new Message("Message 2", "topic1", "producer1");
            _messageBroker.PublishMessage("topic1", msg2);
            results.Add("✓ Producer1 published 'Message 2' to topic1");

            await Task.Delay(200);

            // Make producer2 publish message "Message 3" to topic1
            var msg3 = new Message("Message 3", "topic1", "producer2");
            _messageBroker.PublishMessage("topic1", msg3);
            results.Add("✓ Producer2 published 'Message 3' to topic1");

            await Task.Delay(200);

            // Make producer1 publish message "Message 4" to topic2
            var msg4 = new Message("Message 4", "topic2", "producer1");
            _messageBroker.PublishMessage("topic2", msg4);
            results.Add("✓ Producer1 published 'Message 4' to topic2");

            await Task.Delay(200);

            // Make producer2 publish message "Message 5" to topic2
            var msg5 = new Message("Message 5", "topic2", "producer2");
            _messageBroker.PublishMessage("topic2", msg5);
            results.Add("✓ Producer2 published 'Message 5' to topic2");

            await Task.Delay(1000); // Wait for messages to be consumed

            results.Add("✓ Demo scenario completed successfully!");
            results.Add("Note: Check the console output for message consumption logs");

            return Ok(new { success = true, steps = results });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running demo scenario");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("cleanup")]
    public IActionResult Cleanup()
    {
        try
        {
            _messageBroker.StopAllConsumers();

            var consumers = _consumerManager.GetAllConsumers().ToList();
            foreach (var consumer in consumers)
            {
                _consumerManager.DeleteConsumer(consumer.Id);
            }

            var producers = _producerManager.GetAllProducers().ToList();
            foreach (var producer in producers)
            {
                _producerManager.DeleteProducer(producer.Id);
            }

            var topics = _topicManager.GetAllTopics().ToList();
            foreach (var topic in topics)
            {
                _topicManager.DeleteTopic(topic.Name);
            }

            return Ok(new { message = "All resources cleaned up successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cleanup");
            return BadRequest(new { error = ex.Message });
        }
    }
}
