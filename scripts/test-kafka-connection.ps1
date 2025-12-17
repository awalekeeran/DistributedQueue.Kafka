# Kafka Connection Test Script
# Tests connection to Confluent Cloud and sends test messages

param(
    [string]$BaseUrl = "http://localhost:5297"
)

Write-Host "" -ForegroundColor Cyan
Write-Host "     Kafka Connection Test for Distributed Queue    " -ForegroundColor Cyan
Write-Host "" -ForegroundColor Cyan
Write-Host ""

# Test 1: Connection Test
Write-Host "1  Testing Kafka Connection..." -ForegroundColor Yellow
Write-Host "" -ForegroundColor Gray

try {
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/kafkatest/test-connection" -Method POST -ErrorAction Stop
    
    foreach ($line in $response.results) {
        if ($line -match "Error|Failed|FAILED") {
            Write-Host $line -ForegroundColor Red
        } elseif ($line -match "success|PASSED|Connected") {
            Write-Host $line -ForegroundColor Green
        } elseif ($line -match "Broker|Topic|Cluster") {
            Write-Host $line -ForegroundColor Cyan
        } elseif ($line -match "Common Issues|Troubleshooting") {
            Write-Host $line -ForegroundColor Yellow
        } else {
            Write-Host $line
        }
    }
    
    if ($response.success) {
        Write-Host ""
        Write-Host " Connection Test PASSED!" -ForegroundColor Green
        $connectionSuccess = $true
    } else {
        Write-Host ""
        Write-Host " Connection Test FAILED!" -ForegroundColor Red
        $connectionSuccess = $false
    }
}
catch {
    Write-Host " Error connecting to API: $_" -ForegroundColor Red
    $connectionSuccess = $false
}

Write-Host ""

if (-not $connectionSuccess) {
    Write-Host "  Cannot proceed without Kafka connection." -ForegroundColor Yellow
    Write-Host ""
    Write-Host " Troubleshooting Steps:" -ForegroundColor Cyan
    Write-Host "   1. Check if the API is running: $BaseUrl" -ForegroundColor White
    Write-Host "   2. Verify Kafka credentials in appsettings.Development.json" -ForegroundColor White
    Write-Host "   3. Check Confluent Cloud cluster status" -ForegroundColor White
    Write-Host "   4. Review docs\KAFKA_CONNECTION_TEST.md for detailed help" -ForegroundColor White
    Write-Host ""
    exit 1
}

# Test 2: Create Topic
Write-Host "2  Creating Test Topic..." -ForegroundColor Yellow
Write-Host "" -ForegroundColor Gray

try {
    $timestamp = Get-Date -Format "HHmmss"
    $topicName = "test-dotnet-$timestamp"
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/kafkatest/create-test-topic?topicName=$topicName" -Method POST -ErrorAction Stop
    
    foreach ($line in $response.results) {
        if ($line -match "Error|Failed|FAILED") {
            Write-Host $line -ForegroundColor Red
        } elseif ($line -match "success|created|OK") {
            Write-Host $line -ForegroundColor Green
        } elseif ($line -match "already exists") {
            Write-Host $line -ForegroundColor Yellow
        } else {
            Write-Host $line
        }
    }
    
    Write-Host ""
    if ($response.success -or ($response.results -like "*already exists*")) {
        Write-Host " Topic Test PASSED! Topic: $topicName" -ForegroundColor Green
        $topicSuccess = $true
    } else {
        Write-Host " Topic Test FAILED!" -ForegroundColor Red
        $topicSuccess = $false
    }
}
catch {
    Write-Host " Error creating topic: $_" -ForegroundColor Red
    $topicSuccess = $false
    $topicName = "test-dotnet"  # Fallback to default
}

Write-Host ""

# Test 3: Send Message
Write-Host "3  Sending Test Message..." -ForegroundColor Yellow
Write-Host "" -ForegroundColor Gray

try {
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $message = "Hello from PowerShell test at $timestamp"
    $encodedMessage = [System.Web.HttpUtility]::UrlEncode($message)
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/kafkatest/send-test-message?topicName=$topicName&message=$encodedMessage" -Method POST -ErrorAction Stop
    
    foreach ($line in $response.results) {
        if ($line -match "Error|Failed|FAILED") {
            Write-Host $line -ForegroundColor Red
        } elseif ($line -match "success|delivered") {
            Write-Host $line -ForegroundColor Green
        } else {
            Write-Host $line -ForegroundColor Cyan
        }
    }
    
    Write-Host ""
    if ($response.success) {
        Write-Host " Message Test PASSED!" -ForegroundColor Green
        Write-Host "   Topic: $($response.deliveryResult.topic)" -ForegroundColor White
        Write-Host "   Partition: $($response.deliveryResult.partition)" -ForegroundColor White
        Write-Host "   Offset: $($response.deliveryResult.offset)" -ForegroundColor White
        $messageSuccess = $true
    } else {
        Write-Host " Message Test FAILED!" -ForegroundColor Red
        $messageSuccess = $false
    }
}
catch {
    Write-Host " Error sending message: $_" -ForegroundColor Red
    $messageSuccess = $false
}

Write-Host ""
Write-Host "" -ForegroundColor Cyan
Write-Host "                     Test Summary                     " -ForegroundColor Cyan
Write-Host "" -ForegroundColor Cyan
Write-Host ""

$totalTests = 3
$passedTests = 0

if ($connectionSuccess) { $passedTests++ }
if ($topicSuccess) { $passedTests++ }
if ($messageSuccess) { $passedTests++ }

Write-Host "   Connection Test:  " -NoNewline
if ($connectionSuccess) { Write-Host " PASSED" -ForegroundColor Green } else { Write-Host " FAILED" -ForegroundColor Red }

Write-Host "   Topic Test:       " -NoNewline
if ($topicSuccess) { Write-Host " PASSED" -ForegroundColor Green } else { Write-Host " FAILED" -ForegroundColor Red }

Write-Host "   Message Test:     " -NoNewline
if ($messageSuccess) { Write-Host " PASSED" -ForegroundColor Green } else { Write-Host " FAILED" -ForegroundColor Red }

Write-Host ""
Write-Host "   Total: $passedTests / $totalTests tests passed" -ForegroundColor Cyan
Write-Host ""

if ($passedTests -eq $totalTests) {
    Write-Host " All tests PASSED! Your Kafka connection is working!" -ForegroundColor Green
    Write-Host ""
    Write-Host " Next Steps:" -ForegroundColor Cyan
    Write-Host "   1. Check Confluent Cloud console: https://confluent.cloud" -ForegroundColor White
    Write-Host "   2. Navigate to your cluster -> Topics -> $topicName" -ForegroundColor White
    Write-Host "   3. View the Messages tab to see your test message" -ForegroundColor White
    Write-Host "   4. Try the hybrid mode with: POST /api/messages/publish" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host "  Some tests failed. Please check the errors above." -ForegroundColor Yellow
    Write-Host ""
    Write-Host " For detailed troubleshooting, see:" -ForegroundColor Cyan
    Write-Host "   docs\KAFKA_CONNECTION_TEST.md" -ForegroundColor White
    Write-Host ""
}

Write-Host "" -ForegroundColor Cyan

