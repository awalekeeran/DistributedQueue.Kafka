# Extension Guide

This guide shows how to extend the Distributed Queue System with common scenarios.

## 🎯 Adding a New Feature

### Example: Add Message Priority

#### 1. Update the Message Model
**File**: `src/DistributedQueue.Core/Models/Message.cs`

```csharp
public class Message
{
    // Existing properties...
    public MessagePriority Priority { get; set; } = MessagePriority.Normal;
}

public enum MessagePriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}
```

#### 2. Update Topic to Use Priority Queue
**File**: `src/DistributedQueue.Core/Models/Topic.cs`

Replace `Queue<Message>` with `PriorityQueue<Message, MessagePriority>`

#### 3. Update API DTO
**File**: `src/DistributedQueue.Api/DTOs/Requests.cs`

```csharp
public class PublishMessageRequest
{
    // Existing properties...
    public MessagePriority Priority { get; set; } = MessagePriority.Normal;
}
```

## 🔌 Adding Kafka Integration

### 1. Update API to Use Kafka Producer

**File**: `src/DistributedQueue.Api/Program.cs`

```csharp
// Add Kafka configuration
builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection("KafkaSettings"));

// Register Kafka services
builder.Services.AddSingleton<IKafkaProducerService>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<KafkaSettings>>().Value;
    return new KafkaProducerService(settings.GetProducerConfig());
});
```

### 2. Create Hybrid Broker

**File**: `src/DistributedQueue.Core/Services/HybridMessageBroker.cs`

```csharp
public class HybridMessageBroker : IMessageBroker
{
    private readonly IKafkaProducerService _kafkaProducer;
    private readonly MessageBroker _inMemoryBroker;
    private readonly bool _useKafka;

    public async Task PublishMessage(string topicName, Message message)
    {
        if (_useKafka)
        {
            await _kafkaProducer.PublishMessageAsync(topicName, message);
        }
        else
        {
            _inMemoryBroker.PublishMessage(topicName, message);
        }
    }
}
```

## 🎨 Adding a Web UI

### Recommended Stack:
- **Frontend**: React, Vue.js, or Blazor
- **Communication**: SignalR for real-time updates
- **API**: Use existing REST endpoints

### 1. Add SignalR

```bash
dotnet add package Microsoft.AspNetCore.SignalR
```

### 2. Create Hub

**File**: `src/DistributedQueue.Api/Hubs/MessageHub.cs`

```csharp
public class MessageHub : Hub
{
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}
```

### 3. Update Program.cs

```csharp
builder.Services.AddSignalR();
app.MapHub<MessageHub>("/messageHub");
```

### 4. React Component Example

```jsx
import { HubConnectionBuilder } from '@microsoft/signalr';

function MessageMonitor() {
  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl("https://localhost:5001/messageHub")
      .build();

    connection.on("ReceiveMessage", (message) => {
      console.log(message);
    });

    connection.start();
  }, []);
}
```

## 📊 Adding Metrics and Monitoring

### 1. Add Application Insights

```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

### 2. Update Program.cs

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### 3. Add Custom Metrics

**File**: `src/DistributedQueue.Core/Services/MessageBroker.cs`

```csharp
private readonly TelemetryClient _telemetryClient;

public void PublishMessage(string topicName, Message message)
{
    // Existing code...
    
    _telemetryClient.TrackEvent("MessagePublished", 
        new Dictionary<string, string>
        {
            { "Topic", topicName },
            { "Producer", message.ProducerId }
        });
}
```

## 💾 Adding Persistence

### Example: Add SQL Server Storage

### 1. Add Entity Framework Core

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

### 2. Create DbContext

**File**: `src/DistributedQueue.Core/Data/QueueDbContext.cs`

```csharp
public class QueueDbContext : DbContext
{
    public DbSet<Message> Messages { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Consumer> Consumers { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entities
    }
}
```

### 3. Update Services to Use Repository Pattern

**File**: `src/DistributedQueue.Core/Repositories/IMessageRepository.cs`

```csharp
public interface IMessageRepository
{
    Task<Message> AddAsync(Message message);
    Task<Message?> GetByIdAsync(string id);
    Task<IEnumerable<Message>> GetByTopicAsync(string topicName);
}
```

## 🔒 Adding Authentication

### 1. Add JWT Authentication

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

### 2. Update Program.cs

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

app.UseAuthentication();
app.UseAuthorization();
```

### 3. Protect Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TopicsController : ControllerBase
{
    // Controller code...
}
```

## 🧪 Adding Unit Tests

### 1. Create Test Project

```bash
dotnet new xunit -n DistributedQueue.Tests -o tests/DistributedQueue.Tests
dotnet sln add tests/DistributedQueue.Tests/DistributedQueue.Tests.csproj
dotnet add tests/DistributedQueue.Tests reference src/DistributedQueue.Core
```

### 2. Add Moq for Mocking

```bash
cd tests/DistributedQueue.Tests
dotnet add package Moq
```

### 3. Example Test

**File**: `tests/DistributedQueue.Tests/Services/TopicManagerTests.cs`

```csharp
public class TopicManagerTests
{
    [Fact]
    public void CreateTopic_ShouldCreateNewTopic()
    {
        // Arrange
        var manager = new TopicManager();
        
        // Act
        var topic = manager.CreateTopic("test-topic");
        
        // Assert
        Assert.NotNull(topic);
        Assert.Equal("test-topic", topic.Name);
    }

    [Fact]
    public void CreateTopic_DuplicateName_ShouldThrowException()
    {
        // Arrange
        var manager = new TopicManager();
        manager.CreateTopic("test-topic");
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            manager.CreateTopic("test-topic"));
    }
}
```

## 🐳 Adding Docker Support

### 1. Create Dockerfile

**File**: `Dockerfile`

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/DistributedQueue.Api/DistributedQueue.Api.csproj", "DistributedQueue.Api/"]
COPY ["src/DistributedQueue.Core/DistributedQueue.Core.csproj", "DistributedQueue.Core/"]
RUN dotnet restore "DistributedQueue.Api/DistributedQueue.Api.csproj"
COPY src/ .
WORKDIR "/src/DistributedQueue.Api"
RUN dotnet build "DistributedQueue.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DistributedQueue.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DistributedQueue.Api.dll"]
```

### 2. Create docker-compose.yml

```yaml
version: '3.8'
services:
  api:
    build: .
    ports:
      - "5001:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
```

### 3. Build and Run

```bash
docker-compose up --build
```

## 📈 Adding Rate Limiting

### 1. Add Rate Limiting Package

```bash
dotnet add package AspNetCoreRateLimit
```

### 2. Configure Rate Limiting

**File**: `src/DistributedQueue.Api/Program.cs`

```csharp
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Limit = 100,
            Period = "1m"
        }
    };
});

builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

app.UseIpRateLimiting();
```

## 🔍 Best Practices for Extensions

1. **Follow SOLID Principles**: Keep classes focused on single responsibilities
2. **Use Interfaces**: Define interfaces for all new services
3. **Dependency Injection**: Register all services in DI container
4. **Configuration**: Use appsettings.json for configuration
5. **Logging**: Add logging for debugging and monitoring
6. **Error Handling**: Implement proper exception handling
7. **Testing**: Write unit tests for new features
8. **Documentation**: Update README.md and API documentation

## 📚 Useful Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Confluent Kafka .NET](https://docs.confluent.io/kafka-clients/dotnet/current/overview.html)
- [SignalR Documentation](https://docs.microsoft.com/aspnet/core/signalr)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Docker Documentation](https://docs.docker.com)
