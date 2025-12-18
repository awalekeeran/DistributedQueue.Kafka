<#
.SYNOPSIS
    Deploy DistributedQueue API to Azure Kubernetes Service (AKS)
.DESCRIPTION
    Builds container image, pushes to Azure Container Registry (ACR), and deploys to AKS
.PARAMETER ResourceGroup
    Azure Resource Group name
.PARAMETER AksClusterName
    AKS cluster name
.PARAMETER AcrName
    Azure Container Registry name
.PARAMETER Mode
    Deployment mode: InMemory (default), Kafka, or Hybrid
.PARAMETER ImageTag
    Container image tag (default: latest)
.PARAMETER BuildOnly
    Only build and push image, don't deploy
.PARAMETER DeployOnly
    Only deploy to AKS, don't build image
.EXAMPLE
    .\deploy-aks.ps1 -ResourceGroup "rg-distributedqueue" -AksClusterName "aks-dq" -AcrName "acrdq"
    Full deployment in InMemory mode
.EXAMPLE
    .\deploy-aks.ps1 -ResourceGroup "rg-dq" -AksClusterName "aks-dq" -AcrName "acrdq" -Mode Kafka
    Deploy in Kafka mode
.EXAMPLE
    .\deploy-aks.ps1 -ResourceGroup "rg-dq" -AksClusterName "aks-dq" -AcrName "acrdq" -BuildOnly
    Only build and push to ACR
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroup,
    
    [Parameter(Mandatory=$true)]
    [string]$AksClusterName,
    
    [Parameter(Mandatory=$true)]
    [string]$AcrName,
    
    [Parameter()]
    [ValidateSet("InMemory", "Kafka", "Hybrid")]
    [string]$Mode = "InMemory",
    
    [Parameter()]
    [string]$ImageTag = "latest",
    
    [Parameter()]
    [switch]$BuildOnly,
    
    [Parameter()]
    [switch]$DeployOnly
)

$ErrorActionPreference = "Stop"

$imageName = "distributedqueue-api"
$fullImageName = "$AcrName.azurecr.io/$imageName`:$ImageTag"

Write-Host "🚀 DistributedQueue - AKS Deployment Script" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Cyan

# Check prerequisites
Write-Host "`n📋 Checking prerequisites..." -ForegroundColor Yellow

try {
    $azVersion = az version --output json | ConvertFrom-Json
    Write-Host "✅ Azure CLI: $($azVersion.'azure-cli')" -ForegroundColor Green
}
catch {
    Write-Host "❌ Azure CLI is not installed" -ForegroundColor Red
    Write-Host "Install from: https://docs.microsoft.com/cli/azure/install-azure-cli" -ForegroundColor Yellow
    exit 1
}

try {
    kubectl version --client=true --output=json | Out-Null
    Write-Host "✅ kubectl is installed" -ForegroundColor Green
}
catch {
    Write-Host "❌ kubectl is not installed" -ForegroundColor Red
    Write-Host "Install with: az aks install-cli" -ForegroundColor Yellow
    exit 1
}

# Check Azure login
Write-Host "`n🔐 Checking Azure login..." -ForegroundColor Yellow
$account = az account show 2>$null | ConvertFrom-Json
if (-not $account) {
    Write-Host "❌ Not logged in to Azure" -ForegroundColor Red
    Write-Host "Please run: az login" -ForegroundColor Yellow
    exit 1
}
Write-Host "✅ Logged in as: $($account.user.name)" -ForegroundColor Green
Write-Host "   Subscription: $($account.name)" -ForegroundColor Gray

