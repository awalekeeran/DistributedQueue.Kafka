namespace DistributedQueue.Api.Configuration;

/// <summary>
/// Configuration for queue operation mode
/// </summary>
public class QueueModeSettings
{
    /// <summary>
    /// Use in-memory queue (fast, no persistence)
    /// </summary>
    public bool UseInMemory { get; set; } = true;

    /// <summary>
    /// Use Confluent Cloud Kafka (persistent, distributed)
    /// </summary>
    public bool UseKafka { get; set; } = false;

    /// <summary>
    /// Enable hybrid mode: messages go to BOTH in-memory AND Kafka
    /// </summary>
    public bool EnableHybridMode { get; set; } = false;

    /// <summary>
    /// Gets the current operation mode as a string
    /// </summary>
    public string GetMode()
    {
        if (EnableHybridMode && UseInMemory && UseKafka)
            return "Hybrid (In-Memory + Kafka)";
        if (UseKafka)
            return "Kafka Only";
        if (UseInMemory)
            return "In-Memory Only";
        return "Disabled";
    }
}
