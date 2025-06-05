# Docker run script for Tasin.Website
param(
    [string]$Tag = "tasin-website:latest",
    [string]$ContainerName = "tasin-website",
    [int]$Port = 8080,
    [string]$Environment = "Production",
    [switch]$Detached = $true,
    [switch]$Remove = $true
)

Write-Host "Running Docker container for Tasin.Website..." -ForegroundColor Green

# Stop and remove existing container if it exists
$existingContainer = docker ps -a -q -f name=$ContainerName
if ($existingContainer) {
    Write-Host "Stopping and removing existing container: $ContainerName" -ForegroundColor Yellow
    docker stop $ContainerName
    docker rm $ContainerName
}

# Build run command
$runCommand = "docker run"

if ($Remove) {
    $runCommand += " --rm"
}

if ($Detached) {
    $runCommand += " -d"
}

$runCommand += " --name $ContainerName"
$runCommand += " -p ${Port}:8080"
$runCommand += " -e ASPNETCORE_ENVIRONMENT=$Environment"

# Add volume mounts for persistent data
$runCommand += " -v tasin-files:/app/Files"
$runCommand += " -v tasin-files-low-quality:/app/FilesWithLowQuality"
$runCommand += " -v tasin-logs:/app/logs"

$runCommand += " $Tag"

Write-Host "Executing: $runCommand" -ForegroundColor Yellow
Invoke-Expression $runCommand

if ($LASTEXITCODE -eq 0) {
    Write-Host "Container started successfully!" -ForegroundColor Green
    Write-Host "Container Name: $ContainerName" -ForegroundColor Cyan
    Write-Host "Port: $Port" -ForegroundColor Cyan
    Write-Host "Environment: $Environment" -ForegroundColor Cyan
    Write-Host "Access URL: http://localhost:$Port" -ForegroundColor Cyan
    
    if ($Detached) {
        Write-Host "`nContainer is running in detached mode." -ForegroundColor Yellow
        Write-Host "Use 'docker logs $ContainerName' to view logs" -ForegroundColor Yellow
        Write-Host "Use 'docker stop $ContainerName' to stop the container" -ForegroundColor Yellow
    }
} else {
    Write-Host "Failed to start container!" -ForegroundColor Red
    exit 1
}

Write-Host "`nRun completed!" -ForegroundColor Green
