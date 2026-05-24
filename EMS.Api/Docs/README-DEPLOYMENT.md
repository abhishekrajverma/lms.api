# EMS Deployment Guide

## Overview

Complete guide to deploying the EMS API to production using Docker, GitHub Actions, and Azure Container Instances.

## Table of Contents

1. [Docker Deployment](#docker-deployment)
2. [Docker Compose (Local)](#docker-compose-local)
3. [GitHub Actions CI/CD](#github-actions-cicd)
4. [Azure Deployment](#azure-deployment)
5. [Kubernetes (Optional)](#kubernetes-optional)
6. [Monitoring & Logs](#monitoring--logs)
7. [Rollback Procedures](#rollback-procedures)

## Docker Deployment

### Building Docker Image

```bash
# Build image locally
docker build -t ems-api:latest -f EMS.Api/Dockerfile .

# Build with specific tag
docker build -t ems-api:v1.0.0 -f EMS.Api/Dockerfile .

# Build for multi-platform
docker buildx build --platform linux/amd64,linux/arm64 -t ems-api:latest .
```

### Running Docker Container

```bash
# Run with environment variables
docker run -d \
  --name ems-api \
  -p 5000:5000 \
  -e ConnectionStrings__DefaultConnection="Server=sqlserver;Database=EMS_DB;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;" \
  -e ConnectionStrings__Redis="redis:6379" \
  -e JwtSettings__SecretKey="your-secret-key" \
  -e ASPNETCORE_ENVIRONMENT="Production" \
  ems-api:latest

# View container logs
docker logs -f ems-api

# Stop container
docker stop ems-api

# Remove container
docker rm ems-api
```

### Docker Image Layers

The Dockerfile uses multi-stage build:

```
Stage 1: Build
  ?? Copy project files
  ?? Restore dependencies
  ?? Copy source
  ?? Build Release
     ?
Stage 2: Publish
  ?? Publish to /app/publish
     ?
Stage 3: Runtime
  ?? Copy published files
  ?? Create logs directory
  ?? Set environment
  ?? Health check
  ?? Run dotnet EMS.Api.dll
```

## Docker Compose (Local)

### Setup with Docker Compose

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f ems-api

# Stop services
docker-compose down

# Remove volumes (cleanup)
docker-compose down -v
```

### Services

```yaml
sqlserver:       Database
redis:          Cache
ems-api:        Application
```

### Environment Setup

```bash
# Copy environment template
cp .env.example .env

# Edit .env file
nano .env
```

### Connection Strings

**Local Development:**
```
Server=localhost;Database=EMS_DB;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;
```

**Docker Compose:**
```
Server=sqlserver,1433;Database=EMS_DB;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;
```

## GitHub Actions CI/CD

### Automated Workflows

#### 1. Build & Test (`.github/workflows/build-test.yml`)

Runs on every push and pull request:

```
Push to main/develop
    ?
Checkout code
    ?
Setup .NET
    ?
Restore & Build
    ?
Run Unit Tests
    ?
Collect Coverage
    ?
Upload Artifacts
```

#### 2. Docker Build (`.github/workflows/docker-build.yml`)

Builds and pushes Docker image:

```
Push to main/develop or tag
    ?
Setup Docker Buildx
    ?
Login to Registry (ghcr.io)
    ?
Build & Push Image
    ?
Tag with commit/branch
```

#### 3. Deploy Staging (`.github/workflows/deploy-staging.yml`)

Deploys to staging on develop push:

```
Push to develop
    ?
Build Docker image
    ?
Deploy to Azure ACI
    ?
Run Integration Tests
    ?
Notify Slack
```

#### 4. Deploy Production (`.github/workflows/deploy-prod.yml`)

Deploys to production on main push:

```
Push to main/tag
    ?
Build Docker image
    ?
Deploy to Azure ACI
    ?
Health check
    ?
Run Smoke Tests
    ?
Create Release
    ?
Notify Slack
```

### Secrets Configuration

Required GitHub secrets:

```
AZURE_CREDENTIALS               # Azure service principal JSON
STAGING_DATABASE_CONNECTION_STRING
PROD_DATABASE_CONNECTION_STRING
STAGING_REDIS_CONNECTION_STRING
PROD_REDIS_CONNECTION_STRING
JWT_SECRET_KEY
SLACK_WEBHOOK                   # Slack notifications
SONAR_TOKEN                     # Code quality scans
```

### Setting Up Secrets

```bash
# Using GitHub CLI
gh secret set AZURE_CREDENTIALS < azure-credentials.json
gh secret set JWT_SECRET_KEY --body "your-secret-key"

# Or through GitHub Web UI
Settings ? Secrets and variables ? Actions ? New repository secret
```

## Azure Deployment

### Prerequisites

1. Azure account
2. Azure CLI installed
3. Service Principal created

### Creating Service Principal

```bash
# Login to Azure
az login

# Create service principal
az ad sp create-for-rbac \
  --name "ems-api-deployment" \
  --role contributor \
  --scopes /subscriptions/{subscription-id}

# Output: JSON with credentials for AZURE_CREDENTIALS secret
```

### Create Azure Resources

```bash
# Create resource group
az group create \
  --name ems-prod-rg \
  --location westus

# Create SQL Server
az sql server create \
  --name ems-sql-server \
  --resource-group ems-prod-rg \
  --admin-user sqladmin \
  --admin-password "YourPassword123!"

# Create Azure Cache for Redis
az redis create \
  --name ems-redis \
  --resource-group ems-prod-rg \
  --sku basic \
  --vm-size c0
```

### Container Instances Deployment

```bash
# Manual deployment
az container create \
  --resource-group ems-prod-rg \
  --name ems-api \
  --image ghcr.io/your-org/ems-backend:latest \
  --cpu 2 --memory 4 \
  --ports 5000 \
  --environment-variables \
    ASPNETCORE_ENVIRONMENT=Production \
    JwtSettings__SecretKey='your-secret' \
  --secrets \
    ConnectionStrings__DefaultConnection='your-connection-string'

# View logs
az container logs \
  --resource-group ems-prod-rg \
  --name ems-api
```

## Kubernetes (Optional)

### Prerequisites

```bash
# Install kubectl
curl -LO "https://dl.k8s.io/release/$(curl -L -s https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl"
chmod +x kubectl
sudo mv kubectl /usr/local/bin/
```

### Deploy to AKS

```bash
# Create AKS cluster
az aks create \
  --resource-group ems-prod-rg \
  --name ems-cluster \
  --node-count 2 \
  --vm-set-type VirtualMachineScaleSets

# Get credentials
az aks get-credentials \
  --resource-group ems-prod-rg \
  --name ems-cluster

# Apply deployment
kubectl apply -f kubernetes/deployment.yaml
kubectl apply -f kubernetes/service.yaml

# Check status
kubectl get pods
kubectl get services
```

### Kubernetes Manifest Example

```yaml
# kubernetes/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: ems-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: ems-api
  template:
    metadata:
      labels:
        app: ems-api
    spec:
      containers:
      - name: ems-api
        image: ghcr.io/your-org/ems-backend:latest
        ports:
        - containerPort: 5000
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: JwtSettings__SecretKey
          valueFrom:
            secretKeyRef:
              name: ems-secrets
              key: jwt-secret
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 5000
          initialDelaySeconds: 10
          periodSeconds: 30
```

## Monitoring & Logs

### Application Logs

```bash
# View logs in container
docker logs ems-api

# Stream logs
docker logs -f ems-api

# Last 100 lines
docker logs --tail=100 ems-api
```

### Database Logs

```bash
# Check SQL Server logs
docker exec ems-sqlserver \
  /opt/mssql-tools/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P YourPassword123! \
  -Q "SELECT TOP 100 * FROM [dbo].[Logs] ORDER BY [TimeStamp] DESC"
```

### Azure Monitor

```bash
# View container logs in Azure
az container logs \
  --resource-group ems-prod-rg \
  --name ems-api \
  --follow
```

### Health Check

```bash
# Check API health
curl http://localhost:5000/health

# Expected response
{"status":"healthy","timestamp":"2024-01-04T12:00:00Z"}
```

## Rollback Procedures

### Docker Rollback

```bash
# Stop current container
docker stop ems-api

# Remove current container
docker rm ems-api

# Run previous version
docker run -d \
  --name ems-api \
  -p 5000:5000 \
  ems-api:v1.0.0
```

### Azure Rollback

```bash
# Restart container with previous image
az container create \
  --resource-group ems-prod-rg \
  --name ems-api-rollback \
  --image ghcr.io/your-org/ems-backend:v1.0.0 \
  --cpu 2 --memory 4 \
  --ports 5000
```

### Kubernetes Rollback

```bash
# View rollout history
kubectl rollout history deployment/ems-api

# Rollback to previous version
kubectl rollout undo deployment/ems-api

# Rollback to specific revision
kubectl rollout undo deployment/ems-api --to-revision=2
```

## Troubleshooting

### Container Won't Start

```bash
# Check container logs
docker logs ems-api

# Inspect container
docker inspect ems-api

# Check health status
docker ps | grep ems-api
```

### Database Connection Fails

```bash
# Test SQL Server connection
docker exec ems-sqlserver \
  /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P password -Q "SELECT 1"

# Check connection string format
# Server=host,port;Database=name;User Id=sa;Password=password;
```

### Out of Memory

```bash
# Check resource usage
docker stats ems-api

# Increase memory limit
docker update --memory 2g ems-api
```

## Security Checklist

- [ ] JWT secret key is secure (>32 characters)
- [ ] Database password is strong
- [ ] HTTPS is enforced
- [ ] CORS is properly configured
- [ ] Security headers are enabled
- [ ] Secrets not in source code
- [ ] Images scanned for vulnerabilities
- [ ] Regular backup of database
- [ ] Monitoring and alerts configured
- [ ] Rate limiting enabled

## Performance Optimization

```bash
# Monitor container resource usage
docker stats

# Set resource limits
docker run --memory 2g --cpus 1 ems-api:latest

# Enable caching layer
# See docker-compose.yml for Redis configuration
```

## Backup & Recovery

```bash
# Backup SQL Server database
docker exec ems-sqlserver \
  /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P password \
  -Q "BACKUP DATABASE EMS_DB TO DISK = '/var/opt/mssql/backup/ems_db.bak'"

# Restore from backup
docker exec ems-sqlserver \
  /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P password \
  -Q "RESTORE DATABASE EMS_DB FROM DISK = '/var/opt/mssql/backup/ems_db.bak'"
```

## References

- [Docker Documentation](https://docs.docker.com/)
- [Azure Container Instances](https://docs.microsoft.com/en-us/azure/container-instances/)
- [GitHub Actions](https://docs.github.com/en/actions)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
