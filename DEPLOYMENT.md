# DistributedQueue - Deployment Guide

This guide covers deploying the DistributedQueue API to **Podman Desktop** (local) and **Azure Kubernetes Service** (AKS).

---

## 📋 Table of Contents

- [Prerequisites](#prerequisites)
- [Podman Desktop Deployment (Local)](#podman-desktop-deployment-local)
- [Azure Kubernetes Service (AKS) Deployment](#azure-kubernetes-service-aks-deployment)
- [Configuration Modes](#configuration-modes)
- [Troubleshooting](#troubleshooting)

---

## Prerequisites

### For Podman Desktop Deployment

- **Podman Desktop** installed ([Download here](https://podman-desktop.io/))
- **PowerShell** 5.1 or later
- **Docker Compose** or **Podman Compose** (usually included with Podman Desktop)

### For AKS Deployment

- **Azure CLI** installed ([Install guide](https://docs.microsoft.com/cli/azure/install-azure-cli))
- **kubectl** installed (`az aks install-cli`)
- **Docker** or **Podman** for building images
- **Azure subscription** with permissions to:
  - Create/manage AKS clusters
  - Create/manage Azure Container Registry (ACR)
  - Assign roles (ACR to AKS attachment)

---

## Podman Desktop Deployment (Local)

### Quick Start

Deploy in **In-Memory mode** (no Kafka required):

```powershell
cd c:\Workshops\Designs\DistributedQueue
.\scripts\deploy-podman.ps1
```

This will:
1. ✅ Check Podman installation
2. ✅ Create `.env` configuration
3. ✅ Build container image (if needed)
4. ✅ Start containers with docker-compose
5. ✅ Wait for API health check
6. ✅ Display access URLs

### Access the API

Once deployed:

- **API Base URL**: http://localhost:8080
- **Swagger UI**: http://localhost:8080/swagger
- **Health Check**: http://localhost:8080/health

### Deployment Options

#### Rebuild Image

Force rebuild of the container image:

```powershell
.\scripts\deploy-podman.ps1 -Build
```

#### Deploy with Kafka Mode

**Note**: Requires Kafka credentials in `.env` file

```powershell
.\scripts\deploy-podman.ps1 -Mode Kafka
```

After running, edit `docker/.env` with your Kafka credentials:

```bash
KAFKA_BOOTSTRAP_SERVERS=your-cluster.confluent.cloud:9092
KAFKA_API_KEY=YOUR_API_KEY
KAFKA_API_SECRET=YOUR_API_SECRET
```

Then restart:

```powershell
cd docker
docker-compose restart
```

#### Deploy with Hybrid Mode

Combines In-Memory + Kafka with fallback:

```powershell
.\scripts\deploy-podman.ps1 -Mode Hybrid
```

#### View Logs

Show container logs in real-time:

```powershell
.\scripts\deploy-podman.ps1 -Logs
```

Or separately:

```powershell
cd docker
docker-compose logs -f
```

#### Stop Containers

```powershell
.\scripts\deploy-podman.ps1 -Stop
```

Or:

```powershell
cd docker
docker-compose down
```

### Manual Deployment (Alternative)

If you prefer manual control:

```powershell
cd docker

# Create .env file from example
Copy-Item .env.example .env

# Edit .env with your configuration
notepad .env

# Build and start
docker-compose up --build -d

# View logs
docker-compose logs -f

# Stop
docker-compose down
```

---

## Azure Kubernetes Service (AKS) Deployment

### Prerequisites Setup

1. **Login to Azure**

```powershell
az login
```

2. **Set your subscription** (if you have multiple)

```powershell
az account set --subscription "YOUR_SUBSCRIPTION_NAME"
```

3. **Create Resource Group** (if not exists)

```powershell
az group create --name rg-distributedqueue --location eastus
```

4. **Create Azure Container Registry**

```powershell
az acr create --resource-group rg-distributedqueue --name acrdistributedqueue --sku Basic
```

5. **Create AKS Cluster**

```powershell
# Basic cluster (2 nodes)
az aks create `
  --resource-group rg-distributedqueue `
  --name aks-distributedqueue `
  --node-count 2 `
  --enable-managed-identity `
  --generate-ssh-keys `
  --attach-acr acrdistributedqueue
```

For production, consider:
- Larger node count
- Autoscaling enabled
- System node pool + user node pool
- Network policies
- Azure Monitor integration

### Deploy to AKS

#### Quick Deployment (In-Memory Mode)

```powershell
cd c:\Workshops\Designs\DistributedQueue

.\scripts\deploy-aks.ps1 `
  -ResourceGroup "rg-distributedqueue" `
  -AksClusterName "aks-distributedqueue" `
  -AcrName "acrdistributedqueue"
```

This will:
1. ✅ Build container image
2. ✅ Push to Azure Container Registry
3. ✅ Configure kubectl context
4. ✅ Attach ACR to AKS (if needed)
5. ✅ Deploy ConfigMap and Deployment
6. ✅ Create LoadBalancer Service
7. ✅ Wait for external IP
8. ✅ Display access URLs

#### Deploy with Kafka Mode

**Step 1**: Create Kafka secret

```powershell
cd k8s

# Copy example file
Copy-Item secret.yaml.example secret.yaml

# Edit with your Kafka credentials
notepad secret.yaml

# Update these values:
# - bootstrap-servers: your-cluster.confluent.cloud:9092
# - sasl-username: YOUR_API_KEY
# - sasl-password: YOUR_API_SECRET

# Apply secret to AKS
kubectl apply -f secret.yaml
```

**Step 2**: Deploy in Kafka mode

```powershell
cd ..

.\scripts\deploy-aks.ps1 `
  -ResourceGroup "rg-distributedqueue" `
  -AksClusterName "aks-distributedqueue" `
  -AcrName "acrdistributedqueue" `
  -Mode Kafka
```

#### Deploy with Custom Image Tag

```powershell
.\scripts\deploy-aks.ps1 `
  -ResourceGroup "rg-distributedqueue" `
  -AksClusterName "aks-distributedqueue" `
  -AcrName "acrdistributedqueue" `
  -ImageTag "v1.0.0"
```

#### Build and Push Only (No Deployment)

```powershell
.\scripts\deploy-aks.ps1 `
  -ResourceGroup "rg-distributedqueue" `
  -AksClusterName "aks-distributedqueue" `
  -AcrName "acrdistributedqueue" `
  -BuildOnly
```

#### Deploy Only (Image Already in ACR)

```powershell
.\scripts\deploy-aks.ps1 `
  -ResourceGroup "rg-distributedqueue" `
  -AksClusterName "aks-distributedqueue" `
  -AcrName "acrdistributedqueue" `
  -DeployOnly
```

### Access the Deployed API

After deployment completes, the script will display:

```
🌐 API Endpoint: http://<EXTERNAL_IP>
🌐 Swagger UI:   http://<EXTERNAL_IP>/swagger
🌐 Health Check: http://<EXTERNAL_IP>/health
```

If external IP is not ready, check manually:

```powershell
kubectl get service distributedqueue-api
```

### AKS Management Commands

#### View Pods

```powershell
kubectl get pods
kubectl describe pod <POD_NAME>
```

#### View Logs

```powershell
# All pods
kubectl logs -l app=distributedqueue-api -f

# Specific pod
kubectl logs <POD_NAME> -f

# Previous pod (if crashed)
kubectl logs <POD_NAME> --previous
```

#### Scale Deployment

```powershell
# Scale to 3 replicas
kubectl scale deployment distributedqueue-api --replicas=3

# Verify
kubectl get deployment
```

#### Update Configuration

Edit ConfigMap:

```powershell
kubectl edit configmap distributedqueue-config
```

Or apply updated file:

```powershell
kubectl apply -f k8s/configmap.yaml
```

Restart deployment to pick up changes:

```powershell
kubectl rollout restart deployment distributedqueue-api
```

#### Delete Deployment

```powershell
# Delete all resources
kubectl delete -k k8s/

# Or individually
kubectl delete deployment distributedqueue-api
kubectl delete service distributedqueue-api
kubectl delete configmap distributedqueue-config
kubectl delete secret kafka-credentials
```

### Monitor Deployment

#### View Deployment Status

```powershell
kubectl get deployment distributedqueue-api
kubectl describe deployment distributedqueue-api
```

#### Check Rollout Status

```powershell
kubectl rollout status deployment/distributedqueue-api
```

#### View Events

```powershell
kubectl get events --sort-by=.metadata.creationTimestamp
```

---

## Configuration Modes

The application supports three operation modes:

### 1. In-Memory Mode (Default)

- ✅ No external dependencies
- ✅ Fast and simple
- ✅ Perfect for development/testing
- ⚠️ Data lost on restart
- ⚠️ Not for production

**Configuration**:
```yaml
queue-mode-use-inmemory: "true"
queue-mode-use-kafka: "false"
queue-mode-enable-hybrid: "false"
kafka-enabled: "false"
```

### 2. Kafka Mode

- ✅ Production-ready
- ✅ Persistent messages
- ✅ High throughput
- ✅ Scalable
- ⚠️ Requires Kafka cluster

**Configuration**:
```yaml
queue-mode-use-inmemory: "false"
queue-mode-use-kafka: "true"
queue-mode-enable-hybrid: "false"
kafka-enabled: "true"
```

**Required**: Kafka credentials in Secret (see `k8s/secret.yaml.example`)

### 3. Hybrid Mode

- ✅ Best of both worlds
- ✅ Kafka primary, in-memory fallback
- ✅ Graceful degradation
- ⚠️ Requires Kafka cluster

**Configuration**:
```yaml
queue-mode-use-inmemory: "true"
queue-mode-use-kafka: "true"
queue-mode-enable-hybrid: "true"
kafka-enabled: "true"
```

---

## Troubleshooting

### Podman Desktop Issues

#### Container fails to start

Check logs:
```powershell
cd docker
docker-compose logs
```

#### Port already in use

Change port in `docker-compose.yml`:
```yaml
ports:
  - "8081:8080"  # Change 8080 to 8081
```

#### Build fails

Clean and rebuild:
```powershell
docker-compose down -v
docker-compose build --no-cache
docker-compose up -d
```

### AKS Issues

#### Pods not starting

Check pod status:
```powershell
kubectl get pods
kubectl describe pod <POD_NAME>
kubectl logs <POD_NAME>
```

Common issues:
- **ImagePullBackOff**: ACR not attached to AKS
  ```powershell
  az aks update --resource-group <RG> --name <AKS> --attach-acr <ACR>
  ```
- **CrashLoopBackOff**: Application error, check logs
- **Pending**: Insufficient resources, scale cluster

#### External IP stuck at `<pending>`

This is normal, can take 2-5 minutes. Check:
```powershell
kubectl describe service distributedqueue-api
kubectl get events
```

#### Can't reach API

1. Check service has external IP:
   ```powershell
   kubectl get service distributedqueue-api
   ```

2. Check pods are running:
   ```powershell
   kubectl get pods
   ```

3. Test from inside cluster:
   ```powershell
   kubectl run test --image=curlimages/curl -it --rm -- curl http://distributedqueue-api/health
   ```

#### Kafka connection fails

1. Verify secret exists:
   ```powershell
   kubectl get secret kafka-credentials
   ```

2. Check secret values:
   ```powershell
   kubectl get secret kafka-credentials -o yaml
   ```

3. Check pod environment variables:
   ```powershell
   kubectl exec <POD_NAME> -- env | grep KAFKA
   ```

4. Check application logs for Kafka errors:
   ```powershell
   kubectl logs <POD_NAME> | grep -i kafka
   ```

### Network Issues

#### CORS errors (if accessing from browser)

API needs CORS configuration. Check Program.cs has:
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

app.UseCors();
```

### Performance Issues

#### High latency

1. Check pod resources:
   ```powershell
   kubectl top pods
   ```

2. Increase resources in `k8s/deployment.yaml`:
   ```yaml
   resources:
     requests:
       memory: "512Mi"
       cpu: "500m"
     limits:
       memory: "1Gi"
       cpu: "1000m"
   ```

3. Scale horizontally:
   ```powershell
   kubectl scale deployment distributedqueue-api --replicas=3
   ```

---

## Next Steps

- ✅ Deploy locally with Podman
- ✅ Test API endpoints
- ✅ Configure Kafka (if needed)
- ✅ Deploy to AKS
- ✅ Configure monitoring (Azure Monitor)
- ✅ Set up CI/CD pipeline
- ✅ Configure autoscaling
- ✅ Add ingress controller (for custom domain)

For more information:
- [API Reference](docs/API_REFERENCE.md)
- [Quick Reference](docs/QUICK_API_REFERENCE.md)
- [README](README.md)
