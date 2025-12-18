# Distributed Queue System

A modular distributed queue system with support for **In-Memory**, **Kafka**, and **Hybrid** modes, built with C# and .NET 9.0.

> **🔄 Mode-Aware Architecture**: All controllers support In-Memory, Kafka, or Hybrid operation modes.  
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
.\test-api.ps1          # Basic test
.\test-api.ps1 -Verbose # Detailed output
.\test-api.ps1 -LocalDev # Test local dev (port 5297)
```

## 🎯 Operation Modes

The system supports three operation modes (configured in `appsettings.json`):

### **In-Memory Mode**
- All operations use in-memory data structures (ConcurrentDictionary)
- Fast, lightweight, perfect for development and testing
- Data is lost when application restarts

### **Kafka Mode**
- All operations use Confluent Cloud Kafka
- Production-ready, persistent, scalable
- Requires Kafka configuration in `appsettings.json`

### **Hybrid Mode** ⭐
- Operations work with **BOTH** in-memory and Kafka
- Best of both worlds: fast local + persistent cloud
- Messages published to both backends simultaneously
- Topics can exist in either or both backends

**Configure in appsettings.json:**
```json
{
  "QueueModeSettings": {
    "UseInMemory": true,
    "UseKafka": true,
    "EnableHybridMode": true
  }
}
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
REST API with mode-aware controllers:
- `TopicsController` - Topic CRUD with in-memory/Kafka/hybrid support
- `ProducersController` - Producer management + Kafka broker info
- `ConsumersController` - Consumer management + Kafka consumer group members
- `ConsumerGroupsController` - Consumer group management (both backends)
- `MessagesController` - Message publishing & retrieval (hybrid-aware)
- `DemoController` - Demonstration scenarios (works in all modes)
- `KafkaTestController` - Kafka diagnostics and testing

**Key Features:**
- All endpoints support `?source=inmemory` or `?source=kafka` filtering
- Responses include current mode information
- Hybrid mode returns combined data from both backends

### 3. **DistributedQueue.Kafka** (Class Library)
Confluent Cloud Kafka integration:
- `KafkaProducerService` - Kafka message publishing
- `KafkaConsumerService` - Kafka message consumption  
- `HybridMessageBroker` - Routes messages to appropriate backend(s)
- `KafkaSettings` - Configuration for Confluent Cloud
- AdminClient integration for metadata queries

## ✨ Features

- ✅ **Three Operation Modes**: In-Memory, Kafka, or Hybrid
- ✅ **Mode-Aware Controllers**: All endpoints work across all modes
- ✅ **Source Filtering**: Query specific backends with `?source=` parameter
- ✅ **Hybrid Message Publishing**: Publish to both backends simultaneously
- ✅ **Multiple Topics**: Support for multiple named topics
- ✅ **Multi-Producer/Consumer**: Multiple producers and consumers
- ✅ **Flexible Subscriptions**: Consumers can subscribe to multiple topics
- ✅ **Consumer Groups**: Support for consumer groups with load balancing
- ✅ **Kafka Integration**: Full Confluent Cloud Kafka support
- ✅ **Thread-Safe**: Concurrent message production and consumption
- ✅ **REST API**: Full RESTful API for all operations
- ✅ **Message Retrieval**: Read messages from Kafka topics via API
- ✅ **Plug & Play**: Easy to add/remove producers, consumers, and topics

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

### **Getting Started**
- **[docs/START_HERE.md](docs/START_HERE.md)** - ⭐ Best place to start!
- **[docs/QUICKSTART.md](docs/QUICKSTART.md)** - Local development guide
- **[docs/DEPLOY_README.md](docs/DEPLOY_README.md)** - Quick deployment guide
- **[docs/PODMAN_DEPLOYMENT.md](docs/PODMAN_DEPLOYMENT.md)** - Complete Podman guide

### **API Documentation**
- **[docs/API_REFERENCE.md](docs/API_REFERENCE.md)** - ⭐ Complete API guide with examples
- **[docs/QUICK_API_REFERENCE.md](docs/QUICK_API_REFERENCE.md)** - Quick reference card
- **[docs/API_EXAMPLES.md](docs/API_EXAMPLES.md)** - Sample request bodies

### **Architecture & Configuration**
- **[docs/COMPLETE_ARCHITECTURE_OVERVIEW.md](docs/COMPLETE_ARCHITECTURE_OVERVIEW.md)** - System architecture
- **[docs/MODE_SWITCHING.md](docs/MODE_SWITCHING.md)** - How to switch between modes
- **[docs/KAFKA_CONNECTION_TEST.md](docs/KAFKA_CONNECTION_TEST.md)** - Kafka setup guide

### **Reference**
- **[docs/INDEX.md](docs/INDEX.md)** - Complete documentation index
- **[docs/EXTENSION_GUIDE.md](docs/EXTENSION_GUIDE.md)** - How to extend the system
- **[docs/FOLDER_STRUCTURE.md](docs/FOLDER_STRUCTURE.md)** - Detailed folder structure

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

