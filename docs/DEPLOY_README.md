# 🐳 Deploy to Podman Desktop - Quick Guide

## 🚀 One Command Deploy

```powershell
.\deploy-podman.ps1
```

That's it! The script will:
- ✅ Build the Docker image
- ✅ Start the container on Podman
- ✅ Open Swagger UI in your browser

## 📋 What You Need

1. **Podman Desktop** - [Download here](https://podman-desktop.io/)
2. **.NET 9.0 SDK** - Already have it!

## 🎯 Access Your Application

Once deployed, access:
- **Swagger UI**: http://localhost:8080/swagger
- **API**: http://localhost:8080

## 🧪 Test the Demo

### Option 1: Run Test Script
```powershell
.\test-api.ps1
```

### Option 2: Use Swagger UI
1. Open http://localhost:8080/swagger
2. Find **Demo** section
3. Execute `POST /api/demo/run-scenario`

### Option 3: PowerShell Command
```powershell
Invoke-WebRequest -Uri http://localhost:8080/api/demo/run-scenario -Method POST -UseBasicParsing
```

## 👀 View Logs

To see consumer messages in real-time:
```powershell
.\view-logs.ps1
```

You'll see output like:
```
consumer1 received Message 1
consumer2 received Message 2
consumer3 received Message 3
...
```

## 🛑 Stop the Container

```powershell
.\stop-podman.ps1
```

## 🔄 Redeploy After Changes

Made code changes? Redeploy:
```powershell
.\deploy-podman.ps1
```

The script automatically cleans up and rebuilds.

## 📊 Podman Desktop GUI

Open Podman Desktop app to:
- View running containers
- Check logs
- Monitor resource usage
- Manage containers visually

## 🆘 Troubleshooting

### Container won't start?
```powershell
# Check logs
podman logs distributed-queue-api

# View all containers
podman ps -a
```

### Port already in use?
```powershell
# Stop other services using port 8080
# Or edit deploy-podman.ps1 to use different port
# Change: -p 8080:8080 to -p 9080:8080
```

### Need to rebuild?
```powershell
# Clean rebuild
podman rmi distributed-queue:latest
.\deploy-podman.ps1
```

## 📚 More Help

See `PODMAN_DEPLOYMENT.md` for detailed documentation.

---

**That's all you need! Happy coding! 🎉**