if (-not $DeployOnly) {
    # Login to ACR
    Write-Host "`n🔐 Logging in to Azure Container Registry..." -ForegroundColor Yellow
    az acr login --name $AcrName
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to login to ACR" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Logged in to ACR: $AcrName" -ForegroundColor Green
    
    # Build and push image
    Write-Host "`n🔨 Building container image..." -ForegroundColor Yellow
    $rootPath = Split-Path $PSScriptRoot -Parent
    $dockerfilePath = Join-Path $rootPath "docker\Dockerfile"
    
    Push-Location $rootPath
    try {
        docker build -t $fullImageName -f $dockerfilePath .
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Docker build failed" -ForegroundColor Red
            exit 1
        }
        Write-Host "✅ Image built: $fullImageName" -ForegroundColor Green
        
        Write-Host "`n📤 Pushing image to ACR..." -ForegroundColor Yellow
        docker push $fullImageName
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Docker push failed" -ForegroundColor Red
            exit 1
        }
        Write-Host "✅ Image pushed to ACR" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
    
    if ($BuildOnly) {
        Write-Host "`n✅ Build complete. Image available at: $fullImageName" -ForegroundColor Green
        exit 0
    }
}

# Get AKS credentials
Write-Host "`n🔧 Getting AKS credentials..." -ForegroundColor Yellow
az aks get-credentials --resource-group $ResourceGroup --name $AksClusterName --overwrite-existing
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to get AKS credentials" -ForegroundColor Red
    exit 1
}
Write-Host "✅ AKS credentials configured" -ForegroundColor Green

# Attach ACR to AKS (if not already attached)
Write-Host "`n🔗 Ensuring ACR is attached to AKS..." -ForegroundColor Yellow
az aks update --resource-group $ResourceGroup --name $AksClusterName --attach-acr $AcrName 2>$null
Write-Host "✅ ACR attached to AKS" -ForegroundColor Green

# Update ConfigMap based on mode
Write-Host "`n📝 Configuring deployment mode: $Mode" -ForegroundColor Yellow
$k8sPath = Join-Path (Split-Path $PSScriptRoot -Parent) "k8s"
$configMapPath = Join-Path $k8sPath "configmap.yaml"

$configMapContent = Get-Content $configMapPath -Raw

switch ($Mode) {
    "InMemory" {
        $configMapContent = $configMapContent -replace 'queue-mode-use-inmemory: ".*"', 'queue-mode-use-inmemory: "true"'
        $configMapContent = $configMapContent -replace 'queue-mode-use-kafka: ".*"', 'queue-mode-use-kafka: "false"'
        $configMapContent = $configMapContent -replace 'queue-mode-enable-hybrid: ".*"', 'queue-mode-enable-hybrid: "false"'
        $configMapContent = $configMapContent -replace 'kafka-enabled: ".*"', 'kafka-enabled: "false"'
        Write-Host "✅ In-Memory mode - No Kafka required" -ForegroundColor Green
    }
    "Kafka" {
        $configMapContent = $configMapContent -replace 'queue-mode-use-inmemory: ".*"', 'queue-mode-use-inmemory: "false"'
        $configMapContent = $configMapContent -replace 'queue-mode-use-kafka: ".*"', 'queue-mode-use-kafka: "true"'
        $configMapContent = $configMapContent -replace 'queue-mode-enable-hybrid: ".*"', 'queue-mode-enable-hybrid: "false"'
        $configMapContent = $configMapContent -replace 'kafka-enabled: ".*"', 'kafka-enabled: "true"'
        Write-Host "⚠️  Kafka mode - Ensure Kafka secret is created" -ForegroundColor Yellow
    }
    "Hybrid" {
        $configMapContent = $configMapContent -replace 'queue-mode-use-inmemory: ".*"', 'queue-mode-use-inmemory: "true"'
        $configMapContent = $configMapContent -replace 'queue-mode-use-kafka: ".*"', 'queue-mode-use-kafka: "true"'
        $configMapContent = $configMapContent -replace 'queue-mode-enable-hybrid: ".*"', 'queue-mode-enable-hybrid: "false"'
        $configMapContent = $configMapContent -replace 'kafka-enabled: ".*"', 'kafka-enabled: "true"'
        Write-Host "⚠️  Hybrid mode - Ensure Kafka secret is created" -ForegroundColor Yellow
    }
}

