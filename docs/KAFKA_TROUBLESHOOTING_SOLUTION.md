# 🔧 Kafka Connection Troubleshooting - Complete Solution

## ✅ What We Added to Fix Kafka Connection Issues

### 1. **Enhanced Logging in KafkaProducerService**

Added comprehensive logging to see exactly what's happening:
- ✅ Configuration validation
- ✅ Connection attempts
- ✅ Error details with error codes
- ✅ Success confirmations
- ✅ Debug logs from Kafka client

**Location:** `src/DistributedQueue.Kafka/Producers/KafkaProducerService.cs`

**What You'll See:**
```
🔧 Initializing Kafka Producer...
   Bootstrap Servers: pkc-56d1g.eastus.azure.confluent.cloud:9092
   Security Protocol: SaslSsl
   SASL Mechanism: Plain
   SASL Username: 2LJ7FZRVNFZA4BU5
✅ Kafka Producer initialized successfully!

📤 Attempting to send message to Kafka topic 'my-topic'...
✅ Message delivered to my-topic [0] at offset 5
   Topic: my-topic
   Partition: 0
   Offset: 5
```

### 2. **Improved ProducerConfig with Timeouts**

Added production-ready Kafka settings:
- ✅ Extended timeouts (30 seconds)
- ✅ Retry configuration
- ✅ Idempotence enabled
- ✅ Debug logging enabled

**Location:** `src/DistributedQueue.Kafka/Configuration/KafkaSettings.cs`

**New Settings:**
```csharp
Acks = Acks.All,
EnableIdempotence = true,
MessageTimeoutMs = 30000,
RequestTimeoutMs = 30000,
MessageSendMaxRetries = 3,
RetryBackoffMs = 1000,
Debug = "broker,topic,msg"
```

### 3. **NEW Kafka Test Controller**

Created dedicated testing endpoints to diagnose connection issues!

**Location:** `src/DistributedQueue.Api/Controllers/KafkaTestController.cs`

**Endpoints:**

| Endpoint | Purpose |
|----------|---------|
| `POST /api/kafkatest/test-connection` | Test Kafka connection & list brokers/topics |
| `POST /api/kafkatest/create-test-topic` | Create a test topic in Confluent Cloud |
| `POST /api/kafkatest/send-test-message` | Send a test message to verify end-to-end |

### 4. **PowerShell Test Script**

Automated testing script to verify your Kafka connection!

**Location:** `scripts/test-kafka-connection.ps1`

**Run it:**
```powershell
cd c:\Workshops\Designs\DistributedQueue
.\scripts\test-kafka-connection.ps1
```

**What it does:**
1. ✅ Tests connection to Confluent Cloud
2. ✅ Lists available brokers and topics
3. ✅ Creates a test topic
4. ✅ Sends a test message
5. ✅ Shows summary of all tests

### 5. **Complete Documentation**

**Location:** `docs/KAFKA_CONNECTION_TEST.md`

Comprehensive guide covering:
- Step-by-step testing
- Common issues and fixes
- Debugging tips
- Verification steps

---

## 🚀 How to Use

### Step 1: Start the Application

```powershell
cd c:\Workshops\Designs\DistributedQueue\src\DistributedQueue.Api
dotnet run
```

**Watch for:**
```
🔧 ProducerConfig created:
   BootstrapServers: pkc-56d1g.eastus.azure.confluent.cloud:9092
   ...
🔧 Initializing Kafka Producer...
✅ Kafka Producer initialized successfully!
🚀 Queue System started in mode: Hybrid (In-Memory + Kafka)
Now listening on: http://localhost:XXXX
```

### Step 2: Run the Test Script

In a **NEW PowerShell window**:

```powershell
cd c:\Workshops\Designs\DistributedQueue
.\scripts\test-kafka-connection.ps1
```

**You'll see:**
```
═══════════════════════════════════════════════════════
    🧪 Kafka Connection Test for Distributed Queue    
═══════════════════════════════════════════════════════

1️⃣  Testing Kafka Connection...
───────────────────────────────────────────────────────
🔍 Testing Kafka Connection...
✅ Connected successfully!
   Cluster ID: 1
   Brokers: 3
📊 Broker Information:
   - Broker 1: pkc-xxxxx-1.eastus.azure.confluent.cloud:9092
   - Broker 2: pkc-xxxxx-2.eastus.azure.confluent.cloud:9092
   - Broker 3: pkc-xxxxx-3.eastus.azure.confluent.cloud:9092

2️⃣  Creating Test Topic...
───────────────────────────────────────────────────────
✅ Topic 'test-dotnet-143052' created successfully!

3️⃣  Sending Test Message...
───────────────────────────────────────────────────────
✅ Message delivered successfully!
   Topic: test-dotnet-143052
   Partition: 0
   Offset: 5

═══════════════════════════════════════════════════════
                    📊 Test Summary                     
═══════════════════════════════════════════════════════

   Connection Test:  ✅ PASSED
   Topic Test:       ✅ PASSED
   Message Test:     ✅ PASSED

   Total: 3 / 3 tests passed

🎉 All tests PASSED! Your Kafka connection is working!
```

