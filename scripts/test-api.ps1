# Comprehensive API Test Script
# Tests all endpoints with mode-aware operations

param(
    [string]$BaseUrl = "http://localhost:8080",
    [switch]$Verbose,
    [switch]$LocalDev
)

# Use localhost:5297 for local development
if ($LocalDev) {
    $BaseUrl = "http://localhost:5297"
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  Distributed Queue API - Full Test     " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Base URL: $BaseUrl" -ForegroundColor Gray
Write-Host ""

$ErrorActionPreference = "Continue"
$testsPassed = 0
$testsFailed = 0

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Uri,
        [object]$Body = $null,
        [switch]$ExpectSuccess = $true
    )
    
    Write-Host "Testing: $Name" -ForegroundColor Yellow -NoNewline
    
    try {
        $params = @{
            Uri = $Uri
            Method = $Method
            UseBasicParsing = $true
            ContentType = "application/json"
        }
        
        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
        }
        
        $response = Invoke-WebRequest @params
        
        if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 300) {
            Write-Host " ✓" -ForegroundColor Green
            
            if ($Verbose -and $response.Content) {
                $json = $response.Content | ConvertFrom-Json
                Write-Host ($json | ConvertTo-Json -Depth 5) -ForegroundColor Gray
            }
            
            $script:testsPassed++
            return $response.Content | ConvertFrom-Json
        } else {
            Write-Host " ✗ (Status: $($response.StatusCode))" -ForegroundColor Red
            $script:testsFailed++
            return $null
        }
    }
    catch {
        if (-not $ExpectSuccess) {
            Write-Host " ✓ (Expected failure)" -ForegroundColor Green
            $script:testsPassed++
        } else {
            Write-Host " ✗" -ForegroundColor Red
            if ($Verbose) {
                Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
            }
            $script:testsFailed++
        }
        return $null
    }
}

# Test 1: Health Check
Write-Host "`n--- Health Check ---" -ForegroundColor Cyan
Test-Endpoint -Name "Get all topics (initial)" -Method GET -Uri "$BaseUrl/api/topics"

# Test 2: Topics
Write-Host "`n--- Topics Tests ---" -ForegroundColor Cyan
Test-Endpoint -Name "Create topic 'test-topic1'" -Method POST -Uri "$BaseUrl/api/topics" `
    -Body @{ topicName = "test-topic1" }

Test-Endpoint -Name "Create topic 'test-topic2'" -Method POST -Uri "$BaseUrl/api/topics" `
    -Body @{ topicName = "test-topic2" }

Test-Endpoint -Name "Get all topics" -Method GET -Uri "$BaseUrl/api/topics"
Test-Endpoint -Name "Get in-memory topics" -Method GET -Uri "$BaseUrl/api/topics?source=inmemory"
Test-Endpoint -Name "Get Kafka topics" -Method GET -Uri "$BaseUrl/api/topics?source=kafka"

# Test 3: Producers
Write-Host "`n--- Producers Tests ---" -ForegroundColor Cyan
Test-Endpoint -Name "Create producer 'test-producer1'" -Method POST -Uri "$BaseUrl/api/producers" `
    -Body @{ producerId = "test-producer1"; name = "Test Producer 1" }

Test-Endpoint -Name "Create producer 'test-producer2'" -Method POST -Uri "$BaseUrl/api/producers" `
    -Body @{ producerId = "test-producer2"; name = "Test Producer 2" }

Test-Endpoint -Name "Get all producers" -Method GET -Uri "$BaseUrl/api/producers"

# Test 4: Consumers
Write-Host "`n--- Consumers Tests ---" -ForegroundColor Cyan
Test-Endpoint -Name "Create consumer 'test-consumer1'" -Method POST -Uri "$BaseUrl/api/consumers" `
    -Body @{ consumerId = "test-consumer1"; name = "Test Consumer 1"; consumerGroup = $null }

Test-Endpoint -Name "Create consumer 'test-consumer2'" -Method POST -Uri "$BaseUrl/api/consumers" `
    -Body @{ consumerId = "test-consumer2"; name = "Test Consumer 2"; consumerGroup = $null }

Test-Endpoint -Name "Create consumer with group" -Method POST -Uri "$BaseUrl/api/consumers" `
    -Body @{ consumerId = "test-consumer3"; name = "Test Consumer 3"; consumerGroup = "test-group" }

Test-Endpoint -Name "Subscribe consumer1 to topic1" -Method POST -Uri "$BaseUrl/api/consumers/subscribe" `
    -Body @{ consumerId = "test-consumer1"; topicName = "test-topic1" }

