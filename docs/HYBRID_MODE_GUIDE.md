# 🔄 Hybrid Mode Guide - In-Memory + Confluent Cloud Kafka

## 🎯 What is Hybrid Mode?

The Distributed Queue System now supports **three operation modes**:

1. **In-Memory Only** - Fast, ephemeral, perfect for development
2. **Kafka Only** - Persistent, distributed, production-ready
3. **Hybrid Mode** - Messages sent to **BOTH** systems simultaneously

## 🚀 Quick Start

### Check Current Mode

```bash
GET http://localhost:8080/api/config/status
```

Or visit Swagger UI:
```
http://localhost:8080/swagger
```

Look for the `/api/config/status` endpoint.

## ⚙️ Configuration

Edit `appsettings.json` or `appsettings.Development.json`:

### Mode 1: In-Memory Only (Default - Development)

```json
{
  "QueueMode": {
    "UseInMemory": true,
    "UseKafka": false,
    "EnableHybridMode": false
  },
  "KafkaSettings": {
    "Enabled": false
  }
}
```

**Benefits:**
- ✅ No Kafka setup required
- ✅ Fast (no network calls)
- ✅ Perfect for local testing
- ⚠️ Data lost when app stops

### Mode 2: Kafka Only (Production)

```json
{
  "QueueMode": {
    "UseInMemory": false,
    "UseKafka": true,
    "EnableHybridMode": false
  },
  "KafkaSettings": {
    "Enabled": true,
    "BootstrapServers": "pkc-xxxxx.eastus.azure.confluent.cloud:9092",
    "SaslUsername": "YOUR_API_KEY",
    "SaslPassword": "YOUR_API_SECRET",
    "SecurityProtocol": "SASL_SSL",
    "SaslMechanism": "PLAIN",
    "GroupId": "distributed-queue-consumer-group"
  }
}
```

**Benefits:**
- ✅ Persistent storage
- ✅ Distributed processing
- ✅ Production-ready
- ✅ Messages in Confluent Cloud console
- ⚠️ Requires Kafka setup
- ⚠️ Network latency

### Mode 3: Hybrid (Both Systems)

```json
{
  "QueueMode": {
    "UseInMemory": true,
    "UseKafka": true,
    "EnableHybridMode": true
  },
  "KafkaSettings": {
    "Enabled": true,
    "BootstrapServers": "pkc-xxxxx.eastus.azure.confluent.cloud:9092",
    "SaslUsername": "YOUR_API_KEY",
    "SaslPassword": "YOUR_API_SECRET",
    "SecurityProtocol": "SASL_SSL",
    "SaslMechanism": "PLAIN",
    "GroupId": "distributed-queue-consumer-group"
  }
}
```

**Benefits:**
- ✅ Fast local testing (in-memory)
- ✅ Persistent backup (Kafka)
- ✅ Messages appear in Confluent Cloud
- ✅ Best of both worlds
- 💡 Great for migration scenarios
- ⚠️ Double storage (RAM + Kafka)

## 📊 How It Works

### Message Publishing Flow

```
API Request
    ↓
POST /api/messages/publish
    ↓
HybridMessageBroker.PublishMessageAsync()
    ↓
    ├── [If UseInMemory=true]
    │   └── MessageBroker.PublishMessage() → RAM
    │
    └── [If UseKafka=true]
        └── KafkaProducerService.PublishMessageAsync() → Confluent Cloud
```

### Hybrid Mode (Both Enabled)

```
Message
    ↓
    ├─→ In-Memory Queue (fast, local consumers)
    │   └── ConcurrentDictionary in RAM
    │
    └─→ Kafka Topic (persistent, cloud)
        └── Confluent Cloud → Visible in console!
```

## 🎮 Using the Hybrid System

### 1. Check Status

```bash
GET /api/config/status
```

**Response:**
```json
{
  "mode": "Hybrid (In-Memory + Kafka)",
  "configuration": {
    "inMemory": {
      "enabled": true,
      "description": "Fast, ephemeral, in-process queue"
    },
    "kafka": {
      "enabled": true,
      "configured": true,
      "bootstrapServers": "pkc-xxxxx.eastus.azure.confluent.cloud:9092",
      "groupId": "distributed-queue-consumer-group",
      "description": "Persistent, distributed, Confluent Cloud"
    },
    "hybrid": {
      "enabled": true,
      "description": "Messages sent to BOTH in-memory and Kafka simultaneously"
    }
  }
}
```

### 2. Create Topic

```bash
POST /api/topics
{
  "topicName": "test-topic"
}
```

### 3. Create Producer

```bash
POST /api/producers
{
  "producerId": "producer1",
  "name": "Test Producer"
}
```

### 4. Publish Message (Goes to BOTH Systems!)

```bash
POST /api/messages/publish
{
  "producerId": "producer1",
  "topicName": "test-topic",
  "content": "Hello Hybrid World!"
}
```

**What Happens:**
1. ✅ Message stored in **in-memory queue** (RAM)
2. ✅ Message sent to **Confluent Cloud Kafka**
3. ✅ Visible in Confluent Cloud console!

### 5. Check Confluent Cloud

1. Go to https://confluent.cloud
2. Select your cluster
3. Go to **Topics**
4. Find `test-topic`
5. See your message! 🎉

## 🔍 Logs

When publishing in hybrid mode, you'll see:

```
📦 Publishing to IN-MEMORY queue: Topic=test-topic, MessageId=abc123
☁️ Publishing to KAFKA: Topic=test-topic, MessageId=abc123
Message delivered to test-topic [0] at offset 42
✅ HYBRID: Message published to BOTH in-memory and Kafka!
```

