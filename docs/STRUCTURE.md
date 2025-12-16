# Project Structure

```
DistributedQueue/
│
├── 📄 DistributedQueue.sln                    # Solution file
├── 📄 .gitignore                              # Git ignore rules
├── 📄 README.md                               # Main documentation
├── 📄 QUICKSTART.md                           # Quick start guide
├── 📄 API_EXAMPLES.md                         # API usage examples
├── 📄 PROJECT_SUMMARY.md                      # Project summary
├── 📄 ProblemStatement.md                     # Original requirements
├── 📄 start.ps1                               # Quick start script
│
└── 📁 src/
    │
    ├── 📁 DistributedQueue.Core/              # Core Library
    │   ├── 📁 Models/
    │   │   ├── Message.cs                     # Message entity
    │   │   ├── Topic.cs                       # Topic with message queue
    │   │   ├── Producer.cs                    # Producer entity
    │   │   ├── Consumer.cs                    # Consumer entity
    │   │   └── ConsumerGroup.cs               # Consumer group entity
    │   │
    │   └── 📁 Services/
    │       ├── MessageBroker.cs               # Message broker service
    │       ├── TopicManager.cs                # Topic management
    │       ├── ProducerManager.cs             # Producer management
    │       ├── ConsumerManager.cs             # Consumer management
    │       └── ConsumerGroupManager.cs        # Consumer group management
    │
    ├── 📁 DistributedQueue.Api/               # Web API
    │   ├── 📁 Controllers/
    │   │   ├── TopicsController.cs            # Topic endpoints
    │   │   ├── ProducersController.cs         # Producer endpoints
    │   │   ├── ConsumersController.cs         # Consumer endpoints
    │   │   ├── MessagesController.cs          # Message endpoints
    │   │   ├── ConsumerGroupsController.cs    # Consumer group endpoints
    │   │   └── DemoController.cs              # Demo scenario
    │   │
    │   ├── 📁 DTOs/
    │   │   └── Requests.cs                    # Request DTOs
    │   │
    │   ├── Program.cs                         # Application entry point
    │   ├── appsettings.json                   # Configuration
    │   └── appsettings.Development.json       # Development config
    │
    └── 📁 DistributedQueue.Kafka/             # Kafka Integration
        ├── 📁 Producers/
        │   └── KafkaProducerService.cs        # Kafka producer
        │
        ├── 📁 Consumers/
        │   └── KafkaConsumerService.cs        # Kafka consumer
        │
        └── 📁 Configuration/
            └── KafkaSettings.cs               # Kafka configuration
```

## Component Relationships

```
┌─────────────────────────────────────────────────────────────┐
│                    DistributedQueue.Api                      │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐            │
│  │  Topics    │  │ Producers  │  │ Consumers  │            │
│  │ Controller │  │ Controller │  │ Controller │            │
│  └──────┬─────┘  └──────┬─────┘  └──────┬─────┘            │
│         │                │                │                  │
│         └────────────────┴────────────────┘                  │
│                          │                                   │
└──────────────────────────┼───────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                   DistributedQueue.Core                      │
│  ┌──────────────────────────────────────────────────┐       │
│  │              MessageBroker Service                │       │
│  │  (Coordinates producers, consumers, messages)     │       │
│  └───────┬──────────────────┬──────────────┬────────┘       │
│          │                  │              │                 │
│  ┌───────▼──────┐  ┌────────▼─────┐  ┌───▼──────────┐      │
│  │    Topic     │  │   Producer    │  │   Consumer   │      │
│  │   Manager    │  │   Manager     │  │   Manager    │      │
│  └───────┬──────┘  └────────┬─────┘  └───┬──────────┘      │
│          │                  │              │                 │
│  ┌───────▼──────────────────▼──────────────▼─────┐          │
│  │          Domain Models Layer                   │          │
│  │  Topic  │  Producer  │  Consumer  │  Message   │          │
│  └────────────────────────────────────────────────┘          │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│              DistributedQueue.Kafka (Future)                 │
│  ┌──────────────────┐      ┌──────────────────┐            │
│  │ Kafka Producer   │      │ Kafka Consumer   │            │
│  │    Service       │      │    Service       │            │
│  └──────────────────┘      └──────────────────┘            │
│                                                              │
│  (Ready for Confluent Cloud Integration)                    │
└─────────────────────────────────────────────────────────────┘
```

## Data Flow

### Message Publishing Flow:
```
Producer → API Controller → MessageBroker → Topic → Consumer
   ↓           ↓                ↓             ↓         ↓
Create    Validate        Add to Queue   Store    Consume
          Producer        
```

### Consumer Subscription Flow:
```
Consumer → API Controller → ConsumerManager → Topic Subscription
   ↓           ↓                  ↓                  ↓
Create    Subscribe          Register          Active
          Request           Subscription      Listening
```

### Consumer Group Flow:
```
Message → Topic → ConsumerGroup → Round-Robin → Consumer
   ↓        ↓          ↓              ↓            ↓
Publish  Queue   Select Next     Distribute   Consume
                  Consumer
```

## Technology Stack

- **Framework**: .NET 9.0
- **API**: ASP.NET Core Web API
- **Documentation**: Swagger/OpenAPI
- **Concurrency**: ConcurrentDictionary, ConcurrentBag
- **DI Container**: Built-in .NET DI
- **Messaging**: Confluent.Kafka (ready for integration)
- **Architecture**: Layered (Models → Services → Controllers)

## Key Design Decisions

1. **In-Memory Storage**: Using ConcurrentDictionary for thread-safe in-memory storage
2. **Singleton Services**: Managers registered as singletons to maintain state
3. **Interface-Based**: All services have interfaces for testability
4. **RESTful API**: Standard HTTP methods and resource-based URLs
5. **Async/Await**: Asynchronous message processing
6. **Cancellation Tokens**: Graceful consumer shutdown
7. **Thread Safety**: Proper locking and concurrent collections
8. **Separation of Concerns**: Clear boundaries between layers
