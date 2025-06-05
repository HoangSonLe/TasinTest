# Docker Setup for Tasin.Website

This document provides instructions for building and running the Tasin.Website application using Docker.

## Prerequisites

- Docker Desktop installed and running
- Docker Compose (included with Docker Desktop)
- PowerShell (for Windows scripts)

## Quick Start

### Using Docker Compose (Recommended)

1. **Start the application with database:**
   ```bash
   docker-compose up -d
   ```

2. **Access the application:**
   - Web Application: http://localhost:8080
   - PostgreSQL Database: localhost:5434

3. **Stop the application:**
   ```bash
   docker-compose down
   ```

### Using PowerShell Scripts

1. **Build the Docker image:**
   ```powershell
   .\docker-build.ps1
   ```

2. **Run the container:**
   ```powershell
   .\docker-run.ps1
   ```

## Manual Docker Commands

### Build Image
```bash
docker build -f src/Tasin.Website/Dockerfile -t tasin-website:latest . --no-cache
```

### Run Container
```bash
docker run -d \
  --name tasin-website \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -v tasin-files:/app/Files \
  -v tasin-logs:/app/logs \
  tasin-website:latest
```

## Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Application environment | `Production` |
| `ASPNETCORE_URLS` | Application URLs | `http://+:8080` |
| `ConnectionStrings__TasinDB` | Database connection string | PostgreSQL connection |
| `JWT__SecretKey` | JWT secret key | Auto-generated |
| `TelegramToken` | Telegram bot token | Empty |
| `EmailSettings__SmtpUser` | SMTP username | Empty |
| `EmailSettings__SmtpPass` | SMTP password | Empty |
| `UrlWeb` | Application base URL | `http://localhost:8080/` |

### Volumes

| Volume | Purpose | Container Path |
|--------|---------|----------------|
| `app_files` | File uploads | `/app/Files` |
| `app_files_low_quality` | Low quality images | `/app/FilesWithLowQuality` |
| `app_logs` | Application logs | `/app/logs` |
| `postgres_data` | Database data | `/var/lib/postgresql/data` |

## Development

### Building for Different Platforms
```bash
docker build --platform linux/amd64 -f src/Tasin.Website/Dockerfile -t tasin-website:latest .
```

### Viewing Logs
```bash
# Application logs
docker logs tasin-web

# Database logs
docker logs tasin-postgres
```

### Debugging
```bash
# Access container shell
docker exec -it tasin-web /bin/bash

# Check application status
docker exec tasin-web curl -f http://localhost:8080/
```

## Production Deployment

### Using Docker Compose
1. Update environment variables in `docker-compose.yml`
2. Set production database credentials
3. Configure external volumes for data persistence
4. Set up reverse proxy (nginx/traefik) for SSL termination

### Security Considerations
- Change default database passwords
- Use secrets management for sensitive data
- Run containers with non-root user (already configured)
- Enable firewall rules for container ports
- Regular security updates for base images

## Troubleshooting

### Common Issues

1. **Port already in use:**
   ```bash
   docker-compose down
   # Or change port in docker-compose.yml
   ```

2. **Database connection failed:**
   - Check if PostgreSQL container is running
   - Verify connection string
   - Check network connectivity between containers

3. **File permission issues:**
   - Ensure volumes have correct permissions
   - Check if appuser has access to mounted directories

4. **Build failures:**
   - Clear Docker cache: `docker system prune -a`
   - Check .dockerignore file
   - Verify all required files are copied

### Health Checks
```bash
# Check container health
docker ps

# Check application health
curl http://localhost:8080/

# Check database health
docker exec tasin-postgres pg_isready -U postgres
```

## Performance Optimization

- Use multi-stage builds (already implemented)
- Minimize image layers
- Use .dockerignore to exclude unnecessary files
- Consider using Alpine-based images for smaller size
- Implement proper caching strategies
