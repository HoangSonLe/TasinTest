# Tasin Application Deployment Script for Windows
# PowerShell version of deploy.sh

param(
    [Parameter(Position=0)]
    [string]$Command = "help",
    [string]$Service = ""
)

# Colors for output
function Write-Info($message) {
    Write-Host "[INFO] $message" -ForegroundColor Blue
}

function Write-Success($message) {
    Write-Host "[SUCCESS] $message" -ForegroundColor Green
}

function Write-Warning($message) {
    Write-Host "[WARNING] $message" -ForegroundColor Yellow
}

function Write-Error($message) {
    Write-Host "[ERROR] $message" -ForegroundColor Red
}

# Check if Docker is running
function Test-Docker {
    try {
        docker version | Out-Null
        docker-compose version | Out-Null
        Write-Success "Docker and Docker Compose are available"
        return $true
    }
    catch {
        Write-Error "Docker is not running or not installed"
        Write-Info "Please start Docker Desktop and try again"
        return $false
    }
}

# Setup environment
function Initialize-Environment {
    if (-not (Test-Path ".env")) {
        if (Test-Path ".env.example") {
            Copy-Item ".env.example" ".env"
            Write-Warning "Created .env file from .env.example"
            Write-Warning "Please edit .env file with your configuration"
        } else {
            Write-Error ".env.example file not found"
            return $false
        }
    } else {
        Write-Success ".env file found"
    }
    return $true
}

# Deploy application
function Start-Deployment {
    Write-Info "Building Docker images..."
    docker-compose build --no-cache

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Docker build failed"
        return
    }

    Write-Info "Starting services..."
    docker-compose up -d

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to start services"
        return
    }

    Write-Info "Waiting for services to be ready..."
    Start-Sleep -Seconds 30

    # Check if services are running
    try {
        $containerStatus = docker ps --filter "name=tasin-web" --format "table {{.Names}}\t{{.Status}}"
        Write-Info "Container status:"
        Write-Host $containerStatus

        Write-Success "Services are running!"

        Write-Info "Application URLs:"
        Write-Host "  - Main Application: http://localhost:8080" -ForegroundColor Cyan
        Write-Host "  - Health Check: http://localhost:8080" -ForegroundColor Cyan

        Write-Info "To view logs:"
        Write-Host "  .\deploy.ps1 logs" -ForegroundColor Cyan
        Write-Host "  docker logs tasin-web" -ForegroundColor Cyan

        # Test health endpoint
        Write-Info "Testing application..."
        try {
            Start-Sleep -Seconds 10
            $response = Invoke-WebRequest -Uri "http://localhost:8080" -TimeoutSec 10
            Write-Success "Application is responding (Status: $($response.StatusCode))"
        }
        catch {
            Write-Warning "Application test failed, but services may still be starting up"
        }

    } catch {
        Write-Error "Failed to check container status. Check logs:"
        docker logs tasin-web --tail 20
    }
}

# Stop services
function Stop-Services {
    Write-Info "Stopping services..."
    docker-compose down
    Write-Success "Services stopped"
}

# Restart services
function Restart-Services {
    Write-Info "Restarting services..."
    docker-compose restart
    Write-Success "Services restarted"
}

# View logs
function Show-Logs {
    if ($Service) {
        docker-compose logs -f $Service
    } else {
        docker-compose logs -f
    }
}

# Database backup (external database)
function New-Backup {
    Write-Warning "Database backup not available - using external PostgreSQL server"
    Write-Info "Database is hosted externally at 103.72.98.222:15432"
    Write-Info "Please contact database administrator for backup procedures"
}

# Test application
function Test-Application {
    Write-Info "Testing Tasin application..."
    
    # Check if services are running
    $services = docker-compose ps --format json | ConvertFrom-Json
    $runningServices = $services | Where-Object { $_.State -eq "running" }
    
    if ($runningServices.Count -eq 0) {
        Write-Error "No services are running. Please deploy first."
        return
    }
    
    Write-Success "Services running: $($runningServices.Count)"
    
    # Test health endpoint
    Write-Info "Testing health endpoint..."
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:8080/health" -TimeoutSec 10
        Write-Success "Health check: $($response.status) at $($response.timestamp)"
    }
    catch {
        Write-Error "Health check failed: $($_.Exception.Message)"
    }
    
    # Test main page
    Write-Info "Testing main application..."
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8080" -TimeoutSec 10
        if ($response.StatusCode -eq 200) {
            Write-Success "Main application accessible (Status: $($response.StatusCode))"
        } else {
            Write-Warning "Main application returned status: $($response.StatusCode)"
        }
    }
    catch {
        Write-Error "Main application test failed: $($_.Exception.Message)"
    }
    
    # Test Swagger
    Write-Info "Testing Swagger API documentation..."
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8080/swagger" -TimeoutSec 10
        if ($response.StatusCode -eq 200) {
            Write-Success "Swagger documentation accessible"
        }
    }
    catch {
        Write-Warning "Swagger documentation test failed: $($_.Exception.Message)"
    }
    
    # Test database connection
    Write-Info "Testing external database connection..."
    Write-Info "Database: 103.72.98.222:15432"
    Write-Info "Connection test will be performed by the application on startup"
    
    Write-Info "Test completed. Check results above."
}

