# Podman Desktop Deployment Guide

Complete guide to build and deploy the Distributed Queue System on Podman Desktop.

## 📋 Prerequisites

### 1. Install Podman Desktop
- Download from: https://podman-desktop.io/
- Install and start Podman Desktop
- Verify installation:
  ```powershell
  podman --version
  ```

### 2. Verify .NET SDK
```powershell
dotnet --version
```
Should show .NET 9.0 or later.

## 🚀 Quick Deploy (Automated)

### Option 1: One-Click Deployment
```powershell
.\deploy-podman.ps1
```

This script will:
1. ✅ Check Podman installation
2. ✅ Clean up existing containers
3. ✅ Build the Docker image
4. ✅ Start the container
5. ✅ Open Swagger UI in browser

### Option 2: Docker Compose
```powershell
podman-compose up -d
```

Or if using Docker Compose with Podman:
```powershell
docker-compose up -d
```

## 🔧 Manual Deployment

### Step 1: Build the Docker Image
```powershell
podman build -t distributed-queue:latest .
```

### Step 2: Run the Container
```powershell
podman run -d `
  --name distributed-queue-api `
  -p 8080:8080 `
  -e ASPNETCORE_ENVIRONMENT=Development `
  distributed-queue:latest
```

### Step 3: Verify Container is Running
```powershell
podman ps
```

You should see `distributed-queue-api` in the list.

## 🧪 Testing the Deployment

### Test 1: Using PowerShell Script
```powershell
.\test-api.ps1
```

### Test 2: Manual API Test
```powershell
# Check API is running
Invoke-WebRequest -Uri http://localhost:8080/api/topics -Method GET -UseBasicParsing

# Run demo scenario
Invoke-WebRequest -Uri http://localhost:8080/api/demo/run-scenario -Method POST -UseBasicParsing
```

### Test 3: Using Swagger UI
1. Open browser: http://localhost:8080/swagger
2. Navigate to **Demo** section
3. Execute `POST /api/demo/run-scenario`
4. Check logs to see message outputs

## 📊 Monitoring

### View Container Logs
```powershell
# Follow logs (live)
.\view-logs.ps1

# Or manually
podman logs -f distributed-queue-api

# View last 100 lines
podman logs --tail 100 distributed-queue-api
```

### Check Container Status
```powershell
# List running containers
podman ps

# Detailed container info
podman inspect distributed-queue-api

# Container stats (CPU, Memory, etc.)
podman stats distributed-queue-api
```

### Expected Log Output
When you run the demo scenario, you should see:
```
consumer1 received Message 1
consumer2 received Message 2
consumer3 received Message 3
consumer4 received Message 4
consumer5 received Message 5
consumer1 received Message 4
consumer3 received Message 5
```

## 🔄 Container Management

### Stop Container
```powershell
.\stop-podman.ps1

# Or manually
podman stop distributed-queue-api
```

### Restart Container
```powershell
podman restart distributed-queue-api
```

### Remove Container
```powershell
podman rm distributed-queue-api
```

### Remove Image
```powershell
podman rmi distributed-queue:latest
```

### Rebuild and Redeploy
```powershell
# Stop and remove existing
podman stop distributed-queue-api
podman rm distributed-queue-api

# Rebuild image
podman build -t distributed-queue:latest .

# Run new container
podman run -d --name distributed-queue-api -p 8080:8080 distributed-queue:latest
```

## 🌐 Access URLs

- **API Base URL**: http://localhost:8080
- **Swagger UI**: http://localhost:8080/swagger
- **Health Check**: http://localhost:8080/api/topics

## 🐛 Troubleshooting

### Problem: "Podman command not found"
**Solution**: 
- Install Podman Desktop from https://podman-desktop.io/
- Restart PowerShell after installation
- Verify with `podman --version`

### Problem: "Port 8080 already in use"
**Solution**:
```powershell
# Option 1: Use different port
podman run -d --name distributed-queue-api -p 9080:8080 distributed-queue:latest
# Access at: http://localhost:9080

# Option 2: Find and stop process using port 8080
netstat -ano | findstr :8080
# Kill the process using the PID
```

