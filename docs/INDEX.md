# 📖 Complete Documentation Index

Welcome to the Distributed Queue System documentation!

## 🎯 Quick Links

| What do you want to do? | Go here |
|-------------------------|---------|
| 🚀 **Get started quickly** | [QUICKSTART.md](QUICKSTART.md) |
| 📖 **See all API endpoints** | [API_REFERENCE.md](API_REFERENCE.md) ⭐⭐⭐ |
| 📋 **Quick reference card** | [QUICK_API_REFERENCE.md](QUICK_API_REFERENCE.md) |
| 🔄 **Switch operation modes** | [MODE_SWITCHING.md](MODE_SWITCHING.md) |
| 🐳 **Deploy to Podman** | [PODMAN_DEPLOYMENT.md](PODMAN_DEPLOYMENT.md) |
| ⚙️ **Setup Kafka** | [KAFKA_CONNECTION_TEST.md](KAFKA_CONNECTION_TEST.md) |
| 🏗️ **Understand architecture** | [COMPLETE_ARCHITECTURE_OVERVIEW.md](COMPLETE_ARCHITECTURE_OVERVIEW.md) |

---

## 📚 All Documentation Files

### **Getting Started Guides**
- **[START_HERE.md](START_HERE.md)** ⭐ - Best place to start
- **[QUICKSTART.md](QUICKSTART.md)** - Quick local development setup
- **[DEPLOY_README.md](DEPLOY_README.md)** - Quick deployment guide
- **[START.md](START.md)** - Visual getting started guide

### **API Documentation**
- **[API_REFERENCE.md](API_REFERENCE.md)** ⭐⭐⭐ - **Complete API guide with all endpoints**
- **[QUICK_API_REFERENCE.md](QUICK_API_REFERENCE.md)** - One-page quick reference
- **[API_EXAMPLES.md](API_EXAMPLES.md)** - Sample request bodies for all endpoints
- **[TOPICS_API_QUICK_REFERENCE.md](TOPICS_API_QUICK_REFERENCE.md)** - Topics controller reference

### **Deployment Guides**
- **[PODMAN_DEPLOYMENT.md](PODMAN_DEPLOYMENT.md)** - Complete Podman deployment guide
- **[DEPLOYMENT_CHECKLIST.md](DEPLOYMENT_CHECKLIST.md)** - Deployment verification checklist
- **[DEPLOYMENT_FLOW.md](DEPLOYMENT_FLOW.md)** - Visual deployment flow diagrams

### **Configuration & Setup**
- **[MODE_SWITCHING.md](MODE_SWITCHING.md)** - How to switch between In-Memory/Kafka/Hybrid modes
- **[KAFKA_CONNECTION_TEST.md](KAFKA_CONNECTION_TEST.md)** - Kafka setup and testing
- **[CONFIG_OPTIMIZATION.md](CONFIG_OPTIMIZATION.md)** - Configuration best practices
- **[KAFKA_TROUBLESHOOTING_SOLUTION.md](KAFKA_TROUBLESHOOTING_SOLUTION.md)** - Kafka troubleshooting

### **Architecture Documentation**
- **[COMPLETE_ARCHITECTURE_OVERVIEW.md](COMPLETE_ARCHITECTURE_OVERVIEW.md)** - Full system architecture
- **[STRUCTURE.md](STRUCTURE.md)** - Project structure details
- **[FOLDER_STRUCTURE.md](FOLDER_STRUCTURE.md)** - Detailed folder structure
- **[CONTROLLER_ARCHITECTURE_RECOMMENDATION.md](CONTROLLER_ARCHITECTURE_RECOMMENDATION.md)** - Controller design
- **[TOPICS_CONTROLLER_CONSOLIDATION.md](TOPICS_CONTROLLER_CONSOLIDATION.md)** - Topics controller architecture
- **[ARCHITECTURE_DECISION_SUMMARY.md](ARCHITECTURE_DECISION_SUMMARY.md)** - Key architecture decisions

### **Reference Documentation**
- **[PROJECT_SUMMARY.md](PROJECT_SUMMARY.md)** - High-level project overview
- **[QUICK_REF.md](QUICK_REF.md)** - Quick reference for common tasks
- **[EXTENSION_GUIDE.md](EXTENSION_GUIDE.md)** - How to extend the system
- **[REORGANIZATION.md](REORGANIZATION.md)** - Project reorganization history
- **[ProblemStatement.md](ProblemStatement.md)** - Original problem statement

