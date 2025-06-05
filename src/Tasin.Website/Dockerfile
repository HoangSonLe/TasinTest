# Use the official .NET 8.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file and project file for better layer caching
COPY Tasin.sln ./
COPY src/Tasin.Website/Tasin.Website.csproj ./src/Tasin.Website/

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Build and publish the application
WORKDIR /src/src/Tasin.Website
RUN dotnet publish -c Release -o /app/publish --no-restore

# Use the official .NET 8.0 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install necessary packages for System.Drawing.Common and other dependencies
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    curl \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/* \
    && apt-get clean

# Create directories for file uploads and logs with proper permissions
RUN mkdir -p /app/Files /app/FilesWithLowQuality /app/logs /app/wwwroot

# Copy published application
COPY --from=build /app/publish .

# Create a non-root user for security
RUN groupadd -r appgroup && useradd -r -g appgroup appuser \
    && chown -R appuser:appgroup /app \
    && chmod -R 755 /app

USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Health check (remove if no health endpoint exists)
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/ || exit 1

# Start the application
ENTRYPOINT ["dotnet", "Tasin.Website.dll"]
