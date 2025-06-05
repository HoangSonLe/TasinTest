# Tasin - Hệ thống quản lý đơn hàng

Hệ thống quản lý đơn hàng được xây dựng bằng ASP.NET Core 8.0 với PostgreSQL.

## 📋 Yêu cầu hệ thống

- .NET 8.0 SDK
- PostgreSQL 16
- Docker & Docker Compose (cho deployment)

## 🚀 Cài đặt và triển khai

### 1. Development (Local)

```bash
# Clone repository
git clone <repository-url>
cd Tasin

# Restore packages
dotnet restore src/Tasin.Website/

# Cập nhật connection string trong appsettings.json
# Chạy database migrations (nếu cần)

# Chạy ứng dụng
dotnet run --project src/Tasin.Website/
```

Ứng dụng sẽ chạy tại: http://localhost:5000

### 2. Test với Docker (Standalone)

```bash
# Build Docker image
docker build -t tasin-web .
# Sử dụng Dockerfile trong thư mục src/Tasin.Website/
docker build -f src/Tasin.Website/Dockerfile -t tasin-web .

# Chạy container (cần PostgreSQL external)
docker run -d --name tasin-web -p 8080:8080
  -e ASPNETCORE_ENVIRONMENT=Production
  -e "ConnectionStrings__TasinDB=Server=host.docker.internal;Port=5434;Database=Tasin;User Id=postgres;Password=123456;"
  tasin-web
```
```bash
docker run -d --name tasin-web -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Production -e "ConnectionStrings__TasinDB=Server=ep-white-leaf-a85po2ip-pooler.eastus2.azure.neon.tech;Port=5432;Database=neondb;User Id=neondb_owner;Password=npg_iZt7zRJPInk3;Ssl Mode=Require;Trust Server Certificate=true;Options=endpoint=ep-white-leaf-a85po2ip-pooler;" tasin-web
```

Ứng dụng sẽ chạy tại: http://localhost:8080

### 3. Production (Docker Compose)

```bash
# Chạy toàn bộ stack
docker-compose up -d

# Xem logs
docker-compose logs -f tasin-web

# Dừng services
docker-compose down
```

Ứng dụng sẽ chạy tại: http://localhost:8080

### 4. Production (Linux Server)

```bash
# 1. Cài đặt Docker và Docker Compose trên server

# 2. Clone code
git clone <repository-url>
cd Tasin

# 3. Cấu hình environment variables (tùy chọn)
cp .env.example .env
# Chỉnh sửa .env với thông tin production

# 4. Deploy
docker-compose up -d

# 5. Kiểm tra health
curl http://localhost:8080/health
```

## 🔧 Cấu hình

### Environment Variables

Tạo file `.env` trong thư mục gốc:

```env
# Database
DB_HOST=localhost
DB_PORT=5434
DB_NAME=Tasin
DB_USER=postgres
DB_PASSWORD=your_secure_password

# JWT
JWT_SECRET_KEY=your_jwt_secret_key

# Telegram Bot (optional)
TELEGRAM_TOKEN=your_telegram_bot_token

# Email (optional)
SMTP_USER=your_smtp_user
SMTP_PASS=your_smtp_password

# App URL
APP_URL=https://your-domain.com/
```

### Cấu hình Database

1. **PostgreSQL External**: Cập nhật connection string trong `appsettings.Production.json`
2. **Docker Compose**: Database sẽ được tự động tạo với sample data

## 📁 Cấu trúc dự án

```
Tasin/
├── src/Tasin.Website/          # Main web application
│   ├── Controllers/            # MVC Controllers
│   ├── Views/                  # Razor Views
│   ├── wwwroot/               # Static files (CSS, JS, images)
│   ├── Migrations/            # Database scripts
│   ├── Models/                # View models
│   ├── DAL/                   # Data Access Layer
│   └── Common/                # Shared utilities
├── Dockerfile                 # Docker build configuration
├── docker-compose.yml         # Docker Compose configuration
├── build-and-test.ps1        # Build and test script
└── deploy.ps1               # Deployment script
```

## ✨ Tính năng chính

- 👥 **Quản lý người dùng và phân quyền**
- 🏢 **Quản lý nhà cung cấp và khách hàng**
- 📦 **Quản lý sản phẩm và danh mục**
- 📋 **Quản lý đơn hàng và đơn tổng hợp**
- 📊 **Báo cáo thống kê**
- 📱 **Thông báo Telegram**
- 🔐 **Bảo mật và authentication**

## 🛠️ Scripts hỗ trợ

### Windows PowerShell

```powershell
# Quick test (khuyến nghị cho development)
.\deploy.ps1 test

# Full deployment với Docker Compose
.\deploy.ps1 deploy

# Production deployment
.\deploy.ps1 prod

# Build và test chi tiết
.\build-and-test.ps1

# Kiểm tra application đang chạy
.\deploy.ps1 check

# Xem logs
.\deploy.ps1 logs

# Dừng services
.\deploy.ps1 stop
```

### Linux/Mac

```bash
# Build image
docker build -t tasin-web .

# Test container
docker run -d --name tasin-web-test -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e "ConnectionStrings__TasinDB=Server=your-db-host;Port=5432;Database=Tasin;User Id=postgres;Password=your-password;" \
  tasin-web

# Check health
curl http://localhost:8080

# Docker Compose deployment
docker-compose up -d

# View logs
docker-compose logs -f tasin-web
```

## � Cập nhật code

### **Sau khi sửa code, bạn có 3 cách cập nhật:**

#### **1. Quick Update (Khuyến nghị cho development)**
```powershell
# Rebuild và restart nhanh
.\update.ps1 quick

# Hoặc manual
docker stop tasin-web && docker rm tasin-web
docker build -t tasin-web .

docker build -f src/Tasin.Website/Dockerfile -t tasin-web .

docker run -d --name tasin-web -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e "ConnectionStrings__TasinDB=Server=host.docker.internal;Port=5434;Database=Tasin;User Id=postgres;Password=123456;" \
  tasin-web
```

#### **2. Full Update (Cho production)**
```powershell
# Rebuild toàn bộ với Docker Compose
.\update.ps1 full

# Hoặc manual
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

#### **3. Test Update (Với testing đầy đủ)**
```powershell
# Build và test kỹ lưỡng
.\update.ps1 test

# Hoặc
.\build-and-test.ps1 test
```

### **Hot Reload cho Development**
```powershell
# Nếu chỉ sửa Views/CSS/JS (không cần rebuild)
# Copy files trực tiếp vào container
docker cp src/Tasin.Website/Views/ tasin-web:/app/Views/
docker cp src/Tasin.Website/wwwroot/ tasin-web:/app/wwwroot/
```

## �🔍 Troubleshooting

### Lỗi thường gặp

1. **Container không start**: Kiểm tra logs với `docker logs tasin-web`
2. **Database connection**: Đảm bảo PostgreSQL đang chạy và connection string đúng
3. **Static files 404**: Kiểm tra file paths có đúng case sensitivity không
4. **Permission denied**: Đảm bảo user có quyền truy cập files và folders
5. **Code không update**: Đảm bảo đã rebuild image với `--no-cache`

### Health Check

```bash
# Kiểm tra container status
docker ps

# Kiểm tra application health
curl http://localhost:8080

# Xem logs
docker logs tasin-web --tail 50

# Kiểm tra với script
.\deploy.ps1 check
```

## 📝 License

[Thêm thông tin license]
