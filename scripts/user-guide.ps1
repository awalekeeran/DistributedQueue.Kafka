# User Guide Script - Interactive API Explorer
# Demonstrates all endpoints with examples

param(
    [string]$BaseUrl = "http://localhost:5297",
    [switch]$Podman
)

if ($Podman) {
    $BaseUrl = "http://localhost:8080"
}

function Show-Header {
    param([string]$Title)
    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Cyan
    Write-Host "  $Title" -ForegroundColor Cyan
    Write-Host "=========================================" -ForegroundColor Cyan
    Write-Host ""
}

function Show-Endpoint {
    param(
        [string]$Method,
        [string]$Endpoint,
        [string]$Description,
        [object]$Body = $null
    )
    
    Write-Host "[$Method] $Endpoint" -ForegroundColor Yellow
    Write-Host "Description: $Description" -ForegroundColor Gray
    
    if ($Body) {
        Write-Host "Body:" -ForegroundColor Gray
        Write-Host ($Body | ConvertTo-Json -Depth 5) -ForegroundColor White
    }
    
    Write-Host ""
}

function Show-Example {
    param(
        [string]$Title,
        [string]$Code
    )
    
    Write-Host "$Title" -ForegroundColor Green
    Write-Host $Code -ForegroundColor White
    Write-Host ""
}

Clear-Host

Show-Header "Distributed Queue API - User Guide"

Write-Host "Base URL: $BaseUrl" -ForegroundColor Cyan
Write-Host "Swagger UI: $BaseUrl/swagger" -ForegroundColor Cyan
Write-Host ""

Write-Host "This guide demonstrates all API endpoints." -ForegroundColor Gray
Write-Host "Copy the examples and paste into Swagger UI or PowerShell." -ForegroundColor Gray
Write-Host ""

# ============================================
# TOPICS
# ============================================
Show-Header "📋 Topics Controller"

Show-Endpoint -Method "GET" -Endpoint "/api/topics" `
    -Description "Get all topics from configured backends (In-Memory + Kafka)"

Show-Example "PowerShell Example:" @"
Invoke-WebRequest -Uri '$BaseUrl/api/topics' -Method GET
"@

Show-Example "Filter by source:" @"
# Get only in-memory topics
GET $BaseUrl/api/topics?source=inmemory

# Get only Kafka topics
GET $BaseUrl/api/topics?source=kafka
"@

Show-Endpoint -Method "POST" -Endpoint "/api/topics" `
    -Description "Create a new topic in configured backends" `
    -Body @{ topicName = "my-topic" }

Show-Example "PowerShell Example:" @"
`$body = @{ topicName = "my-topic" } | ConvertTo-Json
Invoke-WebRequest -Uri '$BaseUrl/api/topics' ``
    -Method POST -Body `$body -ContentType "application/json"
"@

Show-Endpoint -Method "DELETE" -Endpoint "/api/topics/{topicName}" `
    -Description "Delete a topic"

Show-Example "PowerShell Example:" @"
# Delete from all backends
Invoke-WebRequest -Uri '$BaseUrl/api/topics/my-topic' -Method DELETE

# Delete only from Kafka
Invoke-WebRequest -Uri '$BaseUrl/api/topics/my-topic?source=kafka' -Method DELETE
"@

# ============================================
# PRODUCERS
# ============================================
Show-Header "👥 Producers Controller"

Show-Endpoint -Method "GET" -Endpoint "/api/producers" `
    -Description "Get all producers (in-memory registered + Kafka broker endpoints)"

Show-Endpoint -Method "POST" -Endpoint "/api/producers" `
    -Description "Create a new producer" `
    -Body @{ 
        producerId = "producer1"
        name = "My Producer"
    }

Show-Example "PowerShell Example:" @"
`$body = @{
    producerId = "producer1"
    name = "My Producer"
} | ConvertTo-Json
Invoke-WebRequest -Uri '$BaseUrl/api/producers' ``
    -Method POST -Body `$body -ContentType "application/json"
"@

Show-Endpoint -Method "DELETE" -Endpoint "/api/producers/{producerId}" `
    -Description "Delete a producer"

