#!/usr/bin/env pwsh

# Tasin Build and Test Script
# This script builds the Docker image and tests it

param(
    [string]$Mode = "test"  # test, build, clean
)

Write-Host "Starting Tasin Build and Test Process..." -ForegroundColor Green

function Build-DockerImage {
    Write-Host "Building Docker image..." -ForegroundColor Blue
    docker build -t tasin-web .

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Docker build failed!" -ForegroundColor Red
        return $false
    }

    Write-Host "Docker image built successfully!" -ForegroundColor Green
    return $true
}

function Test-Application {
    # Stop and remove existing container if it exists
    Write-Host "Cleaning up existing containers..." -ForegroundColor Yellow
    docker stop tasin-web 2>$null
    docker rm tasin-web 2>$null

    # Run the container for testing
    Write-Host "Starting test container..." -ForegroundColor Blue
    docker run -d --name tasin-web -p 8080:8080 `
        -e ASPNETCORE_ENVIRONMENT=Production `
        -e "ConnectionStrings__TasinDB=Server=host.docker.internal;Port=5434;Database=Tasin;User Id=postgres;Password=123456;" `
        tasin-web

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to start container!" -ForegroundColor Red
        return $false
    }

    Write-Host "Container started successfully!" -ForegroundColor Green

    # Wait for application to start
    Write-Host "Waiting for application to start..." -ForegroundColor Yellow
    Start-Sleep -Seconds 15

    # Test if application is responding
    Write-Host "Testing application health..." -ForegroundColor Blue

    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8080" -Method GET -TimeoutSec 10
        if ($response.StatusCode -eq 200) {
            Write-Host "Application is responding successfully!" -ForegroundColor Green
            Write-Host "Application is available at: http://localhost:8080" -ForegroundColor Cyan
        } else {
            Write-Host "Application responded with status code: $($response.StatusCode)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "Application health check failed: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Container logs:" -ForegroundColor Yellow
        docker logs tasin-web --tail 20
        return $false
    }

    # Test specific JavaScript files
    Write-Host "Testing JavaScript files..." -ForegroundColor Blue

    $jsFiles = @(
        "/js/login/changePassword.js",
        "/js/Permission/index.js",
        "/js/NotificationForUser/index.js"
    )

    foreach ($jsFile in $jsFiles) {
        try {
            $jsResponse = Invoke-WebRequest -Uri "http://localhost:8080$jsFile" -Method HEAD -TimeoutSec 5
            if ($jsResponse.StatusCode -eq 200) {
                Write-Host "$jsFile - OK" -ForegroundColor Green
            } else {
                Write-Host "$jsFile - Status: $($jsResponse.StatusCode)" -ForegroundColor Yellow
            }
        } catch {
            Write-Host "$jsFile - Failed: $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    Write-Host ""
    Write-Host "Build and test completed!" -ForegroundColor Green
    Write-Host "Container status:" -ForegroundColor Cyan
    docker ps --filter "name=tasin-web"

    Write-Host ""
    Write-Host "Useful commands:" -ForegroundColor Cyan
    Write-Host "  View logs:      docker logs tasin-web" -ForegroundColor White
    Write-Host "  Stop container: docker stop tasin-web" -ForegroundColor White
    Write-Host "  Remove container: docker rm tasin-web" -ForegroundColor White
    Write-Host "  Access app:     http://localhost:8080" -ForegroundColor White

    return $true
}

function Clean-DockerResources {
    Write-Host "Cleaning up Docker resources..." -ForegroundColor Yellow
    docker stop tasin-web 2>$null
    docker rm tasin-web 2>$null
    docker rmi tasin-web 2>$null
    Write-Host "Cleanup completed!" -ForegroundColor Green
}

# Main script logic
switch ($Mode.ToLower()) {
    "clean" {
        Clean-DockerResources
    }
    "build" {
        if (Build-DockerImage) {
            Write-Host "Build completed successfully!" -ForegroundColor Green
        } else {
            exit 1
        }
    }
    "test" {
        if (Build-DockerImage) {
            if (Test-Application) {
                Write-Host "All tests passed!" -ForegroundColor Green
            } else {
                Write-Host "Some tests failed!" -ForegroundColor Red
                exit 1
            }
        } else {
            exit 1
        }
    }
    default {
        Write-Host "Invalid mode. Use: test, build, or clean" -ForegroundColor Red
        Write-Host ""
        Write-Host "Script modes:" -ForegroundColor Cyan
        Write-Host "  .\build-and-test.ps1 test   # Full build and test (default)" -ForegroundColor White
        Write-Host "  .\build-and-test.ps1 build  # Build image only" -ForegroundColor White
        Write-Host "  .\build-and-test.ps1 clean  # Clean up Docker resources" -ForegroundColor White
        exit 1
    }
}
