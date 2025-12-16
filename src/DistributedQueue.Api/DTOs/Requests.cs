namespace DistributedQueue.Api.DTOs;

public class CreateTopicRequest
{
    public string TopicName { get; set; } = string.Empty;
}

public class CreateProducerRequest
{
    public string ProducerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class CreateConsumerRequest
{
    public string ConsumerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ConsumerGroup { get; set; }
}

public class PublishMessageRequest
{
    public string ProducerId { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class SubscribeRequest
{
    public string ConsumerId { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
}

public class CreateConsumerGroupRequest
{
    public string GroupName { get; set; } = string.Empty;
}