---

## 🔧 PowerShell Scripts

Located in `scripts/` folder:

### **Main Scripts**
```powershell
# Deploy to Podman Desktop
.\deploy-podman.ps1

# Run locally for development
.\start.ps1

# Comprehensive API testing
.\test-api.ps1
.\test-api.ps1 -Verbose
.\test-api.ps1 -LocalDev

# Interactive API user guide
.\user-guide.ps1
.\user-guide.ps1 -Podman
```

### **Utility Scripts**
```powershell
# View container logs
.\view-logs.ps1

# Stop Podman container
.\stop-podman.ps1

# Test Kafka connectivity
.\test-kafka-connection.ps1
```

---

## 📖 Recommended Reading Order

### **For First-Time Users**
1. [START_HERE.md](START_HERE.md) - Overview
2. [QUICKSTART.md](QUICKSTART.md) - Get it running
3. [API_REFERENCE.md](API_REFERENCE.md) - Learn the API
4. [MODE_SWITCHING.md](MODE_SWITCHING.md) - Understand modes

### **For Developers**
1. [STRUCTURE.md](STRUCTURE.md) - Project structure
2. [COMPLETE_ARCHITECTURE_OVERVIEW.md](COMPLETE_ARCHITECTURE_OVERVIEW.md) - Architecture
3. [API_REFERENCE.md](API_REFERENCE.md) - API details
4. [EXTENSION_GUIDE.md](EXTENSION_GUIDE.md) - Extend the system

### **For DevOps**
1. [DEPLOY_README.md](DEPLOY_README.md) - Quick deploy
2. [PODMAN_DEPLOYMENT.md](PODMAN_DEPLOYMENT.md) - Full deployment guide
3. [DEPLOYMENT_CHECKLIST.md](DEPLOYMENT_CHECKLIST.md) - Verification
4. [KAFKA_CONNECTION_TEST.md](KAFKA_CONNECTION_TEST.md) - Kafka setup

---

## 🔍 I Want To...

| Goal | Documentation |
|------|---------------|
| **Deploy to production** | [PODMAN_DEPLOYMENT.md](PODMAN_DEPLOYMENT.md) |
| **Run locally** | [QUICKSTART.md](QUICKSTART.md) |
| **Use the API** | [API_REFERENCE.md](API_REFERENCE.md) ⭐ |
| **Configure Kafka** | [KAFKA_CONNECTION_TEST.md](KAFKA_CONNECTION_TEST.md) |
| **Switch modes** | [MODE_SWITCHING.md](MODE_SWITCHING.md) |
| **Understand architecture** | [COMPLETE_ARCHITECTURE_OVERVIEW.md](COMPLETE_ARCHITECTURE_OVERVIEW.md) |
| **Extend the system** | [EXTENSION_GUIDE.md](EXTENSION_GUIDE.md) |
| **Troubleshoot Kafka** | [KAFKA_TROUBLESHOOTING_SOLUTION.md](KAFKA_TROUBLESHOOTING.md) |
| **See API examples** | [API_EXAMPLES.md](API_EXAMPLES.md) |
| **Quick reference** | [QUICK_API_REFERENCE.md](QUICK_API_REFERENCE.md) |

---

## 📁 Project Structure

```
DistributedQueue/
├── 📁 src/                          # Source code
│   ├── DistributedQueue.Core/       # Core domain models and services
│   ├── DistributedQueue.Api/        # REST API with mode-aware controllers
│   └── DistributedQueue.Kafka/      # Confluent Kafka integration
│
├── 📁 scripts/                      # PowerShell scripts
│   ├── deploy-podman.ps1            # 🐳 Deploy to Podman
│   ├── start.ps1                    # 💻 Run locally
│   ├── test-api.ps1                 # 🧪 Test all endpoints
│   ├── user-guide.ps1               # 📖 Interactive guide
│   ├── view-logs.ps1                # 📊 View container logs
│   ├── stop-podman.ps1              # 🛑 Stop container
│   └── test-kafka-connection.ps1    # ☁️ Test Kafka
│
├── 📁 docker/                       # Docker/Podman files
│   ├── Dockerfile                   # Multi-stage build config
│   ├── docker-compose.yml           # Compose configuration
│   └── .dockerignore                # Build context exclusions
│
├── 📁 docs/                         # All documentation
│   ├── API_REFERENCE.md             # ⭐⭐⭐ Complete API guide
│   ├── QUICK_API_REFERENCE.md       # Quick reference card
│   ├── INDEX.md                     # This file
│   └── ... (20+ documentation files)
│
├── 📄 README.md                     # Main readme
└── 📄 DistributedQueue.sln          # Solution file
```

