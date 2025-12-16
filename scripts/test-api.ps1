# Test the Deployed API
# This script tests the API running in Podman

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Testing Distributed Queue API      " -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:8080"

# Test 1: Check if API is responding
Write-Host "1. Testing API health..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/topics" -Method GET -UseBasicParsing
    Write-Host "✓ API is responding (Status: $($response.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "✗ API is not responding!" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 2: Run demo scenario
Write-Host "2. Running demo scenario..." -ForegroundColor Yellow
Write-Host "This will create topics, producers, consumers, and publish messages" -ForegroundColor Gray

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/demo/run-scenario" -Method POST -UseBasicParsing
    $result = $response.Content | ConvertFrom-Json
    
    Write-Host "✓ Demo scenario completed!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Steps executed:" -ForegroundColor Cyan
    foreach ($step in $result.steps) {
        Write-Host "  $step" -ForegroundColor White
    }
} catch {
    Write-Host "✗ Demo scenario failed!" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "To see consumer message outputs:" -ForegroundColor Yellow
Write-Host "  Run: .\view-logs.ps1" -ForegroundColor White
Write-Host ""
Write-Host "To view in Swagger UI:" -ForegroundColor Yellow
Write-Host "  Open: http://localhost:8080/swagger" -ForegroundColor White
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Ask if user wants to view logs
$viewLogs = Read-Host "Do you want to view container logs now? (y/n)"
if ($viewLogs -eq 'y' -or $viewLogs -eq 'Y') {
    Write-Host ""
    Write-Host "Press Ctrl+C to stop following logs" -ForegroundColor Yellow
    Write-Host ""
    Start-Sleep -Seconds 2
    podman logs -f distributed-queue-api
}
