# Distributed Queue System

A modular, in-memory distributed queue system similar to Kafka, built with C# and .NET 9.0.

> **📁 Ultra-Clean Structure**: All files organized into 4 main folders. See [docs/FOLDER_STRUCTURE.md](docs/FOLDER_STRUCTURE.md) for details.

## 🚀 Quick Start

### Deploy to Podman Desktop (Recommended)
```powershell
cd scripts
.\deploy-podman.ps1
```

### Run Locally (Development)
```powershell
cd scripts
.\start.ps1
```

### Test the Deployment
```powershell
cd scripts
.\test-api.ps1
```

## 📁 Project Structure

```
DistributedQueue/
├── 📁 src/                          # Source code
│   ├── DistributedQueue.Core/       # Core domain models and services
│   ├── DistributedQueue.Api/        # REST API with Swagger
│   └── DistributedQueue.Kafka/      # Confluent Kafka integration
│
├── 📁 scripts/                      # PowerShell scripts
│   ├── deploy-podman.ps1            # 🐳 Deploy to Podman
│   ├── start.ps1                    # 💻 Run locally
│   ├── test-api.ps1                 # 🧪 Test the API
│   ├── view-logs.ps1                # 📊 View container logs
│   └── stop-podman.ps1              # 🛑 Stop container
│
├── 📁 docker/                       # Docker/Podman files
│   ├── Dockerfile                   # Multi-stage build config
│   ├── docker-compose.yml           # Compose configuration
│   └── .dockerignore                # Build context exclusions
│
├── 📁 docs/                         # All documentation
│   ├── START_HERE.md                # ⭐ Quick start guide
│   ├── INDEX.md                     # Complete documentation index
│   ├── QUICK_REF.md                 # Quick reference card
│   ├── FOLDER_STRUCTURE.md          # Detailed folder structure
│   ├── ProblemStatement.md          # Original requirements
│   ├── DEPLOY_README.md             # Deployment quick guide
│   ├── PODMAN_DEPLOYMENT.md         # Full deployment guide
│   ├── DEPLOYMENT_CHECKLIST.md      # Verification checklist
│   ├── DEPLOYMENT_FLOW.md           # Visual flow diagrams
│   ├── QUICKSTART.md                # Local development guide
│   ├── API_EXAMPLES.md              # API usage examples
│   ├── PROJECT_SUMMARY.md           # Project overview
│   ├── STRUCTURE.md                 # Architecture details
│   ├── EXTENSION_GUIDE.md           # How to extend
│   ├── REORGANIZATION.md            # What changed
│   └── START.md                     # Visual summary
│
├── 📄 README.md                     # This file
├── 📄 .gitignore                    # Git ignore rules
└── 📄 DistributedQueue.sln          # Solution file
```

> **📚 All documentation**: See [docs/INDEX.md](docs/INDEX.md) for complete documentation index

## 🏗️ Architecture

The solution consists of three main projects:

### 1. **DistributedQueue.Core** (Class Library)
Core domain models and business logic:
- **Models**: `Message`, `Topic`, `Producer`, `Consumer`, `ConsumerGroup`
- **Services**: 
  - `TopicManager` - Manages topic lifecycle
  - `ProducerManager` - Manages producers
  - `ConsumerManager` - Manages consumers and subscriptions
  - `ConsumerGroupManager` - Manages consumer groups
  - `MessageBroker` - Handles message publishing and consumption

### 2. **DistributedQueue.Api** (Web API)
REST API with the following controllers:
- `TopicsController` - Topic CRUD operations
- `ProducersController` - Producer management
- `ConsumersController` - Consumer management and subscriptions
- `MessagesController` - Message publishing
- `ConsumerGroupsController` - Consumer group management
- `DemoController` - Demonstration scenarios

### 3. **DistributedQueue.Kafka** (Class Library)
Confluent Cloud Kafka integration (ready for future use):
- `KafkaProducerService` - Kafka message publishing
- `KafkaConsumerService` - Kafka message consumption
- `KafkaSettings` - Configuration for Confluent Cloud

## ✨ Features

