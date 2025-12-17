using Confluent.Kafka;

namespace DistributedQueue.Kafka.Configuration;

/// <summary>
/// Configuration settings for Confluent Cloud Kafka
/// </summary>
public class KafkaSettings
{
    public bool Enabled { get; set; } = false;
    public string BootstrapServers { get; set; } = string.Empty;
    public string SaslUsername { get; set; } = string.Empty;
    public string SaslPassword { get; set; } = string.Empty;
    public string SecurityProtocol { get; set; } = "SASL_SSL";
    public string SaslMechanism { get; set; } = "PLAIN";
    public string GroupId { get; set; } = "distributed-queue-consumer-group";

    /// <summary>
    /// Validates that all required settings are configured
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(BootstrapServers) &&
               !string.IsNullOrEmpty(SaslUsername) &&
               !string.IsNullOrEmpty(SaslPassword) &&
               !BootstrapServers.Contains("your-cluster") &&
               !SaslUsername.Contains("YOUR_API_KEY");
    }

    public ProducerConfig GetProducerConfig()
    {
        var config = new ProducerConfig
        {
            BootstrapServers = BootstrapServers,
            SecurityProtocol = Enum.Parse<SecurityProtocol>(SecurityProtocol),
            SaslMechanism = Enum.Parse<SaslMechanism>(SaslMechanism),
            SaslUsername = SaslUsername,
            SaslPassword = SaslPassword,
            
            // Connection settings for Confluent Cloud
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageTimeoutMs = 30000,
            RequestTimeoutMs = 30000,
            
            // Retry settings
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 1000,
            
            // Debugging
            Debug = "broker,topic,msg"
        };
        
        Console.WriteLine("🔧 ProducerConfig created:");
        Console.WriteLine($"   BootstrapServers: {config.BootstrapServers}");
        Console.WriteLine($"   SecurityProtocol: {config.SecurityProtocol}");
        Console.WriteLine($"   SaslMechanism: {config.SaslMechanism}");
        Console.WriteLine($"   Acks: {config.Acks}");
        Console.WriteLine($"   Timeout: {config.MessageTimeoutMs}ms");
        
        return config;
    }

    public ConsumerConfig GetConsumerConfig()
    {
        return new ConsumerConfig
        {
            BootstrapServers = BootstrapServers,
            SecurityProtocol = Enum.Parse<SecurityProtocol>(SecurityProtocol),
            SaslMechanism = Enum.Parse<SaslMechanism>(SaslMechanism),
            SaslUsername = SaslUsername,
            SaslPassword = SaslPassword,
            GroupId = GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
    }
}