### Step 3: Verify in Confluent Cloud

1. Go to https://confluent.cloud
2. Select your cluster
3. Click **Topics**
4. Find `test-dotnet-XXXXXX` (the topic created by the script)
5. Click **Messages** tab
6. **See your test message!** 🎉

### Step 4: Test Hybrid Mode

Now test publishing through your actual API:

```powershell
# Using curl or PowerShell
Invoke-RestMethod -Uri "http://localhost:5297/api/topics" -Method POST -ContentType "application/json" -Body '{"topicName":"my-hybrid-topic"}'

Invoke-RestMethod -Uri "http://localhost:5297/api/producers" -Method POST -ContentType "application/json" -Body '{"producerId":"producer1","name":"Test Producer"}'

Invoke-RestMethod -Uri "http://localhost:5297/api/messages/publish" -Method POST -ContentType "application/json" -Body '{"producerId":"producer1","topicName":"my-hybrid-topic","content":"Hello Hybrid World!"}'
```

**Check the console logs:**
```
📦 Publishing to IN-MEMORY queue: Topic=my-hybrid-topic, MessageId=abc123
📤 Attempting to send message to Kafka topic 'my-hybrid-topic'...
✅ Message delivered to my-hybrid-topic [0] at offset 1
   Topic: my-hybrid-topic
   Partition: 0
   Offset: 1
✅ HYBRID: Message published to BOTH in-memory and Kafka!
```

---

## 🐛 Troubleshooting

### Issue: "❌ Connection FAILED"

**Check these:**

1. **Bootstrap Servers URL**
   - Should be: `pkc-XXXXX.region.provider.confluent.cloud:9092`
   - Get it from: Confluent Cloud → Cluster → Cluster Settings → Bootstrap server

2. **API Credentials**
   - Go to: Confluent Cloud → Cluster → API Keys
   - Create new API key if needed
   - Update `appsettings.Development.json`:
   ```json
   {
     "KafkaSettings": {
       "SaslUsername": "YOUR_NEW_API_KEY",
       "SaslPassword": "YOUR_NEW_API_SECRET"
     }
   }
   ```

3. **Cluster Status**
   - Ensure cluster is **Running** (not Paused)
   - Check: Confluent Cloud → Environments → Your Cluster

4. **Network/Firewall**
   - Port 9092 must be accessible
   - Check corporate firewall/VPN
   - Try: `telnet pkc-xxxxx.eastus.azure.confluent.cloud 9092`

### Issue: "Broker: Topic does not exist"

**Solutions:**

1. **Auto-create topics** (recommended):
   - Confluent Cloud → Cluster → Cluster Settings
   - Enable "Auto create topics"

2. **Or create topic manually:**
   ```powershell
   Invoke-RestMethod -Uri "http://localhost:5297/api/kafkatest/create-test-topic?topicName=my-topic" -Method POST
   ```

### Issue: Messages not visible in Confluent Cloud

**Check:**

1. Correct topic name
2. Message retention settings
3. Wait a few seconds for UI to refresh
4. Check partition/offset in logs

---

## 📊 Quick Reference

### Test Endpoints

```bash
# Test connection
POST http://localhost:5297/api/kafkatest/test-connection

# Create topic
POST http://localhost:5297/api/kafkatest/create-test-topic?topicName=my-topic

# Send message
POST http://localhost:5297/api/kafkatest/send-test-message?topicName=my-topic&message=Hello

# Check config
GET http://localhost:5297/api/config/status

# Health check
GET http://localhost:5297/api/status
```

### Configuration Modes

```json
// In-Memory Only (no Kafka)
{ "UseInMemory": true, "UseKafka": false, "EnableHybridMode": false }

// Kafka Only (production)
{ "UseInMemory": false, "UseKafka": true, "EnableHybridMode": false }

// Hybrid (BOTH!)
{ "UseInMemory": true, "UseKafka": true, "EnableHybridMode": true }
```

---

## ✅ Success Checklist

- [ ] Application starts without errors
- [ ] Logs show "✅ Kafka Producer initialized successfully!"
- [ ] Test script shows all 3 tests passed
- [ ] Can see test topic in Confluent Cloud
- [ ] Can see test message in Confluent Cloud
- [ ] Hybrid mode publishing works
- [ ] Messages visible in both in-memory and Kafka

---

## 🎯 Summary

**We've added:**
1. ✅ Enhanced logging throughout Kafka integration
2. ✅ Production-ready Kafka configuration
3. ✅ Dedicated test endpoints (`/api/kafkatest/*`)
4. ✅ Automated PowerShell test script
5. ✅ Comprehensive troubleshooting documentation

**Now you can:**
- See exactly what's happening during Kafka connection
- Test connection without publishing real messages
- Diagnose issues quickly with detailed error messages
- Verify end-to-end flow with automated tests

---

**Run the test script now to verify your connection!**

```powershell
.\scripts\test-kafka-connection.ps1
```

**Then check Confluent Cloud to see your messages! 🎉**
