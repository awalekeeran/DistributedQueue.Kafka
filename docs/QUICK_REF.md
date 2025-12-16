# ⚡ Quick Reference

## 📁 Folder Structure
```
DistributedQueue/
├── src/       # Source code
├── scripts/   # PowerShell scripts ⭐
├── docker/    # Docker files
└── docs/      # Documentation
```

## 🚀 One-Liners

```powershell
# Deploy to Podman
scripts\deploy-podman.ps1

# Run locally
scripts\start.ps1

# Test API
scripts\test-api.ps1

# View logs
scripts\view-logs.ps1

# Stop container
scripts\stop-podman.ps1
```

## 🌐 URLs

**Podman:**
- API: http://localhost:8080
- Swagger: http://localhost:8080/swagger

**Local:**
- API: https://localhost:5001
- Swagger: https://localhost:5001/swagger

## 📚 Key Docs

- **[docs/START_HERE.md](docs/START_HERE.md)** - Start here! ⭐
- **[docs/DEPLOY_README.md](docs/DEPLOY_README.md)** - Quick deploy
- **[docs/PODMAN_DEPLOYMENT.md](docs/PODMAN_DEPLOYMENT.md)** - Full guide
- **[INDEX.md](INDEX.md)** - All documentation

## 🐳 Podman Commands

```powershell
# View containers
podman ps

# View logs
podman logs -f distributed-queue-api

# Restart
podman restart distributed-queue-api

# Stop
podman stop distributed-queue-api

# Remove
podman rm distributed-queue-api

# View images
podman images
```

## 🧪 Test Commands

```powershell
# Get all topics
Invoke-WebRequest -Uri http://localhost:8080/api/topics

# Run demo
Invoke-WebRequest -Uri http://localhost:8080/api/demo/run-scenario -Method POST

# Cleanup
Invoke-WebRequest -Uri http://localhost:8080/api/demo/cleanup -Method POST
```

## 🏗️ Build Commands

```powershell
# Build solution
dotnet build

# Clean and rebuild
dotnet clean; dotnet build

# Run tests (when added)
dotnet test

# Build Docker image
podman build -f docker/Dockerfile -t distributed-queue:latest .
```

## 📊 Project Info

- **Framework**: .NET 9.0
- **API**: ASP.NET Core Web API
- **Port (Podman)**: 8080
- **Port (Local)**: 5001

---

**Full index:** [INDEX.md](INDEX.md)
