# Build and Deploy to Podman Desktop
# This script builds the Docker image and runs it on Podman

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Distributed Queue - Podman Setup  " -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check if Podman is installed
Write-Host "Checking Podman installation..." -ForegroundColor Yellow
try {
    $podmanVersion = podman --version
    Write-Host "✓ Podman found: $podmanVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ Podman not found!" -ForegroundColor Red
    Write-Host "Please install Podman Desktop from: https://podman-desktop.io/" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Stop and remove existing container if it exists
Write-Host "Cleaning up existing containers..." -ForegroundColor Yellow
podman stop distributed-queue-api 2>$null
podman rm distributed-queue-api 2>$null
Write-Host "✓ Cleanup complete" -ForegroundColor Green

Write-Host ""

# Build the Docker image
Write-Host "Building Docker image..." -ForegroundColor Yellow
Write-Host "This may take a few minutes..." -ForegroundColor Gray

# Change to parent directory for build context
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptPath
Set-Location $projectRoot

podman build -f docker/Dockerfile -t distributed-queue:latest .

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Docker build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Docker image built successfully!" -ForegroundColor Green
Write-Host ""

# Run the container
Write-Host "Starting container on Podman..." -ForegroundColor Yellow
podman run -d `
    --name distributed-queue-api `
    -p 8080:8080 `
    -e ASPNETCORE_ENVIRONMENT=Development `
    distributed-queue:latest

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Failed to start container!" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Container started successfully!" -ForegroundColor Green
Write-Host ""

# Wait a moment for the container to start
Write-Host "Waiting for API to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Check container status
Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Container Information              " -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
podman ps --filter name=distributed-queue-api

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Access Information                 " -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "API Base URL:    http://localhost:8080" -ForegroundColor Green
Write-Host "Swagger UI:      http://localhost:8080/swagger" -ForegroundColor Green
Write-Host "Health Check:    http://localhost:8080/api/topics" -ForegroundColor Green
Write-Host ""
Write-Host "Demo Scenario:   POST http://localhost:8080/api/demo/run-scenario" -ForegroundColor Yellow
Write-Host ""

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Useful Commands                    " -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "View logs:       podman logs -f distributed-queue-api" -ForegroundColor White
Write-Host "Stop container:  podman stop distributed-queue-api" -ForegroundColor White
Write-Host "Remove container: podman rm distributed-queue-api" -ForegroundColor White
Write-Host "View images:     podman images" -ForegroundColor White
Write-Host ""

# Try to open Swagger in browser
Write-Host "Opening Swagger UI in browser..." -ForegroundColor Yellow
Start-Sleep -Seconds 2
Start-Process "http://localhost:8080/swagger"

Write-Host ""
Write-Host "✓ Setup complete! Your API is running on Podman Desktop!" -ForegroundColor Green
Write-Host ""