# Show help
function Show-Help {
    Write-Host "Tasin Application Deployment Script for Windows" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Usage: .\deploy.ps1 [COMMAND] [-Service SERVICE_NAME]" -ForegroundColor White
    Write-Host ""
    Write-Host "Commands:" -ForegroundColor Yellow
    Write-Host "  deploy    Build and start with Docker Compose" -ForegroundColor White
    Write-Host "  test      Build and test standalone container" -ForegroundColor White
    Write-Host "  prod      Deploy with production optimizations" -ForegroundColor White
    Write-Host "  check     Test application endpoints and connectivity" -ForegroundColor White
    Write-Host "  stop      Stop all services" -ForegroundColor White
    Write-Host "  restart   Restart all services" -ForegroundColor White
    Write-Host "  logs      View logs (optionally specify service name)" -ForegroundColor White
    Write-Host "  help      Show this help message" -ForegroundColor White
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Yellow
    Write-Host "  .\deploy.ps1 deploy   # Full Docker Compose deployment" -ForegroundColor Cyan
    Write-Host "  .\deploy.ps1 test     # Quick test with standalone container" -ForegroundColor Cyan
    Write-Host "  .\deploy.ps1 prod     # Production deployment" -ForegroundColor Cyan
    Write-Host "  .\deploy.ps1 check    # Test running application" -ForegroundColor Cyan
    Write-Host "  .\deploy.ps1 logs     # View all logs" -ForegroundColor Cyan
}

# Test deployment function
function Start-TestDeployment {
    Write-Info "Building and testing Docker image..."

    # Stop existing containers
    docker stop tasin-web 2>$null
    docker rm tasin-web 2>$null

    # Build image
    docker build -t tasin-web .

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Docker build failed"
        return
    }

    # Run test container
    Write-Info "Starting test container..."
    docker run -d --name tasin-web -p 8080:8080 `
        -e ASPNETCORE_ENVIRONMENT=Production `
        -e "ConnectionStrings__TasinDB=Server=host.docker.internal;Port=5434;Database=Tasin;User Id=postgres;Password=123456;" `
        tasin-web

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to start test container"
        return
    }

    Write-Info "Waiting for application to start..."
    Start-Sleep -Seconds 20

    Write-Success "Test deployment completed!"
    Write-Info "Application URLs:"
    Write-Host "  - Main Application: http://localhost:8080" -ForegroundColor Cyan
    Write-Host "  - Test with external DB" -ForegroundColor Yellow
}

# Production deployment function
function Start-ProductionDeployment {
    Write-Info "Using production configuration..."

    # Set production environment
    $env:ASPNETCORE_ENVIRONMENT = "Production"

    Write-Info "Building with production optimizations..."
    docker-compose build --no-cache

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Production build failed"
        return
    }

    Write-Info "Starting services with production settings..."
    docker-compose up -d

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to start production services"
        return
    }

    Write-Info "Waiting for services to be ready..."
    Start-Sleep -Seconds 45

    Write-Success "Production deployment completed!"
    Write-Info "Application URLs:"
    Write-Host "  - Main Application: http://localhost:8080" -ForegroundColor Cyan
    Write-Host "  - API Documentation: http://localhost:8080/swagger" -ForegroundColor Cyan
    Write-Host "  - Health Check: http://localhost:8080/health" -ForegroundColor Cyan

    Write-Warning "Remember to:"
    Write-Host "  1. Configure SSL certificates for production" -ForegroundColor Yellow
    Write-Host "  2. Set up regular backups" -ForegroundColor Yellow
    Write-Host "  3. Monitor application logs" -ForegroundColor Yellow
}

# Main script logic
switch ($Command.ToLower()) {
    "deploy" {
        if (-not (Test-Docker)) { exit 1 }
        if (-not (Initialize-Environment)) { exit 1 }
        Start-Deployment
    }
    "test" {
        if (-not (Test-Docker)) { exit 1 }
        Start-TestDeployment
    }
    "prod" {
        if (-not (Test-Docker)) { exit 1 }
        if (-not (Initialize-Environment)) { exit 1 }
        Start-ProductionDeployment
    }
    "check" {
        if (-not (Test-Docker)) { exit 1 }
        Test-Application
    }
    "stop" {
        Stop-Services
    }
    "restart" {
        Restart-Services
    }
    "logs" {
        Show-Logs
    }
    "help" {
        Show-Help
    }
    default {
        Write-Error "Unknown command: $Command"
        Show-Help
        exit 1
    }
}