## 🎛️ Switching Modes

### Switch to In-Memory Only

Edit `appsettings.Development.json`:
```json
{
  "QueueMode": {
    "UseInMemory": true,
    "UseKafka": false,
    "EnableHybridMode": false
  }
}
```

Restart the app:
```bash
dotnet run
```

Logs:
```
📦 Kafka is DISABLED. Using in-memory only.
🚀 Queue System started in mode: In-Memory Only
```

### Switch to Kafka Only

```json
{
  "QueueMode": {
    "UseInMemory": false,
    "UseKafka": true,
    "EnableHybridMode": false
  },
  "KafkaSettings": {
    "Enabled": true
  }
}
```

Logs:
```
☁️ Kafka is ENABLED and configured
   Bootstrap: pkc-xxxxx.eastus.azure.confluent.cloud:9092
🚀 Queue System started in mode: Kafka Only
```

### Enable Hybrid Mode

```json
{
  "QueueMode": {
    "UseInMemory": true,
    "UseKafka": true,
    "EnableHybridMode": true
  },
  "KafkaSettings": {
    "Enabled": true
  }
}
```

Logs:
```
☁️ Kafka is ENABLED and configured
🚀 Queue System started in mode: Hybrid (In-Memory + Kafka)
```

## 📋 API Endpoints

### Configuration Controller

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/config/status` | GET | Current mode and configuration |
| `/api/config/recommendations` | GET | Configuration recommendations |
| `/api/config/examples` | GET | Configuration examples |

### Health Check

```bash
GET /api/status
```

**Response:**
```json
{
  "mode": "Hybrid (In-Memory + Kafka)",
  "inMemoryEnabled": true,
  "kafkaEnabled": true,
  "hybridEnabled": true,
  "kafkaConfigured": true,
  "kafkaBootstrap": "pkc-xxxxx.eastus.azure.confluent.cloud:9092"
}
```

## 🐛 Troubleshooting

### Messages Not Appearing in Confluent Cloud

**Check:**
1. Is `UseKafka` = `true`?
2. Is `KafkaSettings.Enabled` = `true`?
3. Are credentials valid?
4. Check logs for Kafka errors

**Test Configuration:**
```bash
GET /api/config/status
```

Look for `kafka.configured: true`.

### Hybrid Mode Not Working

**Requirements:**
- ✅ `UseInMemory` = `true`
- ✅ `UseKafka` = `true`
- ✅ `EnableHybridMode` = `true`
- ✅ `KafkaSettings.Enabled` = `true`
- ✅ Valid Kafka credentials

**Check Recommendations:**
```bash
GET /api/config/recommendations
```

### Kafka Connection Errors

**Error:** `Delivery failed: Authentication failure`

**Fix:** Check your API key and secret in `appsettings.Development.json`.

**Error:** `Broker: Topic does not exist`

**Solution:** Create topic in Confluent Cloud first, or enable auto-create topics.

## 🎯 Use Cases

### Development (In-Memory Only)
```json
{ "UseInMemory": true, "UseKafka": false, "EnableHybridMode": false }
```
- Fast iteration
- No cloud costs
- Easy debugging

### Testing (In-Memory + Kafka Ready)
```json
{ "UseInMemory": true, "UseKafka": false, "EnableHybridMode": false }
```
- Keep Kafka configured but disabled
- Quick toggle for cloud testing
- Cost-effective

### Staging (Hybrid)
```json
{ "UseInMemory": true, "UseKafka": true, "EnableHybridMode": true }
```
- Verify cloud integration
- Fast local testing
- Redundancy

### Production (Kafka Only)
```json
{ "UseInMemory": false, "UseKafka": true, "EnableHybridMode": false }
```
- Persistent storage
- Distributed processing
- Cloud-native

### Migration (Hybrid)
```json
{ "UseInMemory": true, "UseKafka": true, "EnableHybridMode": true }
```
- Gradual migration from in-memory to Kafka
- Compare results
- Fallback option

## 🔒 Security Best Practices

### Never Commit Credentials

Add to `.gitignore`:
```
appsettings.Development.json
appsettings.Production.json
```

### Use Environment Variables

```bash
export KafkaSettings__SaslUsername="YOUR_KEY"
export KafkaSettings__SaslPassword="YOUR_SECRET"
```

### Use Azure Key Vault (Production)

```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri("https://your-vault.vault.azure.net/"),
    new DefaultAzureCredential());
```

## 📊 Performance Comparison

| Mode | Latency | Throughput | Persistence | Distribution |
|------|---------|------------|-------------|--------------|
| **In-Memory** | ~1ms | Very High | ❌ No | ❌ No |
| **Kafka** | ~50-200ms | High | ✅ Yes | ✅ Yes |
| **Hybrid** | ~50-200ms* | Medium | ✅ Yes | ✅ Yes |

*Hybrid mode returns after both complete

## 🚀 Next Steps

1. **Start in In-Memory mode** for development
2. **Configure Kafka credentials** in `appsettings.Development.json`
3. **Test Kafka-only mode** to verify connection
4. **Enable Hybrid mode** to see messages in both systems
5. **Check Confluent Cloud console** to confirm messages appear

## 📚 Additional Resources

- [Confluent Cloud Console](https://confluent.cloud)
- [Kafka Documentation](https://docs.confluent.io)
- [API Documentation](http://localhost:8080/swagger)
- [Configuration Examples](/api/config/examples)

---

**Happy Queuing! 🎉**
