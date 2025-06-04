#!/usr/bin/env pwsh

# Tasin Quick Update Script
# This script quickly rebuilds and restarts the application after code changes

param(
    [string]$Mode = "quick"  # quick, full, test
)

Write-Host "=== Tasin Update Process ===" -ForegroundColor Green

function Quick-Update {
    Write-Host "Quick update: Rebuilding container with new code..." -ForegroundColor Blue
    
    # Stop current container
    Write-Host "Stopping current container..." -ForegroundColor Yellow
    docker stop tasin-web 2>$null
    docker rm tasin-web 2>$null
    
    # Build new image
    Write-Host "Building new image..." -ForegroundColor Yellow
    docker build -t tasin-web .
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed!" -ForegroundColor Red
        return $false
    }
    
    # Start new container
    Write-Host "Starting new container..." -ForegroundColor Yellow
    docker run -d --name tasin-web -p 8080:8080 `
        -e ASPNETCORE_ENVIRONMENT=Production `
        -e "ConnectionStrings__TasinDB=Server=host.docker.internal;Port=5434;Database=Tasin;User Id=postgres;Password=123456;" `
        tasin-web
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to start container!" -ForegroundColor Red
        return $false
    }
    
    Write-Host "Waiting for application to start..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10
    
    # Quick health check
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8080" -TimeoutSec 10
        Write-Host "Application updated successfully!" -ForegroundColor Green
        Write-Host "Available at: http://localhost:8080" -ForegroundColor Cyan
        return $true
    } catch {
        Write-Host "Application may still be starting up..." -ForegroundColor Yellow
        Write-Host "Check manually: http://localhost:8080" -ForegroundColor Cyan
        return $true
    }
}

function Full-Update {
    Write-Host "Full update: Rebuilding with Docker Compose..." -ForegroundColor Blue
    
    # Stop all services
    Write-Host "Stopping all services..." -ForegroundColor Yellow
    docker-compose down
    
    # Rebuild with no cache
    Write-Host "Rebuilding images..." -ForegroundColor Yellow
    docker-compose build --no-cache
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed!" -ForegroundColor Red
        return $false
    }
    
    # Start services
    Write-Host "Starting services..." -ForegroundColor Yellow
    docker-compose up -d
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to start services!" -ForegroundColor Red
        return $false
    }
    
    Write-Host "Waiting for services to be ready..." -ForegroundColor Yellow
    Start-Sleep -Seconds 15
    
    Write-Host "Full update completed!" -ForegroundColor Green
    Write-Host "Available at: http://localhost:8080" -ForegroundColor Cyan
    return $true
}

function Test-Update {
    Write-Host "Test update: Building and testing..." -ForegroundColor Blue
    
    # Use existing build-and-test script
    .\build-and-test.ps1 test
    
    return $LASTEXITCODE -eq 0
}

# Main logic
switch ($Mode.ToLower()) {
    "quick" {
        if (Quick-Update) {
            Write-Host "Quick update successful!" -ForegroundColor Green
        } else {
            Write-Host "Quick update failed!" -ForegroundColor Red
            exit 1
        }
    }
    "full" {
        if (Full-Update) {
            Write-Host "Full update successful!" -ForegroundColor Green
        } else {
            Write-Host "Full update failed!" -ForegroundColor Red
            exit 1
        }
    }
    "test" {
        if (Test-Update) {
            Write-Host "Test update successful!" -ForegroundColor Green
        } else {
            Write-Host "Test update failed!" -ForegroundColor Red
            exit 1
        }
    }
    default {
        Write-Host "Invalid mode. Available modes:" -ForegroundColor Red
        Write-Host "  .\update.ps1 quick  # Quick rebuild and restart" -ForegroundColor White
        Write-Host "  .\update.ps1 full   # Full rebuild with Docker Compose" -ForegroundColor White
        Write-Host "  .\update.ps1 test   # Build and test thoroughly" -ForegroundColor White
        exit 1
    }
}

Write-Host ""
Write-Host "Useful commands after update:" -ForegroundColor Cyan
Write-Host "  docker logs tasin-web           # View logs" -ForegroundColor White
Write-Host "  .\deploy.ps1 check             # Check application health" -ForegroundColor White
Write-Host "  http://localhost:8080          # Access application" -ForegroundColor White
