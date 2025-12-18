# API Reference - Complete Guide

## Overview

The Distributed Queue API supports three operation modes:
- **In-Memory**: All operations use in-memory data structures
- **Kafka**: All operations use Confluent Cloud Kafka
- **Hybrid**: Operations work with both in-memory and Kafka

Configure the mode in `appsettings.json` under `QueueModeSettings`.

---

## 📋 Topics Controller

Manage topics across in-memory and Kafka backends.

### GET `/api/topics`
Get all topics from configured backends.

**Query Parameters:**
- `source` (optional): Filter by source - `inmemory`, `kafka`, or omit for all

**Response (Hybrid Mode):**
```json
{
  "mode": "Hybrid",
  "inMemory": [
    {
      "name": "topic1",
      "messageCount": 5,
      "subscribedConsumers": ["consumer1", "consumer2"]
    }
  ],
  "kafka": [
    {
      "name": "com.wk.dev.notify-s.staff",
      "partitions": 3,
      "replicationFactor": 3
    }
  ],
  "combined": [...]
}
```

**Examples:**
```bash
# Get all topics
GET http://localhost:5297/api/topics

# Get only in-memory topics
GET http://localhost:5297/api/topics?source=inmemory

# Get only Kafka topics
GET http://localhost:5297/api/topics?source=kafka
```

### POST `/api/topics`
Create a new topic.

**Request Body:**
```json
{
  "topicName": "my-new-topic"
}
```

**Response:**
```json
{
  "name": "my-new-topic",
  "created": true,
  "createdIn": ["In-Memory", "Kafka"]
}
```

### DELETE `/api/topics/{topicName}`
Delete a topic.

**Query Parameters:**
- `source` (optional): Delete from specific source - `inmemory`, `kafka`, or omit for both

**Example:**
```bash
# Delete from both backends
DELETE http://localhost:5297/api/topics/my-topic

# Delete only from in-memory
DELETE http://localhost:5297/api/topics/my-topic?source=inmemory
```

---

## 👥 Producers Controller

Manage message producers.

### GET `/api/producers`
Get all producers (in-memory registered producers + Kafka broker endpoints).

**Response (Hybrid Mode):**
```json
{
  "mode": "Hybrid",
  "inMemory": [
    {
      "id": "producer1",
      "name": "My Producer",
      "createdAt": "2025-12-18T10:30:00Z"
    }
  ],
  "kafka": [
    {
      "brokerId": 0,
      "host": "pkc-56d1g.eastus.azure.confluent.cloud",
      "port": 9092
    }
  ],
  "combined": [...]
}
```

### POST `/api/producers`
Create a new producer.

**Request Body:**
```json
{
  "producerId": "producer1",
  "name": "My Producer"
}
```

### DELETE `/api/producers/{producerId}`
Delete a producer.

---

## 🎧 Consumers Controller

Manage message consumers.

### GET `/api/consumers`
Get all consumers (in-memory consumers + Kafka consumer group members).

**Response (Hybrid Mode):**
```json
{
  "mode": "Hybrid",
  "inMemory": [
    {
      "id": "consumer1",
      "name": "My Consumer",
      "subscribedTopics": ["topic1"],
      "isRunning": true,
      "messagesConsumed": 10
    }
  ],
  "kafka": [
    {
      "memberId": "consumer-abc123",
      "groupId": "my-group",
      "clientId": "client-1",
      "host": "10.0.0.5"
    }
  ],
  "combined": [...]
}
```

### POST `/api/consumers`
Create a new consumer (in-memory only).

**Request Body:**
```json
{
  "consumerId": "consumer1",
  "name": "My Consumer",
  "consumerGroup": null
}
```

**With Consumer Group:**
```json
{
  "consumerId": "consumer1",
  "name": "My Consumer",
  "consumerGroup": "my-group"
}
```

### POST `/api/consumers/subscribe`
Subscribe a consumer to a topic.

**Request Body:**
```json
{
  "consumerId": "consumer1",
  "topicName": "topic1"
}
```

### POST `/api/consumers/{consumerId}/start`
Start a consumer (in-memory only).

### POST `/api/consumers/{consumerId}/stop`
Stop a consumer (in-memory only).

### DELETE `/api/consumers/{consumerId}`
Delete a consumer.

