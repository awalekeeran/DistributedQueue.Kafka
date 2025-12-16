using DistributedQueue.Core.Models;
using System.Collections.Concurrent;

namespace DistributedQueue.Core.Services;

public interface IMessageBroker
{
    void PublishMessage(string topicName, Message message);
    void StartConsumer(string consumerId);
    void StopConsumer(string consumerId);
    void StopAllConsumers();
}

public class MessageBroker : IMessageBroker
{
    private readonly ITopicManager _topicManager;
    private readonly IConsumerManager _consumerManager;
    private readonly IConsumerGroupManager _consumerGroupManager;
    private readonly ConcurrentDictionary<string, Task> _consumerTasks;
    private readonly object _lock = new object();

    public MessageBroker(
        ITopicManager topicManager, 
        IConsumerManager consumerManager,
        IConsumerGroupManager consumerGroupManager)
    {
        _topicManager = topicManager;
        _consumerManager = consumerManager;
        _consumerGroupManager = consumerGroupManager;
        _consumerTasks = new ConcurrentDictionary<string, Task>();
    }

    public void PublishMessage(string topicName, Message message)
    {
        var topic = _topicManager.GetTopic(topicName);
        if (topic == null)
        {
            throw new InvalidOperationException($"Topic '{topicName}' does not exist");
        }

        topic.AddMessage(message);
    }

    public void StartConsumer(string consumerId)
    {
        var consumer = _consumerManager.GetConsumer(consumerId);
        if (consumer == null)
        {
            throw new InvalidOperationException($"Consumer '{consumerId}' does not exist");
        }

        if (consumer.IsActive)
        {
            return; // Already running
        }

        consumer.IsActive = true;
        var cancellationToken = consumer.GetCancellationTokenSource().Token;

        var task = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested && consumer.IsActive)
            {
                try
                {
                    await ProcessMessagesForConsumer(consumer, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in consumer {consumerId}: {ex.Message}");
                }

                await Task.Delay(100, cancellationToken); // Poll interval
            }
        }, cancellationToken);

        _consumerTasks[consumerId] = task;
    }

    public void StopConsumer(string consumerId)
    {
        var consumer = _consumerManager.GetConsumer(consumerId);
        if (consumer != null)
        {
            consumer.Stop();
        }

        if (_consumerTasks.TryRemove(consumerId, out var task))
        {
            try
            {
                task.Wait(TimeSpan.FromSeconds(5));
            }
            catch (AggregateException)
            {
                // Task was cancelled, which is expected
            }
        }
    }

    public void StopAllConsumers()
    {
        var allConsumers = _consumerManager.GetAllConsumers();
        foreach (var consumer in allConsumers)
        {
            StopConsumer(consumer.Id);
        }
    }

    private async Task ProcessMessagesForConsumer(Consumer consumer, CancellationToken cancellationToken)
    {
        foreach (var topicName in consumer.GetSubscribedTopics())
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var topic = _topicManager.GetTopic(topicName);
            if (topic == null || !topic.HasMessages())
                continue;

            // Check if consumer is part of a consumer group
            if (!string.IsNullOrEmpty(consumer.ConsumerGroup))
            {
                var consumerGroup = _consumerGroupManager.GetConsumerGroup(consumer.ConsumerGroup);
                if (consumerGroup != null)
                {
                    var selectedConsumer = consumerGroup.GetNextConsumer(topicName);
                    if (selectedConsumer?.Id != consumer.Id)
                    {
                        continue; // This message should be consumed by another consumer in the group
                    }
                }
            }

            var message = topic.TryDequeueMessage();
            if (message != null)
            {
                consumer.OnMessageReceived(message);
            }

            await Task.Yield();
        }
    }
}
