# Stage 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copy solution file
COPY DistributedQueue.sln ./

# Copy project files
COPY src/DistributedQueue.Core/DistributedQueue.Core.csproj ./src/DistributedQueue.Core/
COPY src/DistributedQueue.Kafka/DistributedQueue.Kafka.csproj ./src/DistributedQueue.Kafka/
COPY src/DistributedQueue.Api/DistributedQueue.Api.csproj ./src/DistributedQueue.Api/

# Restore dependencies
RUN dotnet restore

# Copy all source files
COPY src/ ./src/

# Build the application
WORKDIR /src/src/DistributedQueue.Api
RUN dotnet build -c Release -o /app/build

# Stage 2: Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

# Install CA certificates and openssl for SSL connections
RUN apk add --no-cache \
    ca-certificates \
    ca-certificates-bundle \
    openssl

# Copy ISRG Root X1 certificate (required for Confluent Cloud / Let's Encrypt)
COPY docker/isrg-root-x1.crt /usr/local/share/ca-certificates/isrg-root-x1.crt

# Debug: Check if file was copied
RUN ls -la /usr/local/share/ca-certificates/ && \
    echo "=== Certificate file content ===" && \
    head -3 /usr/local/share/ca-certificates/isrg-root-x1.crt

# Ensure correct permissions and .crt extension
RUN chmod 644 /usr/local/share/ca-certificates/isrg-root-x1.crt

# Update CA certificates to include the new certificate
RUN update-ca-certificates --verbose 2>&1 | tee /tmp/update-ca-certs.log && \
    echo "✅ CA certificates updated"

# Debug: Check what was added
RUN cat /tmp/update-ca-certs.log || true

# Check if ISRG Root X1 is now in the bundle
RUN echo "=== Checking for ISRG Root X1 in bundle ===" && \
    grep "ISRG Root X1" /etc/ssl/certs/ca-certificates.crt && \
    echo "✅ ISRG Root X1 certificate installed successfully" || \
    (echo "⚠️  Warning: ISRG Root X1 not found in bundle" && \
     echo "=== Listing all certificate files ===" && \
     ls -la /etc/ssl/certs/ | grep -i isrg || echo "No ISRG files found" && \
     echo "=== Certificate bundle location ===" && \
     ls -la /etc/ssl/certs/ca-certificates.crt)

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Copy published app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run as root (for terminal access and troubleshooting)
# In production, you may want to use a non-root user
USER root

# Run the application
ENTRYPOINT ["dotnet", "DistributedQueue.Api.dll"]
