# 🎉 Podman Deployment - Ready to Go!

## ✅ What's Been Created

### Docker/Podman Files
- ✅ `Dockerfile` - Multi-stage Docker build configuration
- ✅ `.dockerignore` - Excludes unnecessary files from build
- ✅ `docker-compose.yml` - Compose configuration (optional)

### PowerShell Scripts
- ✅ `deploy-podman.ps1` - **Main deployment script** (use this!)
- ✅ `test-api.ps1` - Test the deployed API
- ✅ `view-logs.ps1` - View container logs
- ✅ `stop-podman.ps1` - Stop and remove container
- ✅ `start.ps1` - Run API locally (non-containerized)

### Documentation
- ✅ `PODMAN_DEPLOYMENT.md` - Comprehensive deployment guide
- ✅ `DEPLOY_README.md` - Quick start guide
- ✅ `README.md` - Project documentation
- ✅ `QUICKSTART.md` - Quick start for local development

## 🚀 Deployment Steps

### Step 1: Make Sure Podman is Installed
```powershell
podman --version
```

If not installed:
1. Download from https://podman-desktop.io/
2. Install Podman Desktop
3. Restart PowerShell

### Step 2: Deploy
```powershell
.\deploy-podman.ps1
```

### Step 3: Test
```powershell
.\test-api.ps1
```

### Step 4: View Logs
```powershell
.\view-logs.ps1
```

## 📊 What to Expect

### During Deployment
```
=====================================
  Distributed Queue - Podman Setup
=====================================

✓ Podman found: podman version 4.x.x
✓ Cleanup complete
Building Docker image...
✓ Docker image built successfully!
✓ Container started successfully!

=====================================
  Access Information
=====================================
API Base URL:    http://localhost:8080
Swagger UI:      http://localhost:8080/swagger
```

### During Testing
```
=====================================
  Testing Distributed Queue API
=====================================

1. Testing API health...
✓ API is responding (Status: 200)

2. Running demo scenario...
✓ Demo scenario completed!

Steps executed:
  ✓ Created topics: topic1, topic2
  ✓ Created producers: producer1, producer2
  ✓ Created consumers: consumer1-5
  ✓ All 5 consumers subscribed to topic1
  ...
```

### In Container Logs
```
consumer1 received Message 1
consumer2 received Message 2
consumer3 received Message 3
consumer4 received Message 4
consumer5 received Message 5
consumer1 received Message 4
consumer3 received Message 5
```

## 🎯 Container Details

### Image Information
- **Name**: `distributed-queue:latest`
- **Base Image**: `mcr.microsoft.com/dotnet/aspnet:9.0`
- **Size**: ~220 MB
- **Build Type**: Multi-stage (optimized)

### Container Configuration
- **Name**: `distributed-queue-api`
- **Port**: 8080 (host) → 8080 (container)
- **Environment**: Development
- **Restart Policy**: unless-stopped

## 🔧 Common Tasks

### Check Container Status
```powershell
podman ps
```

### View Logs
```powershell
podman logs -f distributed-queue-api
```

### Restart Container
```powershell
podman restart distributed-queue-api
```

### Stop Container
```powershell
podman stop distributed-queue-api
```

### Remove Everything
```powershell
# Stop and remove container
podman stop distributed-queue-api
podman rm distributed-queue-api

# Remove image
podman rmi distributed-queue:latest
```

### Rebuild and Redeploy
```powershell
.\deploy-podman.ps1
```
(Automatically cleans up and rebuilds)

## 🌐 Using the API

### Swagger UI
Open: http://localhost:8080/swagger

### API Endpoints
- `GET /api/topics` - List all topics
- `POST /api/topics` - Create topic
- `POST /api/producers` - Create producer
- `POST /api/consumers` - Create consumer
- `POST /api/messages/publish` - Publish message
- `POST /api/demo/run-scenario` - Run demo

### Example: Create Topic via PowerShell
```powershell
$body = @{
    topicName = "my-topic"
} | ConvertTo-Json

Invoke-WebRequest `
  -Uri http://localhost:8080/api/topics `
  -Method POST `
  -Body $body `
  -ContentType "application/json"
```

## 📦 Podman Desktop GUI

1. Open **Podman Desktop** application
2. Go to **Containers** section
3. Find `distributed-queue-api`
4. Click on it to see:
   - Logs
   - Terminal
   - Stats (CPU, Memory)
   - Inspect details

## 🎨 Architecture in Container

```
┌─────────────────────────────────┐
│   Podman Desktop (Host)         │
│                                 │
│  ┌───────────────────────────┐ │
│  │  Container                │ │
│  │  distributed-queue-api    │ │
│  │                           │ │
│  │  ┌─────────────────────┐ │ │
│  │  │  .NET 9.0 Runtime   │ │ │
│  │  │                     │ │ │
│  │  │  DistributedQueue   │ │ │
│  │  │  API                │ │ │
│  │  │                     │ │ │
│  │  │  Port: 8080         │ │ │
│  │  └─────────────────────┘ │ │
│  └───────────────────────────┘ │
│         ↓                       │
│    localhost:8080               │
└─────────────────────────────────┘
```

## 🎉 Success Indicators

✅ Build completes without errors
✅ Container starts and stays running
✅ Swagger UI loads at http://localhost:8080/swagger
✅ Demo scenario runs successfully
✅ Consumer messages appear in logs

## 🔍 Verification Checklist

Run these commands to verify everything is working:

```powershell
# 1. Check Podman is installed
podman --version

# 2. Check image exists
podman images | findstr distributed-queue

# 3. Check container is running
podman ps | findstr distributed-queue-api

# 4. Test API
Invoke-WebRequest -Uri http://localhost:8080/api/topics -UseBasicParsing

# 5. View logs
podman logs distributed-queue-api --tail 20
```

All should return success! ✅

## 📚 Additional Resources

- **Podman Documentation**: https://docs.podman.io/
- **Podman Desktop**: https://podman-desktop.io/docs/intro
- **.NET on Docker**: https://learn.microsoft.com/dotnet/core/docker/
- **ASP.NET Core**: https://learn.microsoft.com/aspnet/core/

## 🎯 Next Steps

1. ✅ Deploy to Podman
2. ✅ Test the demo scenario
3. ⏭️ Explore the API via Swagger
4. ⏭️ Build a web UI
5. ⏭️ Integrate with Confluent Kafka
6. ⏭️ Deploy to cloud

---

**Everything is ready! Run `.\deploy-podman.ps1` to get started! 🚀**
