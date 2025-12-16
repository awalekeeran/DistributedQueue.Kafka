using Confluent.Kafka;

namespace DistributedQueue.Kafka.Configuration;

/// <summary>
/// Configuration settings for Confluent Cloud Kafka
/// </summary>
public class KafkaSettings
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string SaslUsername { get; set; } = string.Empty;
    public string SaslPassword { get; set; } = string.Empty;
    public string SecurityProtocol { get; set; } = "SASL_SSL";
    public string SaslMechanism { get; set; } = "PLAIN";
    public string GroupId { get; set; } = "distributed-queue-consumer-group";

    public ProducerConfig GetProducerConfig()
    {
        return new ProducerConfig
        {
            BootstrapServers = BootstrapServers,
            SecurityProtocol = Enum.Parse<SecurityProtocol>(SecurityProtocol),
            SaslMechanism = Enum.Parse<SaslMechanism>(SaslMechanism),
            SaslUsername = SaslUsername,
            SaslPassword = SaslPassword
        };
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
