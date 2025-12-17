using DistributedQueue.Core.Services;
using DistributedQueue.Kafka.Configuration;
using DistributedQueue.Kafka.Producers;
using DistributedQueue.Api.Configuration;
using DistributedQueue.Api.Services;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Distributed Queue API (Hybrid Mode)",
        Version = "v1",
        Description = "A hybrid distributed queue system supporting both in-memory and Confluent Cloud Kafka"
    });
});

// Add CORS for future web UI
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Read configuration once
var kafkaSettings = builder.Configuration.GetSection("KafkaSettings").Get<KafkaSettings>() ?? new KafkaSettings();
var queueMode = builder.Configuration.GetSection("QueueMode").Get<QueueModeSettings>() ?? new QueueModeSettings();

// Configure Queue Mode and Kafka Settings for DI
builder.Services.Configure<QueueModeSettings>(builder.Configuration.GetSection("QueueMode"));
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("KafkaSettings"));

// Register in-memory services (always available)
builder.Services.AddSingleton<ITopicManager, TopicManager>();
builder.Services.AddSingleton<IProducerManager, ProducerManager>();
builder.Services.AddSingleton<IConsumerGroupManager, ConsumerGroupManager>();
builder.Services.AddSingleton<IConsumerManager, ConsumerManager>();
builder.Services.AddSingleton<IMessageBroker, MessageBroker>();

// Register Kafka services conditionally

if (kafkaSettings?.Enabled == true && queueMode?.UseKafka == true)
{
    if (kafkaSettings.IsValid())
    {
        Console.WriteLine("☁️  Kafka is ENABLED and configured");
        Console.WriteLine($"   Bootstrap: {kafkaSettings.BootstrapServers}");
        Console.WriteLine($"   Username: {kafkaSettings.SaslUsername}");
        
        builder.Services.AddSingleton<IKafkaProducerService>(sp =>
            new KafkaProducerService(kafkaSettings.GetProducerConfig()));
    }
    else
    {
        Console.WriteLine("⚠️  Kafka is enabled but NOT configured properly. Using in-memory only.");
        Console.WriteLine("   Please update appsettings.json with valid Kafka credentials.");
    }
}
else
{
    Console.WriteLine("📦 Kafka is DISABLED. Using in-memory only.");
}

// Register Hybrid Message Broker
builder.Services.AddSingleton<IHybridMessageBroker, HybridMessageBroker>();

var app = builder.Build();

// Log the current mode (using already-read configuration)
Console.WriteLine($"🚀 Queue System started in mode: {queueMode.GetMode()}");

// Configure the HTTP request pipeline
// Always enable Swagger for easy API access
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Distributed Queue API v1");
    c.RoutePrefix = "swagger";
});

// Don't redirect to HTTPS in container (using HTTP on port 8080)
// app.UseHttpsRedirection();

app.UseCors();
app.UseAuthorization();
app.MapControllers();

// Add a simple root endpoint
app.MapGet("/", () => Results.Redirect("/swagger"));

// Add a health/status endpoint
app.MapGet("/api/status", () =>
{
    return Results.Ok(new
    {
        Mode = queueMode.GetMode(),
        InMemoryEnabled = queueMode.UseInMemory,
        KafkaEnabled = queueMode.UseKafka,
        HybridEnabled = queueMode.EnableHybridMode,
        KafkaConfigured = kafkaSettings.IsValid(),
        KafkaBootstrap = kafkaSettings.BootstrapServers
    });
});

app.Run();
