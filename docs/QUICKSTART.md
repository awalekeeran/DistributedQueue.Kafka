# Quick Start Guide

## 🚀 Run the Application

1. **Navigate to the project directory:**
   ```bash
   cd c:\Workshops\Designs\DistributedQueue
   ```

2. **Run the API:**
   ```bash
   dotnet run --project src/DistributedQueue.Api/DistributedQueue.Api.csproj
   ```

3. **Open Swagger UI:**
   - Navigate to: `https://localhost:5001/swagger` (or the port shown in console)

## 🎯 Run the Demo Scenario

### Using Swagger UI:
1. Open Swagger UI
2. Find the **Demo** section
3. Click on `POST /api/demo/run-scenario`
4. Click **Try it out**
5. Click **Execute**
6. **Check your console** where the API is running to see output like:
   ```
   consumer1 received Message 1
   consumer2 received Message 2
   consumer3 received Message 3
   consumer4 received Message 4
   consumer5 received Message 5
   ...
   ```

### Using curl:
```bash
curl -X POST https://localhost:5001/api/demo/run-scenario -k
```

### Using PowerShell:
```powershell
Invoke-WebRequest -Uri https://localhost:5001/api/demo/run-scenario -Method POST -SkipCertificateCheck
```

## 📝 Manual Testing

### 1. Create a Topic
```json
POST /api/topics
{
  "topicName": "test-topic"
}
```

### 2. Create a Producer
```json
POST /api/producers
{
  "producerId": "test-producer",
  "name": "Test Producer"
}
```

### 3. Create a Consumer
```json
POST /api/consumers
{
  "consumerId": "test-consumer",
  "name": "Test Consumer",
  "consumerGroup": null
}
```

### 4. Subscribe Consumer to Topic
```json
POST /api/consumers/subscribe
{
  "consumerId": "test-consumer",
  "topicName": "test-topic"
}
```

### 5. Start the Consumer
```
POST /api/consumers/test-consumer/start
```

### 6. Publish a Message
```json
POST /api/messages/publish
{
  "producerId": "test-producer",
  "topicName": "test-topic",
  "content": "Hello from distributed queue!"
}
```

### 7. Check Console Output
You should see:
```
test-consumer received Hello from distributed queue!
```

## 🧹 Cleanup

After testing, clean up all resources:

```
POST /api/demo/cleanup
```

This will stop all consumers and delete all producers, consumers, and topics.

## 💡 Tips

- **Console Output**: Messages are printed to the console where the API is running
- **Multiple Consumers**: Create multiple consumers subscribing to the same topic to see parallel consumption
- **Consumer Groups**: Set `consumerGroup` when creating consumers to enable round-robin message distribution
- **Swagger UI**: Best tool for exploring and testing all API endpoints interactively

## 🔧 Troubleshooting

**Problem**: Not seeing consumer messages in console  
**Solution**: Make sure you've started the consumer with `POST /api/consumers/{consumerId}/start`

**Problem**: "Topic does not exist" error  
**Solution**: Create the topic first using `POST /api/topics`

**Problem**: "Producer does not exist" error  
**Solution**: Create the producer first using `POST /api/producers`

## 🎨 Next Steps

1. Try creating multiple consumers and see how messages are distributed
2. Experiment with consumer groups
3. Subscribe a single consumer to multiple topics
4. Test with multiple producers publishing to the same topic
5. Explore the Kafka integration library for future Confluent Cloud integration
