# Use the official .NET 8.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project file first for better caching
COPY src/Tasin.Website/Tasin.Website.csproj ./

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY src/Tasin.Website/ ./



# Build and publish the application
RUN dotnet publish -c Release -o /app/publish

# Verify wwwroot files are copied
RUN ls -la /app/publish/wwwroot/ || echo "wwwroot not found"
RUN ls -la /app/publish/wwwroot/js/ || echo "js folder not found"

# Use the official .NET 8.0 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install necessary packages for System.Drawing.Common and other dependencies
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Create directories for file uploads and logs
RUN mkdir -p /app/Files /app/FilesWithLowQuality /app/logs

# Copy published application
COPY --from=build /app/publish .

# Verify static files are copied to runtime
RUN ls -la /app/wwwroot/ || echo "wwwroot not found in runtime"
RUN ls -la /app/wwwroot/js/ || echo "js folder not found in runtime"

# Create a non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "Tasin.Website.dll"]