Test-Endpoint -Name "Subscribe consumer2 to topic1" -Method POST -Uri "$BaseUrl/api/consumers/subscribe" `
    -Body @{ consumerId = "test-consumer2"; topicName = "test-topic1" }

Test-Endpoint -Name "Get all consumers" -Method GET -Uri "$BaseUrl/api/consumers"

Test-Endpoint -Name "Start consumer1" -Method POST -Uri "$BaseUrl/api/consumers/test-consumer1/start"
Test-Endpoint -Name "Start consumer2" -Method POST -Uri "$BaseUrl/api/consumers/test-consumer2/start"

# Test 5: Consumer Groups
Write-Host "`n--- Consumer Groups Tests ---" -ForegroundColor Cyan
Test-Endpoint -Name "Get all consumer groups" -Method GET -Uri "$BaseUrl/api/consumergroups"

# Test 6: Messages
Write-Host "`n--- Messages Tests ---" -ForegroundColor Cyan
Test-Endpoint -Name "Publish message 1 to test-topic1" -Method POST -Uri "$BaseUrl/api/messages/publish" `
    -Body @{ producerId = "test-producer1"; topicName = "test-topic1"; content = "Test Message 1" }

Test-Endpoint -Name "Publish message 2 to test-topic1" -Method POST -Uri "$BaseUrl/api/messages/publish" `
    -Body @{ producerId = "test-producer1"; topicName = "test-topic1"; content = "Test Message 2" }

Test-Endpoint -Name "Publish message to test-topic2" -Method POST -Uri "$BaseUrl/api/messages/publish" `
    -Body @{ producerId = "test-producer2"; topicName = "test-topic2"; content = "Hello Topic 2" }

Start-Sleep -Seconds 1

Test-Endpoint -Name "Get message stats" -Method GET -Uri "$BaseUrl/api/messages/stats"
Test-Endpoint -Name "Get messages from topic1" -Method GET -Uri "$BaseUrl/api/messages/topic/test-topic1?source=inmemory"

# Test 7: Demo Scenario
Write-Host "`n--- Demo Scenario ---" -ForegroundColor Cyan
$demoResult = Test-Endpoint -Name "Run demo scenario" -Method POST -Uri "$BaseUrl/api/demo/run-scenario"

if ($demoResult -and $demoResult.steps) {
    Write-Host "`nDemo Steps:" -ForegroundColor Cyan
    foreach ($step in $demoResult.steps) {
        Write-Host "  $step" -ForegroundColor White
    }
}

Start-Sleep -Seconds 2

# Test 8: Cleanup Demo
Write-Host "`n--- Demo Cleanup ---" -ForegroundColor Cyan
$cleanupResult = Test-Endpoint -Name "Cleanup demo resources" -Method POST -Uri "$BaseUrl/api/demo/cleanup"

if ($cleanupResult -and $cleanupResult.results) {
    Write-Host "`nCleanup Results:" -ForegroundColor Cyan
    foreach ($result in $cleanupResult.results) {
        Write-Host "  $result" -ForegroundColor White
    }
}

# Test 9: Cleanup Test Resources
Write-Host "`n--- Test Cleanup ---" -ForegroundColor Cyan
Test-Endpoint -Name "Stop consumer1" -Method POST -Uri "$BaseUrl/api/consumers/test-consumer1/stop"
Test-Endpoint -Name "Stop consumer2" -Method POST -Uri "$BaseUrl/api/consumers/test-consumer2/stop"
Test-Endpoint -Name "Delete consumer1" -Method DELETE -Uri "$BaseUrl/api/consumers/test-consumer1"
Test-Endpoint -Name "Delete consumer2" -Method DELETE -Uri "$BaseUrl/api/consumers/test-consumer2"
Test-Endpoint -Name "Delete consumer3" -Method DELETE -Uri "$BaseUrl/api/consumers/test-consumer3"
Test-Endpoint -Name "Delete producer1" -Method DELETE -Uri "$BaseUrl/api/producers/test-producer1"
Test-Endpoint -Name "Delete producer2" -Method DELETE -Uri "$BaseUrl/api/producers/test-producer2"
Test-Endpoint -Name "Delete topic1 (in-memory)" -Method DELETE -Uri "$BaseUrl/api/topics/test-topic1?source=inmemory"
Test-Endpoint -Name "Delete topic2 (in-memory)" -Method DELETE -Uri "$BaseUrl/api/topics/test-topic2?source=inmemory"

# Summary
Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  Test Summary                           " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Tests Passed: $testsPassed" -ForegroundColor Green
Write-Host "Tests Failed: $testsFailed" -ForegroundColor $(if ($testsFailed -eq 0) { "Green" } else { "Red" })
Write-Host ""

if ($testsFailed -eq 0) {
    Write-Host "✓ All tests passed!" -ForegroundColor Green
} else {
    Write-Host "✗ Some tests failed. Run with -Verbose for details." -ForegroundColor Red
}

Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  • View Swagger UI: $BaseUrl/swagger" -ForegroundColor White
Write-Host "  • See API Reference: docs/API_REFERENCE.md" -ForegroundColor White
Write-Host "  • Run with -Verbose for detailed output" -ForegroundColor White
Write-Host "  • Run with -LocalDev to test localhost:5297" -ForegroundColor White
Write-Host ""
