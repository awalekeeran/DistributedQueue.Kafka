# 📖 Documentation Index

Welcome to the Distributed Queue System documentation!

## 🎯 Choose Your Path

### New to the Project?
Start here: **[docs/START_HERE.md](docs/START_HERE.md)** ⭐

### Want to Deploy?
- **Quick Deploy**: [docs/DEPLOY_README.md](docs/DEPLOY_README.md)
- **Full Deployment Guide**: [docs/PODMAN_DEPLOYMENT.md](docs/PODMAN_DEPLOYMENT.md)
- **Deployment Checklist**: [docs/DEPLOYMENT_CHECKLIST.md](docs/DEPLOYMENT_CHECKLIST.md)
- **Deployment Flow**: [docs/DEPLOYMENT_FLOW.md](docs/DEPLOYMENT_FLOW.md)

### Want to Develop Locally?
- **Quick Start**: [docs/QUICKSTART.md](docs/QUICKSTART.md)
- **API Examples**: [docs/API_EXAMPLES.md](docs/API_EXAMPLES.md)

### Want to Understand the Project?
- **Project Summary**: [docs/PROJECT_SUMMARY.md](docs/PROJECT_SUMMARY.md)
- **Architecture**: [docs/STRUCTURE.md](docs/STRUCTURE.md)
- **Problem Statement**: [ProblemStatement.md](ProblemStatement.md)

### Want to Extend the System?
- **Extension Guide**: [docs/EXTENSION_GUIDE.md](docs/EXTENSION_GUIDE.md)

## 📁 Folder Structure

```
DistributedQueue/
├── 📁 src/              # Source code (C# projects)
├── 📁 scripts/          # PowerShell scripts
├── 📁 docker/           # Docker/Podman files
├── 📁 docs/             # All documentation
├── 📄 README.md         # Main readme
└── 📄 INDEX.md          # This file
```

## 🚀 Quick Commands

### Deploy to Podman
```powershell
cd scripts
.\deploy-podman.ps1
```

### Run Locally
```powershell
cd scripts
.\start.ps1
```

### Test API
```powershell
cd scripts
.\test-api.ps1
```

### View Logs
```powershell
cd scripts
.\view-logs.ps1
```

### Stop Container
```powershell
cd scripts
.\stop-podman.ps1
```

## 📚 All Documentation Files

### Getting Started
- [START_HERE.md](docs/START_HERE.md) - Quick start guide ⭐
- [README.md](README.md) - Main project documentation
- [QUICKSTART.md](docs/QUICKSTART.md) - Local development guide

### Deployment
- [DEPLOY_README.md](docs/DEPLOY_README.md) - Quick deployment guide
- [PODMAN_DEPLOYMENT.md](docs/PODMAN_DEPLOYMENT.md) - Complete Podman guide
- [DEPLOYMENT_CHECKLIST.md](docs/DEPLOYMENT_CHECKLIST.md) - Verification checklist
- [DEPLOYMENT_FLOW.md](docs/DEPLOYMENT_FLOW.md) - Visual flow diagrams

### Usage & Examples
- [API_EXAMPLES.md](docs/API_EXAMPLES.md) - API usage examples
- [ProblemStatement.md](ProblemStatement.md) - Original requirements

### Architecture & Extension
- [PROJECT_SUMMARY.md](docs/PROJECT_SUMMARY.md) - Project overview
- [STRUCTURE.md](docs/STRUCTURE.md) - Architecture details
- [EXTENSION_GUIDE.md](docs/EXTENSION_GUIDE.md) - How to extend

## 🔧 Scripts Reference

All scripts are in the `scripts/` folder:

| Script | Purpose |
|--------|---------|
| `deploy-podman.ps1` | Deploy to Podman Desktop |
| `start.ps1` | Run API locally (development) |
| `test-api.ps1` | Test the deployed/running API |
| `view-logs.ps1` | View container logs in real-time |
| `stop-podman.ps1` | Stop and remove Podman container |

## 🐳 Docker Files

All Docker-related files are in the `docker/` folder:

| File | Purpose |
|------|---------|
| `Dockerfile` | Multi-stage Docker build configuration |
| `docker-compose.yml` | Docker Compose configuration |
| `.dockerignore` | Files to exclude from build context |

## 🌐 Access Points

### Podman Deployment
- **API Base**: http://localhost:8080
- **Swagger UI**: http://localhost:8080/swagger

### Local Development
- **API Base**: https://localhost:5001
- **Swagger UI**: https://localhost:5001/swagger

## 🎯 Common Tasks

### First Time Setup
1. Read [docs/START_HERE.md](docs/START_HERE.md)
2. Run `cd scripts` then `.\deploy-podman.ps1`
3. Test with `.\test-api.ps1`

### Daily Development
1. Make code changes
2. Run `cd scripts` then `.\start.ps1` for local testing
3. Deploy to Podman with `.\deploy-podman.ps1`

### Troubleshooting
1. Check [docs/PODMAN_DEPLOYMENT.md](docs/PODMAN_DEPLOYMENT.md) - Troubleshooting section
2. View logs with `cd scripts` then `.\view-logs.ps1`
3. Verify with [docs/DEPLOYMENT_CHECKLIST.md](docs/DEPLOYMENT_CHECKLIST.md)

---

**Need help? Start with [docs/START_HERE.md](docs/START_HERE.md)** ⭐