# ============================================
# CONSUMERS
# ============================================
Show-Header "🎧 Consumers Controller"

Show-Endpoint -Method "GET" -Endpoint "/api/consumers" `
    -Description "Get all consumers (in-memory + Kafka consumer group members)"

Show-Endpoint -Method "POST" -Endpoint "/api/consumers" `
    -Description "Create a new consumer" `
    -Body @{
        consumerId = "consumer1"
        name = "My Consumer"
        consumerGroup = $null
    }

Show-Example "PowerShell Example:" @"
# Create consumer without group
`$body = @{
    consumerId = "consumer1"
    name = "My Consumer"
    consumerGroup = `$null
} | ConvertTo-Json
Invoke-WebRequest -Uri '$BaseUrl/api/consumers' ``
    -Method POST -Body `$body -ContentType "application/json"

# Create consumer with group
`$body = @{
    consumerId = "consumer2"
    name = "My Consumer 2"
    consumerGroup = "my-group"
} | ConvertTo-Json
Invoke-WebRequest -Uri '$BaseUrl/api/consumers' ``
    -Method POST -Body `$body -ContentType "application/json"
"@

Show-Endpoint -Method "POST" -Endpoint "/api/consumers/subscribe" `
    -Description "Subscribe a consumer to a topic" `
    -Body @{
        consumerId = "consumer1"
        topicName = "my-topic"
    }

Show-Example "PowerShell Example:" @"
`$body = @{
    consumerId = "consumer1"
    topicName = "my-topic"
} | ConvertTo-Json
Invoke-WebRequest -Uri '$BaseUrl/api/consumers/subscribe' ``
    -Method POST -Body `$body -ContentType "application/json"
"@

Show-Endpoint -Method "POST" -Endpoint "/api/consumers/{consumerId}/start" `
    -Description "Start a consumer (in-memory only)"

Show-Endpoint -Method "POST" -Endpoint "/api/consumers/{consumerId}/stop" `
    -Description "Stop a consumer (in-memory only)"

Show-Endpoint -Method "DELETE" -Endpoint "/api/consumers/{consumerId}" `
    -Description "Delete a consumer"

# ============================================
# MESSAGES
# ============================================
Show-Header "📨 Messages Controller"

Show-Endpoint -Method "POST" -Endpoint "/api/messages/publish" `
    -Description "Publish a message to configured backend(s)" `
    -Body @{
        producerId = "producer1"
        topicName = "my-topic"
        content = "Hello, World!"
    }

Show-Example "PowerShell Example:" @"
`$body = @{
    producerId = "producer1"
    topicName = "my-topic"
    content = "Hello, World!"
} | ConvertTo-Json
Invoke-WebRequest -Uri '$BaseUrl/api/messages/publish' ``
    -Method POST -Body `$body -ContentType "application/json"
"@

Show-Endpoint -Method "GET" -Endpoint "/api/messages/topic/{topicName}" `
    -Description "Get messages from a topic"

Show-Example "PowerShell Examples:" @"
# Get messages from Kafka (latest 10)
GET $BaseUrl/api/messages/topic/my-topic?source=kafka

# Get messages from Kafka (first 50 from beginning)
GET $BaseUrl/api/messages/topic/my-topic?source=kafka&offset=earliest&limit=50

# Get in-memory topic info
GET $BaseUrl/api/messages/topic/my-topic?source=inmemory

# PowerShell:
Invoke-WebRequest -Uri '$BaseUrl/api/messages/topic/my-topic?source=kafka&offset=earliest&limit=50' ``
    -Method GET | Select-Object -ExpandProperty Content | ConvertFrom-Json
"@

Show-Endpoint -Method "GET" -Endpoint "/api/messages/stats" `
    -Description "Get message statistics across all topics"

# ============================================
# CONSUMER GROUPS
# ============================================
Show-Header "👪 Consumer Groups Controller"

Show-Endpoint -Method "GET" -Endpoint "/api/consumergroups" `
    -Description "Get all consumer groups (in-memory + Kafka groups)"

Show-Endpoint -Method "POST" -Endpoint "/api/consumergroups" `
    -Description "Create a consumer group (in-memory only)" `
    -Body @{ groupId = "my-group" }

Show-Endpoint -Method "DELETE" -Endpoint "/api/consumergroups/{groupId}" `
    -Description "Delete a consumer group"

# ============================================
# DEMO
# ============================================
Show-Header "🧪 Demo Controller"

Show-Endpoint -Method "POST" -Endpoint "/api/demo/run-scenario" `
    -Description "Run a complete demo scenario (creates topics, producers, consumers, publishes messages)"

Show-Example "PowerShell Example:" @"
Invoke-WebRequest -Uri '$BaseUrl/api/demo/run-scenario' -Method POST
"@

Show-Endpoint -Method "POST" -Endpoint "/api/demo/cleanup" `
    -Description "Cleanup demo resources"

# ============================================
# KAFKA TEST
# ============================================
Show-Header "🔧 Kafka Test Controller"

Write-Host "Note: Only works when Kafka mode is enabled" -ForegroundColor Yellow
Write-Host ""

Show-Endpoint -Method "POST" -Endpoint "/api/kafkatest/test-connection" `
    -Description "Test Kafka connectivity"

Show-Endpoint -Method "POST" -Endpoint "/api/kafkatest/send-test-message" `
    -Description "Send a test message to Kafka" `
    -Body @{
        topic = "test-topic"
        message = "Test message content"
    }

Show-Endpoint -Method "GET" -Endpoint "/api/kafkatest/show-config" `
    -Description "Display current Kafka configuration"

# ============================================
# COMMON WORKFLOWS
# ============================================
Show-Header "🚀 Common Workflows"

Write-Host "Workflow 1: Quick Publish & Verify (Hybrid Mode)" -ForegroundColor Green
Write-Host @"
# 1. Create topic
POST /api/topics
Body: {"topicName": "notifications"}

# 2. Create producer
POST /api/producers
Body: {"producerId": "notifier", "name": "Notification Service"}

# 3. Publish message
POST /api/messages/publish
Body: {"producerId": "notifier", "topicName": "notifications", "content": "User registered"}

# 4. Verify in Kafka
GET /api/messages/topic/notifications?source=kafka&offset=earliest
"@ -ForegroundColor White
Write-Host ""

Write-Host "Workflow 2: In-Memory Consumer Processing" -ForegroundColor Green
Write-Host @"
# 1. Create consumer
POST /api/consumers
Body: {"consumerId": "processor", "name": "Message Processor"}

# 2. Subscribe to topic
POST /api/consumers/subscribe
Body: {"consumerId": "processor", "topicName": "notifications"}

# 3. Start processing
POST /api/consumers/processor/start

# 4. Check console logs for consumed messages
"@ -ForegroundColor White
Write-Host ""

Write-Host "Workflow 3: Check All Resources" -ForegroundColor Green
Write-Host @"
GET /api/topics
GET /api/producers
GET /api/consumers
GET /api/consumergroups
GET /api/messages/stats
"@ -ForegroundColor White
Write-Host ""

# ============================================
# FOOTER
# ============================================
Show-Header "📚 Additional Resources"

Write-Host "Complete API Reference:" -ForegroundColor Yellow
Write-Host "  docs/API_REFERENCE.md" -ForegroundColor White
Write-Host ""

Write-Host "Quick Reference Card:" -ForegroundColor Yellow
Write-Host "  docs/QUICK_API_REFERENCE.md" -ForegroundColor White
Write-Host ""

Write-Host "Test All Endpoints:" -ForegroundColor Yellow
Write-Host "  .\test-api.ps1" -ForegroundColor White
Write-Host "  .\test-api.ps1 -Verbose" -ForegroundColor White
Write-Host "  .\test-api.ps1 -LocalDev" -ForegroundColor White
Write-Host ""

Write-Host "Swagger UI:" -ForegroundColor Yellow
Write-Host "  $BaseUrl/swagger" -ForegroundColor White
Write-Host ""