- ✅ **In-Memory Queue**: No file system dependency
- ✅ **Multiple Topics**: Support for multiple named topics
- ✅ **Multi-Producer/Consumer**: Multiple producers and consumers
- ✅ **Flexible Subscriptions**: Consumers can subscribe to multiple topics
- ✅ **Consumer Groups**: Support for consumer groups with load balancing
- ✅ **Thread-Safe**: Concurrent message production and consumption
- ✅ **REST API**: Full RESTful API for all operations
- ✅ **Plug & Play**: Easy to add/remove producers, consumers, and topics
- ✅ **Kafka-Ready**: Prepared for Confluent Cloud Kafka integration

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK or later
- Podman Desktop (for containerized deployment)

### Option 1: Deploy to Podman (Recommended)
```bash
cd scripts
.\deploy-podman.ps1
```
Access at: http://localhost:8080/swagger

### Option 2: Run Locally
```bash
cd scripts
.\start.ps1
```
Access at: https://localhost:5001/swagger

## 📚 Documentation

All documentation is in the `docs/` folder:

- **[docs/START_HERE.md](docs/START_HERE.md)** - ⭐ Best place to start!
- **[docs/INDEX.md](docs/INDEX.md)** - Complete documentation index
- **[docs/QUICK_REF.md](docs/QUICK_REF.md)** - Quick reference card
- **[docs/DEPLOY_README.md](docs/DEPLOY_README.md)** - Quick deployment guide
- **[docs/PODMAN_DEPLOYMENT.md](docs/PODMAN_DEPLOYMENT.md)** - Complete Podman guide
- **[docs/QUICKSTART.md](docs/QUICKSTART.md)** - Local development guide
- **[docs/API_EXAMPLES.md](docs/API_EXAMPLES.md)** - API usage examples
- **[docs/EXTENSION_GUIDE.md](docs/EXTENSION_GUIDE.md)** - How to extend the system

## 🧪 Running the Demo

### Using Podman
```powershell
cd scripts
.\deploy-podman.ps1
.\test-api.ps1
.\view-logs.ps1
```

### Using Swagger UI
1. Navigate to http://localhost:8080/swagger (Podman) or https://localhost:5001/swagger (local)
2. Find the **Demo** section
3. Execute `POST /api/demo/run-scenario`
4. Check console/logs for output:
   ```
   consumer1 received Message 1
   consumer2 received Message 2
   consumer3 received Message 3
   ...
   ```

## 📝 API Endpoints

### Topics
- `POST /api/topics` - Create a topic
- `GET /api/topics` - Get all topics
- `GET /api/topics/{topicName}` - Get topic details
- `DELETE /api/topics/{topicName}` - Delete a topic

### Producers
- `POST /api/producers` - Create a producer
- `GET /api/producers` - Get all producers
- `GET /api/producers/{producerId}` - Get producer details
- `DELETE /api/producers/{producerId}` - Delete a producer

### Consumers
- `POST /api/consumers` - Create a consumer
- `GET /api/consumers` - Get all consumers
- `GET /api/consumers/{consumerId}` - Get consumer details
- `POST /api/consumers/subscribe` - Subscribe consumer to topic
- `POST /api/consumers/{consumerId}/start` - Start consumer
- `POST /api/consumers/{consumerId}/stop` - Stop consumer
- `DELETE /api/consumers/{consumerId}` - Delete a consumer

### Messages
- `POST /api/messages/publish` - Publish a message to a topic

### Consumer Groups
- `POST /api/consumergroups` - Create a consumer group
- `GET /api/consumergroups` - Get all consumer groups
- `GET /api/consumergroups/{groupName}` - Get group details
- `DELETE /api/consumergroups/{groupName}` - Delete a group

### Demo
- `POST /api/demo/run-scenario` - Run the demonstration scenario
- `POST /api/demo/cleanup` - Clean up all resources

## 🎯 Demo Scenario

Run the built-in demo scenario that matches the problem statement:

### Using Test Script
```bash
cd scripts
.\test-api.ps1
```

### Using curl
```bash
curl -X POST http://localhost:8080/api/demo/run-scenario
```