$configMapContent | Out-File -FilePath $configMapPath -Encoding UTF8 -Force

# Update kustomization.yaml with correct image
Write-Host "`n🔧 Updating Kustomization..." -ForegroundColor Yellow
$kustomizationPath = Join-Path $k8sPath "kustomization.yaml"
$kustomizationContent = Get-Content $kustomizationPath -Raw
$kustomizationContent = $kustomizationContent -replace 'newName:.*', "newName: $AcrName.azurecr.io/$imageName"
$kustomizationContent = $kustomizationContent -replace 'newTag:.*', "newTag: $ImageTag"
$kustomizationContent | Out-File -FilePath $kustomizationPath -Encoding UTF8 -Force

# Deploy to AKS
Write-Host "`n🚀 Deploying to AKS..." -ForegroundColor Yellow
Push-Location $k8sPath
try {
    kubectl apply -k .
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Deployment failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Deployed to AKS" -ForegroundColor Green
}
finally {
    Pop-Location
}

# Wait for deployment
Write-Host "`n⏳ Waiting for deployment to be ready..." -ForegroundColor Yellow
kubectl rollout status deployment/distributedqueue-api --timeout=5m
if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️  Deployment status check timed out" -ForegroundColor Yellow
}

# Get service information
Write-Host "`n📊 Service Information:" -ForegroundColor Cyan
kubectl get service distributedqueue-api

Write-Host "`n⏳ Waiting for external IP..." -ForegroundColor Yellow
$maxRetries = 60
$retryCount = 0
$externalIP = $null

while ($retryCount -lt $maxRetries) {
    $service = kubectl get service distributedqueue-api -o json | ConvertFrom-Json
    $externalIP = $service.status.loadBalancer.ingress[0].ip
    
    if ($externalIP) {
        break
    }
    
    Start-Sleep -Seconds 5
    $retryCount++
    Write-Host "." -NoNewline
}

Write-Host ""

# Show completion message
Write-Host "`n" + ("=" * 60) -ForegroundColor Cyan
Write-Host "✅ Deployment Complete!" -ForegroundColor Green
Write-Host ("=" * 60) -ForegroundColor Cyan
Write-Host ""
Write-Host "📍 Image: " -NoNewline -ForegroundColor Cyan
Write-Host $fullImageName -ForegroundColor White
Write-Host "📍 Mode:  " -NoNewline -ForegroundColor Cyan
Write-Host $Mode -ForegroundColor White

if ($externalIP) {
    Write-Host ""
    Write-Host "🌐 API Endpoint: " -NoNewline -ForegroundColor Cyan
    Write-Host "http://$externalIP" -ForegroundColor White
    Write-Host "🌐 Swagger UI:   " -NoNewline -ForegroundColor Cyan
    Write-Host "http://$externalIP/swagger" -ForegroundColor White
    Write-Host "🌐 Health Check: " -NoNewline -ForegroundColor Cyan
    Write-Host "http://$externalIP/health" -ForegroundColor White
}
else {
    Write-Host ""
    Write-Host "⚠️  External IP not available yet. Check with:" -ForegroundColor Yellow
    Write-Host "   kubectl get service distributedqueue-api" -ForegroundColor Gray
}

Write-Host ""
Write-Host "📝 Useful Commands:" -ForegroundColor Cyan
Write-Host "  View pods:        kubectl get pods" -ForegroundColor Gray
Write-Host "  View logs:        kubectl logs -l app=distributedqueue-api -f" -ForegroundColor Gray
Write-Host "  Describe deploy:  kubectl describe deployment distributedqueue-api" -ForegroundColor Gray
Write-Host "  Scale:            kubectl scale deployment distributedqueue-api --replicas=3" -ForegroundColor Gray
Write-Host "  Delete:           kubectl delete -k k8s/" -ForegroundColor Gray
Write-Host ""