### Topics (Mode-Aware)
- `POST /api/topics` - Create a topic (in both backends if hybrid)
- `GET /api/topics` - Get all topics from configured backends
- `GET /api/topics?source=inmemory` - Get only in-memory topics
- `GET /api/topics?source=kafka` - Get only Kafka topics
- `DELETE /api/topics/{topicName}` - Delete a topic
- `DELETE /api/topics/{topicName}?source=kafka` - Delete from specific backend

### Producers (Mode-Aware)
- `POST /api/producers` - Create a producer
- `GET /api/producers` - Get all producers + Kafka broker endpoints
- `DELETE /api/producers/{producerId}` - Delete a producer

### Consumers (Mode-Aware)
- `POST /api/consumers` - Create a consumer (in-memory)
- `GET /api/consumers` - Get in-memory consumers + Kafka consumer group members
- `POST /api/consumers/subscribe` - Subscribe consumer to topic
- `POST /api/consumers/{consumerId}/start` - Start consumer
- `POST /api/consumers/{consumerId}/stop` - Stop consumer
- `DELETE /api/consumers/{consumerId}` - Delete a consumer

### Messages (Hybrid-Aware)
- `POST /api/messages/publish` - Publish to configured backend(s)
- `GET /api/messages/topic/{topicName}?source=kafka&offset=earliest&limit=50` - Get messages from Kafka
- `GET /api/messages/topic/{topicName}?source=inmemory` - Get in-memory topic info
- `GET /api/messages/stats` - Get message statistics

### Consumer Groups (Mode-Aware)
- `POST /api/consumergroups` - Create a consumer group
- `GET /api/consumergroups` - Get in-memory + Kafka consumer groups
- `DELETE /api/consumergroups/{groupName}` - Delete a group

### Demo (Works in All Modes)
- `POST /api/demo/run-scenario` - Run complete demo
- `POST /api/demo/cleanup` - Cleanup demo resources

### Kafka Test (Kafka Mode Only)
- `POST /api/kafkatest/test-connection` - Test Kafka connectivity
- `POST /api/kafkatest/send-test-message` - Send test message to Kafka
- `GET /api/kafkatest/show-config` - Display Kafka configuration

> **💡 Tip**: See [docs/API_REFERENCE.md](docs/API_REFERENCE.md) for complete API documentation with examples

## 🎯 Demo Scenario

Run the built-in demo scenario that demonstrates the queue system in action:

### Quick Run
```powershell
# Using test script (recommended)
cd scripts
.\test-api.ps1

# Or run the user guide for interactive examples
.\user-guide.ps1
```

### Using Swagger UI
1. Navigate to http://localhost:8080/swagger (Podman) or http://localhost:5297/swagger (local)
2. Find the **Demo** section
3. Execute `POST /api/demo/run-scenario`
4. Check response for demo steps

### Using PowerShell
```powershell
# Run demo scenario
Invoke-WebRequest -Uri http://localhost:5297/api/demo/run-scenario -Method POST

# Cleanup demo resources
Invoke-WebRequest -Uri http://localhost:5297/api/demo/cleanup -Method POST
```

**The demo scenario:**
1. Creates 2 topics (demo-topic1, demo-topic2)
2. Creates 2 producers (demo-producer1, demo-producer2)
3. In-memory mode: Creates 5 consumers and subscribes them to topics
4. Publishes 5 messages to demonstrate message routing
5. Shows mode-specific behavior (In-Memory/Kafka/Hybrid)

**Check the console output** for in-memory message consumption:
```
consumer1 received Demo Message 1
consumer2 received Demo Message 2
...
```

## 🧪 Example Usage

### Quick Examples

```powershell
# Create a topic
POST /api/topics
{"topicName": "notifications"}

# Create a producer
POST /api/producers
{"producerId": "notifier", "name": "Notification Service"}

# Publish a message (goes to configured backend(s))
POST /api/messages/publish
{
  "producerId": "notifier",
  "topicName": "notifications",
  "content": "New user registered"
}

# Get messages from Kafka
GET /api/messages/topic/notifications?source=kafka&offset=earliest&limit=50

# Create and start a consumer (in-memory)
POST /api/consumers
{"consumerId": "processor", "name": "Message Processor"}

POST /api/consumers/subscribe
{"consumerId": "processor", "topicName": "notifications"}

POST /api/consumers/processor/start
```

### PowerShell Scripts

```powershell
# Comprehensive API testing
.\scripts\test-api.ps1 -Verbose

# Interactive user guide with examples
.\scripts\user-guide.ps1

# Test Kafka connectivity
.\scripts\test-kafka-connection.ps1
```

## 🔄 Confluent Cloud Kafka Integration

The system includes full Confluent Cloud Kafka integration. To enable Kafka mode:

### 1. Configure Kafka Settings

Update `appsettings.json`:
```json
{
  "QueueModeSettings": {
    "UseInMemory": false,
    "UseKafka": true,
    "EnableHybridMode": false
  },
  "KafkaSettings": {
    "BootstrapServers": "pkc-xxxxx.eastus.azure.confluent.cloud:9092",
    "SaslUsername": "YOUR-API-KEY",
    "SaslPassword": "YOUR-API-SECRET",
    "GroupId": "distributed-queue-group"
  }
}
```