### Using PowerShell
```powershell
Invoke-WebRequest -Uri http://localhost:8080/api/demo/run-scenario -Method POST
```

This will:
1. Create 2 topics (topic1, topic2)
2. Create 2 producers (producer1, producer2)
3. Create 5 consumers (consumer1-5)
4. Subscribe all consumers to topic1
5. Subscribe consumers 1, 3, 4 to topic2
6. Publish messages as specified
7. Display consumption logs

**Check the console output** to see messages like:
```
consumer1 received Message 1
consumer2 received Message 2
consumer3 received Message 3
...
```

## 🧪 Example Usage

### Create a Topic
```bash
POST /api/topics
{
  "topicName": "my-topic"
}
```

### Create a Producer
```bash
POST /api/producers
{
  "producerId": "producer1",
  "name": "My Producer"
}
```

### Create a Consumer with Consumer Group
```bash
POST /api/consumers
{
  "consumerId": "consumer1",
  "name": "My Consumer",
  "consumerGroup": "group1"
}
```

### Subscribe Consumer to Topic
```bash
POST /api/consumers/subscribe
{
  "consumerId": "consumer1",
  "topicName": "my-topic"
}
```

### Start Consumer
```bash
POST /api/consumers/consumer1/start
```

### Publish a Message
```bash
POST /api/messages/publish
{
  "producerId": "producer1",
  "topicName": "my-topic",
  "content": "Hello, World!"
}
```

## 🔄 Confluent Cloud Kafka Integration

The `DistributedQueue.Kafka` project is ready for Confluent Cloud integration. To use it:

1. Update `appsettings.json` with your Kafka credentials:
```json
{
  "KafkaSettings": {
    "BootstrapServers": "your-cluster.cloud.confluent.com:9092",
    "SaslUsername": "your-api-key",
    "SaslPassword": "your-api-secret",
    "GroupId": "distributed-queue-group"
  }
}
```

2. Register services in `Program.cs`:
```csharp
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("KafkaSettings"));
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddSingleton<IKafkaConsumerService, KafkaConsumerService>();
```

## 🏛️ Design Principles

- **Separation of Concerns**: Clear separation between domain logic, services, and API
- **Dependency Injection**: All services are injected via DI
- **Thread Safety**: Concurrent collections and locking mechanisms
- **SOLID Principles**: Interface-based design, single responsibility
- **Extensibility**: Easy to add new features and integrations

## 📦 Project Structure

```
DistributedQueue/
├── src/
│   ├── DistributedQueue.Core/
│   │   ├── Models/
│   │   │   ├── Message.cs
│   │   │   ├── Topic.cs
│   │   │   ├── Producer.cs
│   │   │   ├── Consumer.cs
│   │   │   └── ConsumerGroup.cs
│   │   └── Services/
│   │       ├── MessageBroker.cs
│   │       ├── TopicManager.cs
│   │       ├── ProducerManager.cs
│   │       ├── ConsumerManager.cs
│   │       └── ConsumerGroupManager.cs
│   ├── DistributedQueue.Api/
│   │   ├── Controllers/
│   │   │   ├── TopicsController.cs
│   │   │   ├── ProducersController.cs
│   │   │   ├── ConsumersController.cs
│   │   │   ├── MessagesController.cs
│   │   │   ├── ConsumerGroupsController.cs
│   │   │   └── DemoController.cs
│   │   ├── DTOs/
│   │   │   └── Requests.cs
│   │   └── Program.cs
│   └── DistributedQueue.Kafka/
│       ├── Producers/
│       │   └── KafkaProducerService.cs
│       ├── Consumers/
│       │   └── KafkaConsumerService.cs
│       └── Configuration/
│           └── KafkaSettings.cs
└── DistributedQueue.sln
```

## 🎨 Future Enhancements

- [ ] Add web-based GUI for management
- [ ] Implement message persistence
- [ ] Add metrics and monitoring
- [ ] Support for message filtering
- [ ] Dead letter queue support
- [ ] Message retry mechanisms
- [ ] Authentication and authorization
- [ ] Docker containerization
- [ ] Unit and integration tests

## 📄 License

This project is for demonstration and educational purposes.