---

## 🎯 Documentation by Topic

### **Operation Modes**
The system supports three modes: In-Memory, Kafka, and Hybrid.

**Related Documentation:**
- [MODE_SWITCHING.md](MODE_SWITCHING.md) - How to configure modes
- [API_REFERENCE.md](API_REFERENCE.md) - Mode-aware API usage
- [CONFIG_OPTIMIZATION.md](CONFIG_OPTIMIZATION.md) - Configuration best practices

### **API Usage**
Complete REST API with mode-aware endpoints.

**Related Documentation:**
- **[API_REFERENCE.md](API_REFERENCE.md)** ⭐⭐⭐ - **START HERE**
- [QUICK_API_REFERENCE.md](QUICK_API_REFERENCE.md) - Quick reference
- [API_EXAMPLES.md](API_EXAMPLES.md) - Request/response examples

### **Kafka Integration**
Full Confluent Cloud Kafka support.

**Related Documentation:**
- [KAFKA_CONNECTION_TEST.md](KAFKA_CONNECTION_TEST.md) - Setup guide
- [KAFKA_TROUBLESHOOTING_SOLUTION.md](KAFKA_TROUBLESHOOTING_SOLUTION.md) - Troubleshooting
- [MODE_SWITCHING.md](MODE_SWITCHING.md) - Enable Kafka mode

### **Deployment**
Deploy to Podman or run locally.

**Related Documentation:**
- [PODMAN_DEPLOYMENT.md](PODMAN_DEPLOYMENT.md) - Full deployment guide
- [DEPLOYMENT_CHECKLIST.md](DEPLOYMENT_CHECKLIST.md) - Verification
- [QUICKSTART.md](QUICKSTART.md) - Local development

---

## 🆘 Getting Help

1. **Check the API Reference**: [API_REFERENCE.md](API_REFERENCE.md) - Most comprehensive guide
2. **Run the user guide script**: `.\scripts\user-guide.ps1`
3. **Try Swagger UI**: Interactive API documentation at `/swagger`
4. **Test the API**: Run `.\scripts\test-api.ps1 -Verbose`
5. **Use Quick Reference**: [QUICK_API_REFERENCE.md](QUICK_API_REFERENCE.md)

---

## 🔄 What's New

This documentation reflects the latest system features:

✅ **Mode-Aware Architecture**
- All controllers support In-Memory, Kafka, and Hybrid modes
- `?source=` parameter for filtering by backend
- Mode information in all responses

✅ **Hybrid Message Publishing**
- Publish to both backends simultaneously
- Topics can exist in either or both backends
- Automatic routing based on topic availability

✅ **Kafka Message Retrieval**
- Read messages directly from Kafka via API
- Support for offset control (earliest/latest)
- Configurable message limits

✅ **Enhanced Controllers**
- TopicsController - Unified topic management
- ProducersController - Shows Kafka brokers
- ConsumersController - Shows Kafka consumer members
- MessagesController - Hybrid publishing + retrieval
- DemoController - Works in all modes

✅ **Comprehensive Testing**
- Enhanced test-api.ps1 script
- Interactive user-guide.ps1
- Verbose output support

---

## 📊 Documentation Statistics

- **Total Documentation Files**: 20+
- **API Endpoints Documented**: 40+
- **Code Examples**: 100+
- **PowerShell Scripts**: 7
- **Operation Modes**: 3 (In-Memory, Kafka, Hybrid)

---

**Start Here**: [API_REFERENCE.md](API_REFERENCE.md) | **Main README**: [../README.md](../README.md)

Last Updated: December 2025
