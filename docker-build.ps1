# Docker build script for Tasin.Website
param(
    [string]$Tag = "tasin-website:latest",
    [string]$Platform = "linux/amd64",
    [switch]$Push = $false,
    [string]$Registry = ""
)

Write-Host "Building Docker image for Tasin.Website..." -ForegroundColor Green

# Build the Docker image
$buildCommand = "docker build -f src/Tasin.Website/Dockerfile -t $Tag"

if ($Platform) {
    $buildCommand += " --platform $Platform"
}

$buildCommand += " ."

Write-Host "Executing: $buildCommand" -ForegroundColor Yellow
Invoke-Expression $buildCommand

if ($LASTEXITCODE -eq 0) {
    Write-Host "Docker image built successfully: $Tag" -ForegroundColor Green
    
    # Show image info
    docker images $Tag
    
    if ($Push -and $Registry) {
        $registryTag = "$Registry/$Tag"
        Write-Host "Tagging image for registry: $registryTag" -ForegroundColor Yellow
        docker tag $Tag $registryTag
        
        Write-Host "Pushing image to registry..." -ForegroundColor Yellow
        docker push $registryTag
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Image pushed successfully to registry!" -ForegroundColor Green
        } else {
            Write-Host "Failed to push image to registry!" -ForegroundColor Red
            exit 1
        }
    }
} else {
    Write-Host "Docker build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Build completed!" -ForegroundColor Green
