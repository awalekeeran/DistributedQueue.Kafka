# API Examples

## Sample Request Bodies

Copy and paste these into Swagger UI or your API testing tool.

### Create Topics
```json
{
  "topicName": "topic1"
}
```

```json
{
  "topicName": "topic2"
}
```

### Create Producers
```json
{
  "producerId": "producer1",
  "name": "Producer 1"
}
```

```json
{
  "producerId": "producer2",
  "name": "Producer 2"
}
```

### Create Consumers (No Consumer Group)
```json
{
  "consumerId": "consumer1",
  "name": "Consumer 1",
  "consumerGroup": null
}
```

```json
{
  "consumerId": "consumer2",
  "name": "Consumer 2",
  "consumerGroup": null
}
```

```json
{
  "consumerId": "consumer3",
  "name": "Consumer 3",
  "consumerGroup": null
}
```

```json
{
  "consumerId": "consumer4",
  "name": "Consumer 4",
  "consumerGroup": null
}
```

```json
{
  "consumerId": "consumer5",
  "name": "Consumer 5",
  "consumerGroup": null
}
```

### Create Consumers (With Consumer Group)
```json
{
  "consumerId": "group1-consumer1",
  "name": "Group 1 Consumer 1",
  "consumerGroup": "group1"
}
```

```json
{
  "consumerId": "group1-consumer2",
  "name": "Group 1 Consumer 2",
  "consumerGroup": "group1"
}
```

### Subscribe Consumers to Topics
Subscribe all 5 consumers to topic1:
```json
{"consumerId": "consumer1", "topicName": "topic1"}
```
```json
{"consumerId": "consumer2", "topicName": "topic1"}
```
```json
{"consumerId": "consumer3", "topicName": "topic1"}
```
```json
{"consumerId": "consumer4", "topicName": "topic1"}
```
```json
{"consumerId": "consumer5", "topicName": "topic1"}
```

Subscribe consumers 1, 3, and 4 to topic2:
```json
{"consumerId": "consumer1", "topicName": "topic2"}
```
```json
{"consumerId": "consumer3", "topicName": "topic2"}
```
```json
{"consumerId": "consumer4", "topicName": "topic2"}
```

### Publish Messages
```json
{
  "producerId": "producer1",
  "topicName": "topic1",
  "content": "Message 1"
}
```

```json
{
  "producerId": "producer1",
  "topicName": "topic1",
  "content": "Message 2"
}
```

```json
{
  "producerId": "producer2",
  "topicName": "topic1",
  "content": "Message 3"
}
```

```json
{
  "producerId": "producer1",
  "topicName": "topic2",
  "content": "Message 4"
}
```

```json
{
  "producerId": "producer2",
  "topicName": "topic2",
  "content": "Message 5"
}
```

## PowerShell Script

Save this as `test-api.ps1`:

```powershell
$baseUrl = "https://localhost:5001"

# Run the demo scenario
Write-Host "Running demo scenario..." -ForegroundColor Green
Invoke-WebRequest -Uri "$baseUrl/api/demo/run-scenario" -Method POST -SkipCertificateCheck

Write-Host "`nDemo completed! Check the console for message outputs." -ForegroundColor Green
Write-Host "`nPress any key to cleanup..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# Cleanup
Write-Host "`nCleaning up..." -ForegroundColor Green
Invoke-WebRequest -Uri "$baseUrl/api/demo/cleanup" -Method POST -SkipCertificateCheck

Write-Host "Cleanup completed!" -ForegroundColor Green
```

## Bash Script

Save this as `test-api.sh`:

```bash
#!/bin/bash

BASE_URL="https://localhost:5001"

echo "Running demo scenario..."
curl -X POST "$BASE_URL/api/demo/run-scenario" -k

echo -e "\n\nDemo completed! Check the console for message outputs."
echo "Press any key to cleanup..."
read -n 1 -s

echo -e "\nCleaning up..."
curl -X POST "$BASE_URL/api/demo/cleanup" -k

echo -e "\nCleanup completed!"
```

## Expected Console Output

When you run the demo, you should see output similar to:

```
consumer1 received Message 1
consumer2 received Message 2
consumer3 received Message 3
consumer4 received Message 4
consumer5 received Message 5
consumer1 received Message 4
consumer3 received Message 5
```

Note: The exact order may vary due to concurrent processing.