### 2. Enable Hybrid Mode (Optional)

For both in-memory and Kafka:
```json
{
  "QueueModeSettings": {
    "UseInMemory": true,
    "UseKafka": true,
    "EnableHybridMode": true
  }
}
```

### 3. Test Kafka Connection

```powershell
# Using test script
.\scripts\test-kafka-connection.ps1

# Or using API endpoint
POST /api/kafkatest/test-connection
```

See [docs/KAFKA_CONNECTION_TEST.md](docs/KAFKA_CONNECTION_TEST.md) for detailed setup guide.

## 🏛️ Design Principles

- **Mode-Aware Architecture**: All components support In-Memory, Kafka, and Hybrid modes
- **Separation of Concerns**: Clear separation between domain logic, services, and API
- **Dependency Injection**: All services are injected via DI container
- **Thread Safety**: Concurrent collections and locking mechanisms
- **SOLID Principles**: Interface-based design, single responsibility
- **Extensibility**: Easy to add new features and integrations
- **Hybrid Publishing**: Messages can be published to multiple backends simultaneously

## 🎨 Future Enhancements

- [x] **Mode-Aware Controllers** - Completed ✅
- [x] **Kafka Integration** - Completed ✅
- [x] **Hybrid Mode Support** - Completed ✅
- [x] **Message Retrieval from Kafka** - Completed ✅
- [ ] Add web-based GUI for management
- [ ] Implement message persistence to file system
- [ ] Add metrics and monitoring dashboard
- [ ] Support for message filtering and routing
- [ ] Dead letter queue support
- [ ] Message retry mechanisms with backoff
- [ ] Authentication and authorization
- [ ] Enhanced Docker/Kubernetes deployment
- [ ] Comprehensive unit and integration tests
- [ ] Message compression support
- [ ] Schema registry integration

## 📊 Scripts Reference

All PowerShell scripts are in the `scripts/` folder:

| Script | Description | Usage |
|--------|-------------|-------|
| **deploy-podman.ps1** | Deploy to Podman Desktop | `.\deploy-podman.ps1` |
| **start.ps1** | Run locally for development | `.\start.ps1` |
| **test-api.ps1** | Comprehensive API testing | `.\test-api.ps1 -Verbose` |
| **user-guide.ps1** | Interactive API guide | `.\user-guide.ps1` |
| **view-logs.ps1** | View container logs | `.\view-logs.ps1` |
| **stop-podman.ps1** | Stop Podman container | `.\stop-podman.ps1` |
| **test-kafka-connection.ps1** | Test Kafka connectivity | `.\test-kafka-connection.ps1` |

## 🆘 Troubleshooting

### Common Issues

**Q: API not responding after deployment**
- Check if container is running: `podman ps`
- View logs: `.\scripts\view-logs.ps1`
- Restart: `.\scripts\stop-podman.ps1` then `.\scripts\deploy-podman.ps1`

**Q: Messages not publishing to Kafka**
- Verify Kafka settings in `appsettings.json`
- Test connection: `POST /api/kafkatest/test-connection`
- Check mode settings: Ensure `UseKafka: true`
- See: [docs/KAFKA_TROUBLESHOOTING_SOLUTION.md](docs/KAFKA_TROUBLESHOOTING_SOLUTION.md)

**Q: Can't retrieve messages from Kafka topic**
- Ensure messages were published: Check Confluent Cloud UI
- Use `offset=earliest` to read from beginning
- Increase `limit` parameter if needed
- Topic may be empty or recently created

**Q: Hybrid mode not working**
- Verify all three settings are true in `appsettings.json`:
  ```json
  {
    "UseInMemory": true,
    "UseKafka": true,
    "EnableHybridMode": true
  }
  ```
- Restart the application after configuration changes

## 📚 Additional Resources

- **Complete API Reference**: [docs/API_REFERENCE.md](docs/API_REFERENCE.md)
- **Quick Reference**: [docs/QUICK_API_REFERENCE.md](docs/QUICK_API_REFERENCE.md)
- **Documentation Index**: [docs/INDEX.md](docs/INDEX.md)
- **Kafka Setup**: [docs/KAFKA_CONNECTION_TEST.md](docs/KAFKA_CONNECTION_TEST.md)
- **Mode Switching**: [docs/MODE_SWITCHING.md](docs/MODE_SWITCHING.md)
- **Architecture**: [docs/COMPLETE_ARCHITECTURE_OVERVIEW.md](docs/COMPLETE_ARCHITECTURE_OVERVIEW.md)

## 🤝 Contributing

This is a demonstration project. Feel free to fork and extend for your needs.

## 📄 License

This project is for demonstration and educational purposes.

---

**Quick Start**: Run `.\scripts\start.ps1` | **Documentation**: [docs/INDEX.md](docs/INDEX.md) | **API Reference**: [docs/API_REFERENCE.md](docs/API_REFERENCE.md)

