# 📁 Organized Folder Structure

## ✨ New Clean Organization

All files are now organized into logical folders!

```
DistributedQueue/
│
├── 📁 src/                              # SOURCE CODE
│   ├── DistributedQueue.Core/           # Core library
│   │   ├── Models/                      # Domain models
│   │   │   ├── Message.cs
│   │   │   ├── Topic.cs
│   │   │   ├── Producer.cs
│   │   │   ├── Consumer.cs
│   │   │   └── ConsumerGroup.cs
│   │   └── Services/                    # Business services
│   │       ├── MessageBroker.cs
│   │       ├── TopicManager.cs
│   │       ├── ProducerManager.cs
│   │       ├── ConsumerManager.cs
│   │       └── ConsumerGroupManager.cs
│   │
│   ├── DistributedQueue.Api/            # Web API
│   │   ├── Controllers/                 # API controllers
│   │   │   ├── TopicsController.cs
│   │   │   ├── ProducersController.cs
│   │   │   ├── ConsumersController.cs
│   │   │   ├── MessagesController.cs
│   │   │   ├── ConsumerGroupsController.cs
│   │   │   └── DemoController.cs
│   │   ├── DTOs/
│   │   │   └── Requests.cs
│   │   ├── Program.cs
│   │   └── appsettings*.json
│   │
│   └── DistributedQueue.Kafka/          # Kafka integration
│       ├── Producers/
│       │   └── KafkaProducerService.cs
│       ├── Consumers/
│       │   └── KafkaConsumerService.cs
│       └── Configuration/
│           └── KafkaSettings.cs
│
├── 📁 scripts/                          # POWERSHELL SCRIPTS ⭐
│   ├── deploy-podman.ps1                # 🐳 Deploy to Podman
│   ├── start.ps1                        # 💻 Run locally
│   ├── test-api.ps1                     # 🧪 Test API
│   ├── view-logs.ps1                    # 📊 View logs
│   └── stop-podman.ps1                  # 🛑 Stop container
│
├── 📁 docker/                           # DOCKER FILES
│   ├── Dockerfile                       # Multi-stage build
│   ├── docker-compose.yml               # Compose config
│   └── .dockerignore                    # Build exclusions
│
├── 📁 docs/                             # DOCUMENTATION
│   ├── START_HERE.md                    # ⭐ Quick start guide
│   ├── DEPLOY_README.md                 # Quick deployment
│   ├── PODMAN_DEPLOYMENT.md             # Full Podman guide
│   ├── DEPLOYMENT_CHECKLIST.md          # Verification checklist
│   ├── DEPLOYMENT_FLOW.md               # Visual diagrams
│   ├── QUICKSTART.md                    # Local dev guide
│   ├── API_EXAMPLES.md                  # API usage
│   ├── PROJECT_SUMMARY.md               # Project overview
│   ├── STRUCTURE.md                     # Architecture
│   └── EXTENSION_GUIDE.md               # Extension guide
│
├── 📄 README.md                         # Main readme
├── 📄 INDEX.md                          # Documentation index
├── 📄 QUICK_REF.md                      # Quick reference
├── 📄 ProblemStatement.md               # Requirements
├── 📄 .gitignore                        # Git ignore
└── 📄 DistributedQueue.sln              # Solution file
```

## 🎯 Quick Access

### I want to...

**Deploy to Podman:**
```powershell
cd scripts
.\deploy-podman.ps1
```

**Run locally:**
```powershell
cd scripts
.\start.ps1
```

**Test the API:**
```powershell
cd scripts
.\test-api.ps1
```

**View logs:**
```powershell
cd scripts
.\view-logs.ps1
```

**Read documentation:**
- Start: [docs/START_HERE.md](docs/START_HERE.md)
- Index: [INDEX.md](INDEX.md)

**Build Docker image:**
```powershell
podman build -f docker/Dockerfile -t distributed-queue:latest .
```

## 📋 Folder Purposes

| Folder | Purpose | Key Files |
|--------|---------|-----------|
| `src/` | Source code (.NET projects) | Core, API, Kafka |
| `scripts/` | PowerShell automation scripts | deploy, test, view-logs |
| `docker/` | Container configuration | Dockerfile, compose |
| `docs/` | All documentation | Guides, examples, references |

## 🚀 Getting Started Path

1. **Read**: [docs/START_HERE.md](docs/START_HERE.md)
2. **Deploy**: `cd scripts` → `.\deploy-podman.ps1`
3. **Test**: `.\test-api.ps1`
4. **Explore**: http://localhost:8080/swagger

## 📚 Documentation Organization

All docs are in `docs/` folder, organized by purpose:

**Getting Started:**
- START_HERE.md
- QUICKSTART.md

**Deployment:**
- DEPLOY_README.md
- PODMAN_DEPLOYMENT.md
- DEPLOYMENT_CHECKLIST.md
- DEPLOYMENT_FLOW.md

**Usage:**
- API_EXAMPLES.md

**Architecture:**
- PROJECT_SUMMARY.md
- STRUCTURE.md
- EXTENSION_GUIDE.md

## 🔧 Scripts Organization

All scripts are in `scripts/` folder:

| Script | Purpose |
|--------|---------|
| `deploy-podman.ps1` | Build and deploy to Podman Desktop |
| `start.ps1` | Build and run locally (development) |
| `test-api.ps1` | Run automated API tests |
| `view-logs.ps1` | View container logs in real-time |
| `stop-podman.ps1` | Stop and remove container |

## 🐳 Docker Organization

All Docker files are in `docker/` folder:

| File | Purpose |
|------|---------|
| `Dockerfile` | Multi-stage build configuration |
| `docker-compose.yml` | Compose orchestration |
| `.dockerignore` | Build context exclusions |

## ✅ Benefits of This Structure

1. **Clear Separation**: Code, scripts, docs, and Docker files are separate
2. **Easy Navigation**: Know exactly where to find things
3. **Professional**: Standard industry organization
4. **Scalable**: Easy to add more scripts or docs
5. **Clean Root**: Root directory is uncluttered

## 🎨 Visual Overview

```
┌─────────────────────────────────────────────┐
│  DistributedQueue/ (Root)                   │
│  ├── Clean & Organized                      │
│  ├── Only essential files in root           │
│  └── Everything categorized                 │
└─────────────────────────────────────────────┘
         │
         ├──► src/      (Code - don't touch for deployment)
         ├──► scripts/  (Run scripts from here!) ⭐
         ├──► docker/   (Container configs)
         └──► docs/     (Read documentation here!)
```

---

**This is much cleaner! 🎉**

Start with: `cd scripts` then `.\deploy-podman.ps1`
