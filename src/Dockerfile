# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY GeminiRAG.Core/GeminiRAG.Core.csproj GeminiRAG.Core/
COPY GeminiRAG.Infrastructure/GeminiRAG.Infrastructure.csproj GeminiRAG.Infrastructure/
COPY GeminiRAG.Api/GeminiRAG.Api.csproj GeminiRAG.Api/

# Restore dependencies
RUN dotnet restore GeminiRAG.Api/GeminiRAG.Api.csproj

# Copy everything else
COPY GeminiRAG.Core/ GeminiRAG.Core/
COPY GeminiRAG.Infrastructure/ GeminiRAG.Infrastructure/
COPY GeminiRAG.Api/ GeminiRAG.Api/

# Build the application
WORKDIR /src/GeminiRAG.Api
RUN dotnet build GeminiRAG.Api.csproj -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish GeminiRAG.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Create a non-root user
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD curl --fail http://localhost:8080/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "GeminiRAG.Api.dll"]