### Problem: "Container exits immediately"
**Solution**:
```powershell
# Check logs for error details
podman logs distributed-queue-api

# Run in interactive mode to see errors
podman run -it --rm -p 8080:8080 distributed-queue:latest
```

### Problem: "Cannot connect to Podman"
**Solution**:
- Open Podman Desktop application
- Ensure Podman machine is running
- Restart Podman machine if needed
- Check Podman Desktop settings

### Problem: "Build fails"
**Solution**:
```powershell
# Clean build
dotnet clean
dotnet build

# Check Dockerfile syntax
podman build --no-cache -t distributed-queue:latest .
```

### Problem: "API not responding"
**Solution**:
```powershell
# Check if container is running
podman ps

# Check logs
podman logs distributed-queue-api

# Restart container
podman restart distributed-queue-api
```

## 📦 Podman Desktop GUI

### Using Podman Desktop Application

1. **View Containers**:
   - Open Podman Desktop
   - Go to "Containers" section
   - Find `distributed-queue-api`

2. **View Logs**:
   - Click on the container
   - Go to "Logs" tab

3. **Manage Container**:
   - Start/Stop/Restart buttons
   - Delete container
   - Inspect details

4. **View Images**:
   - Go to "Images" section
   - Find `distributed-queue:latest`
   - Can pull, push, or delete images

## 🔐 Production Considerations

For production deployment:

### 1. Use Production Configuration
```powershell
podman run -d `
  --name distributed-queue-api `
  -p 8080:8080 `
  -e ASPNETCORE_ENVIRONMENT=Production `
  -e ASPNETCORE_URLS=http://+:8080 `
  distributed-queue:latest
```

### 2. Add Health Checks
Update Dockerfile:
```dockerfile
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
  CMD curl -f http://localhost:8080/api/topics || exit 1
```

### 3. Use Volume for Logs
```powershell
podman run -d `
  --name distributed-queue-api `
  -p 8080:8080 `
  -v ${PWD}/logs:/app/logs `
  distributed-queue:latest
```

### 4. Resource Limits
```powershell
podman run -d `
  --name distributed-queue-api `
  -p 8080:8080 `
  --memory="512m" `
  --cpus="1.0" `
  distributed-queue:latest
```

## 🚀 Advanced Scenarios

### Running Multiple Instances
```powershell
# Instance 1
podman run -d --name queue-api-1 -p 8081:8080 distributed-queue:latest

# Instance 2
podman run -d --name queue-api-2 -p 8082:8080 distributed-queue:latest

# Instance 3
podman run -d --name queue-api-3 -p 8083:8080 distributed-queue:latest
```

### Networking with Other Containers
```powershell
# Create network
podman network create distributed-queue-network

# Run container in network
podman run -d `
  --name distributed-queue-api `
  --network distributed-queue-network `
  -p 8080:8080 `
  distributed-queue:latest
```

### Save and Load Images
```powershell
# Save image to tar file
podman save -o distributed-queue.tar distributed-queue:latest

# Load image from tar file
podman load -i distributed-queue.tar
```

## 📝 Quick Reference

### Essential Commands
```powershell
# Deploy
.\deploy-podman.ps1

# Test
.\test-api.ps1

# View Logs
.\view-logs.ps1

# Stop
.\stop-podman.ps1

# Build
podman build -t distributed-queue:latest .

# Run
podman run -d --name distributed-queue-api -p 8080:8080 distributed-queue:latest

# Logs
podman logs -f distributed-queue-api

# Status
podman ps

# Stop
podman stop distributed-queue-api

# Remove
podman rm distributed-queue-api
```

## 🎯 Next Steps

1. ✅ Deploy to Podman Desktop
2. ✅ Test the demo scenario
3. ✅ Explore Swagger UI
4. ⏭️ Add persistence (database)
5. ⏭️ Integrate with Confluent Cloud Kafka
6. ⏭️ Create Web UI
7. ⏭️ Deploy to cloud (Azure, AWS, etc.)