---

## 👪 Consumer Groups Controller

Manage consumer groups.

### GET `/api/consumergroups`
Get all consumer groups (in-memory + Kafka consumer groups).

**Response (Hybrid Mode):**
```json
{
  "mode": "Hybrid",
  "inMemory": [
    {
      "groupId": "group1",
      "consumers": ["consumer1", "consumer2"],
      "consumerCount": 2
    }
  ],
  "kafka": [
    {
      "groupId": "my-kafka-group",
      "protocol": "consumer",
      "state": "Stable",
      "members": 3
    }
  ],
  "combined": [...]
}
```

### POST `/api/consumergroups`
Create a consumer group (in-memory only).

**Request Body:**
```json
{
  "groupId": "group1"
}
```

### DELETE `/api/consumergroups/{groupId}`
Delete a consumer group.

---

## 📨 Messages Controller

Publish and retrieve messages.

### POST `/api/messages/publish`
Publish a message to configured backend(s).

**Request Body:**
```json
{
  "producerId": "producer1",
  "topicName": "my-topic",
  "content": "Hello, World!"
}
```

**Response:**
```json
{
  "id": "msg-12345",
  "content": "Hello, World!",
  "topicName": "my-topic",
  "producerId": "producer1",
  "timestamp": "2025-12-18T10:30:00Z",
  "mode": "Hybrid",
  "publishedTo": ["In-Memory", "Kafka"],
  "status": "Published successfully"
}
```

### GET `/api/messages/topic/{topicName}`
Get messages from a topic.

**Query Parameters:**
- `source` (optional): `inmemory` or `kafka` - defaults to current mode
- `limit` (optional): Max messages to retrieve (default: 10)
- `offset` (optional): For Kafka - `earliest` or `latest` (default: `latest`)

**Examples:**
```bash
# Get latest 10 messages from Kafka
GET http://localhost:5297/api/messages/topic/com.wk.dev.notify-s.staff?source=kafka

# Get first 50 messages from Kafka
GET http://localhost:5297/api/messages/topic/com.wk.dev.notify-s.staff?source=kafka&offset=earliest&limit=50

# Get in-memory topic info
GET http://localhost:5297/api/messages/topic/topic1?source=inmemory
```

**Response (Kafka):**
```json
{
  "source": "Kafka",
  "topicName": "com.wk.dev.notify-s.staff",
  "messageCount": 3,
  "messages": [
    {
      "key": null,
      "value": "Message content",
      "partition": 0,
      "offset": 42,
      "timestamp": "2025-12-18T10:30:00Z"
    }
  ],
  "limit": 10,
  "offset": "latest"
}
```

**Note:** 
- Kafka messages are fetched using a temporary consumer (5-second timeout)
- For production use, create a dedicated consumer for continuous processing
- In-memory messages are consumed when retrieved

### GET `/api/messages/stats`
Get message statistics.

**Response:**
```json
{
  "mode": "Hybrid",
  "totalTopics": 5,
  "totalMessages": 42,
  "topicStats": [
    {
      "topicName": "topic1",
      "messageCount": 10
    }
  ]
}
```

---

## 🧪 Demo Controller

Run demo scenarios.

### POST `/api/demo/run-scenario`
Run a complete demo scenario.

**What it does:**
- Creates 2 topics: `demo-topic1`, `demo-topic2`
- Creates 2 producers: `demo-producer1`, `demo-producer2`
- In-memory mode: Creates 5 consumers, subscribes them to topics
- Publishes 5 messages using `IHybridMessageBroker`
- Works in all three modes (In-Memory, Kafka, Hybrid)

**Response:**
```json
{
  "success": true,
  "mode": "Hybrid",
  "steps": [
    "🚀 Running demo in Hybrid mode...",
    "✓ Created topics: demo-topic1, demo-topic2",
    "✓ Created producers: demo-producer1, demo-producer2",
    "✓ Created consumers: demo-consumer1 through demo-consumer5 (in-memory)",
    "✓ All 5 consumers subscribed to demo-topic1",
    "✓ Consumers 1, 3, and 4 subscribed to demo-topic2",
    "✓ Started all consumers",
    "✓ demo-producer1 published 'Demo Message 1' to demo-topic1",
    "...",
    "✅ Demo scenario completed successfully!",
    "📝 Check the console output for in-memory message consumption logs",
    "☁️ Messages published to Kafka topics. Use Confluent Cloud UI or Kafka consumers to view.",
    "🔄 Messages published to BOTH in-memory and Kafka!"
  ]
}
```

