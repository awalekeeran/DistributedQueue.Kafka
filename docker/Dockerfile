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

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Copy published app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "DistributedQueue.Api.dll"]
