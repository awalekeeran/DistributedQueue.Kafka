# DistributedQueue - Quick Start Guide

## 🚀 Deploy to Podman Desktop (5 minutes)

### Prerequisites
- Install [Podman Desktop](https://podman-desktop.io/)
- Open Podman Desktop and start the Podman machine

### Step 1: Deploy the API

Open PowerShell and run:

```powershell
cd c:\Workshops\Designs\DistributedQueue
.\scripts\deploy-podman.ps1
```

That's it! The script will:
- ✅ Build the container image
- ✅ Start the API in In-Memory mode (no Kafka needed)
- ✅ Wait for the API to be ready
- ✅ Display access URLs

### Step 2: Access the API

Once deployed, open:
- **Swagger UI**: http://localhost:8080/swagger
- **API Health**: http://localhost:8080/health

### Step 3: Test the API

Use the included test script:

```powershell
.\scripts\test-api.ps1 -LocalDev -Verbose
```

Or test manually:

```powershell
# Create a topic
Invoke-RestMethod -Uri "http://localhost:8080/api/Topics/my-topic" -Method Post

# Publish a message
$body = @{ message = "Hello World"; priority = 1 } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:8080/api/Producers/my-topic/publish" -Method Post -Body $body -ContentType "application/json"

# Get messages
Invoke-RestMethod -Uri "http://localhost:8080/api/Messages/topic/my-topic"
```

## 🔄 Common Commands

### View Logs
```powershell
cd docker
docker-compose logs -f
```

### Stop the API
```powershell
.\scripts\deploy-podman.ps1 -Stop
```

### Rebuild and Restart
```powershell
.\scripts\deploy-podman.ps1 -Build
```

### Deploy with Kafka Mode
```powershell
.\scripts\deploy-podman.ps1 -Mode Kafka
# Then edit docker/.env with your Kafka credentials
cd docker
docker-compose restart
```

## 📚 Next Steps

- Read the full [Deployment Guide](DEPLOYMENT.md)
- Explore the [API Reference](docs/API_REFERENCE.md)
- Try the [PowerShell User Guide](scripts/user-guide.ps1)
- Deploy to [Azure Kubernetes Service](DEPLOYMENT.md#azure-kubernetes-service-aks-deployment)

## ⚡ Interactive Demo

Run all endpoints in a demo scenario:

```powershell
Invoke-RestMethod -Uri "http://localhost:8080/api/demo/run-scenario" -Method Post
```

This creates topics, producers, consumers, and publishes messages automatically!
