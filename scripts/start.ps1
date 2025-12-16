# Distributed Queue System - Quick Start Script
# This script will build and run the API, then open Swagger in your browser

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Distributed Queue System Setup    " -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check if .NET SDK is installed
Write-Host "Checking .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ .NET SDK not found! Please install .NET 9.0 SDK or later." -ForegroundColor Red
    exit 1
}

# Change to project root directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptPath
Set-Location $projectRoot

Write-Host ""
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Build successful!" -ForegroundColor Green
Write-Host ""

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Starting API Server...             " -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "The API will start on https://localhost:5001" -ForegroundColor Yellow
Write-Host "Swagger UI will be available at https://localhost:5001/swagger" -ForegroundColor Yellow
Write-Host ""
Write-Host "To test the demo scenario:" -ForegroundColor Cyan
Write-Host "  1. Open Swagger UI in your browser" -ForegroundColor White
Write-Host "  2. Navigate to the Demo section" -ForegroundColor White
Write-Host "  3. Execute POST /api/demo/run-scenario" -ForegroundColor White
Write-Host "  4. Watch this console for message outputs!" -ForegroundColor White
Write-Host ""
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow
Write-Host ""

# Wait a moment
Start-Sleep -Seconds 2

# Start the API (this will block)
dotnet run --project src/DistributedQueue.Api/DistributedQueue.Api.csproj
