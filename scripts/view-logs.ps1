# View Container Logs
# This script shows the logs from the running Podman container

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Container Logs                     " -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press Ctrl+C to stop following logs" -ForegroundColor Yellow
Write-Host ""

podman logs -f distributed-queue-api