### POST `/api/demo/cleanup`
Clean up demo resources.

**Response:**
```json
{
  "success": true,
  "mode": "Hybrid",
  "results": [
    "✓ Stopped all consumers",
    "✓ Deleted 5 demo consumers",
    "✓ Deleted 2 demo producers",
    "✓ Deleted 2 demo topics (in-memory)",
    "✅ Demo resources cleaned up successfully!",
    "⚠️ Note: Kafka topics must be deleted manually via Confluent Cloud UI or Kafka admin tools"
  ]
}
```

---

## 🔧 Kafka Test Controller

Kafka-specific diagnostic endpoints (Kafka mode only).

### POST `/api/kafkatest/test-connection`
Test Kafka connectivity.

**Response:**
```json
{
  "success": true,
  "message": "Successfully connected to Kafka",
  "broker": "pkc-56d1g.eastus.azure.confluent.cloud:9092"
}
```

### POST `/api/kafkatest/send-test-message`
Send a test message to Kafka.

**Request Body:**
```json
{
  "topic": "test-topic",
  "message": "Test message content"
}
```

### GET `/api/kafkatest/show-config`
Display current Kafka configuration.

---

## 📊 Mode Information

All endpoints that return data include mode information:

```json
{
  "mode": "Hybrid"  // or "In-Memory" or "Kafka"
}
```

### Query Parameter: `source`
Many endpoints support the `?source=` parameter:
- `inmemory` or `in-memory` - Get data from in-memory backend only
- `kafka` - Get data from Kafka only
- Omit - Get data from all configured backends

---

## 🚀 Common Usage Patterns

### Pattern 1: Quick Message Publishing (Hybrid Mode)
```bash
# 1. Create a topic (creates in both backends)
POST /api/topics
{"topicName": "notifications"}

# 2. Create a producer
POST /api/producers
{"producerId": "notification-service", "name": "Notification Service"}

# 3. Publish a message (goes to both backends)
POST /api/messages/publish
{
  "producerId": "notification-service",
  "topicName": "notifications",
  "content": "New user registered"
}
```

### Pattern 2: Verify Kafka Messages
```bash
# Check if messages exist in Kafka
GET /api/messages/topic/notifications?source=kafka&offset=earliest&limit=100
```

### Pattern 3: In-Memory Consumer Processing
```bash
# 1. Create consumer
POST /api/consumers
{"consumerId": "processor1", "name": "Message Processor"}

# 2. Subscribe to topic
POST /api/consumers/subscribe
{"consumerId": "processor1", "topicName": "notifications"}

# 3. Start consumer
POST /api/consumers/processor1/start

# 4. Check what it consumed (check console logs)
```

### Pattern 4: Check All Resources in Hybrid Mode
```bash
# See everything from both backends
GET /api/topics
GET /api/producers
GET /api/consumers
GET /api/consumergroups
GET /api/messages/stats
```

---

## 🔍 Troubleshooting

### "Topic does not exist" when publishing
- Ensure topic is created via `POST /api/topics`
- In hybrid mode, topic can exist in either backend
- Check with `GET /api/topics?source=kafka` or `?source=inmemory`

### No messages returned from Kafka topic
- Check `offset` parameter (use `earliest` to read from beginning)
- Increase `limit` parameter
- Kafka consumer has 5-second timeout - may return empty if no recent messages
- Verify messages exist in Confluent Cloud UI

### Consumer not processing messages
- Ensure consumer is started: `POST /api/consumers/{id}/start`
- Check console logs for consumption output
- In-memory consumers only work in In-Memory or Hybrid mode
- Kafka consumers require separate Kafka consumer setup

---

## 📚 Additional Resources

- **Quick Reference**: See `QUICK_REF.md`
- **Mode Switching**: See `MODE_SWITCHING.md`
- **Kafka Setup**: See `KAFKA_CONNECTION_TEST.md`
- **Architecture**: See `COMPLETE_ARCHITECTURE_OVERVIEW.md`
