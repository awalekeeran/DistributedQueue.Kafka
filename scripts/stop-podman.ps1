# Stop and Remove Container
# This script stops and removes the Podman container

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Stopping Container                 " -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Stopping container..." -ForegroundColor Yellow
podman stop distributed-queue-api

Write-Host "Removing container..." -ForegroundColor Yellow
podman rm distributed-queue-api

Write-Host ""
Write-Host "✓ Container stopped and removed!" -ForegroundColor Green
Write-Host ""

Write-Host "Container status:" -ForegroundColor Yellow
podman ps -a --filter name=distributed-queue-api
