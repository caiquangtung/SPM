# **Commands Documentation**

### **Hệ thống Web Quản lý Dự án và Theo dõi Tiến độ Thông minh**

**Phiên bản:** 1.0

**Ngày:** 28/10/2025

---

## **📋 Mục lục**

1. [Docker Commands](#1-docker-commands)
2. [.NET Commands](#2-net-commands)
3. [Frontend Commands](#3-frontend-commands)
4. [Python/AI Service Commands](#4-pythonai-service-commands)
5. [Database Commands](#5-database-commands)
6. [Git Commands](#6-git-commands)
7. [Development Workflow](#7-development-workflow)

---

## **1. Docker Commands**

### **Khởi động services**

```bash
# Khởi động tất cả services
docker-compose up -d

# Khởi động với rebuild images
docker-compose up -d --build

# Khởi động một service cụ thể
docker-compose up -d postgres
docker-compose up -d kafka
docker-compose up -d user-service
```

### **Dừng services**

```bash
# Dừng tất cả services
docker-compose down

# Dừng và xóa volumes (WARNING: xóa dữ liệu)
docker-compose down -v

# Dừng một service cụ thể
docker-compose stop user-service
```

### **Xem logs**

```bash
# Xem logs tất cả services
docker-compose logs -f

# Xem logs một service cụ thể
docker-compose logs -f user-service
docker-compose logs -f postgres
docker-compose logs -f frontend

# Xem logs 100 dòng cuối
docker-compose logs --tail=100 user-service
```

### **Kiểm tra trạng thái**

```bash
# Xem trạng thái tất cả containers
docker-compose ps

# Kiểm tra health của services
docker-compose ps | grep healthy

# Xem resource usage
docker stats
```

### **Rebuild images**

```bash
# Rebuild một service
docker-compose build user-service

# Rebuild tất cả services
docker-compose build --no-cache

# Rebuild và restart
docker-compose up -d --build user-service
```

### **Truy cập container**

```bash
# Exec vào container
docker-compose exec user-service /bin/bash
docker-compose exec postgres psql -U spm_user -d spm_db

# Chạy command trong container
docker-compose exec postgres psql -U spm_user -d spm_db -c "SELECT * FROM spm_user.users;"
```

---

## **2. .NET Commands**

### **User Service**

```bash
# Di chuyển vào thư mục service
cd services/user-service

# Restore dependencies
dotnet restore

# Build project
dotnet build

# Build với Release mode
dotnet build -c Release

# Chạy service (development)
dotnet run

# Chạy với watch mode (auto-reload)
dotnet watch run

# Chạy với configuration cụ thể
dotnet run --environment Development
```

### **Entity Framework Core Migrations**

```bash
# Cài đặt EF Core tools (chỉ cần một lần)
dotnet tool install --global dotnet-ef

# Tạo migration mới
dotnet ef migrations add MigrationName --context UserDbContext

# Tạo migration ban đầu
dotnet ef migrations add InitialCreate --context UserDbContext

# Apply migrations vào database
dotnet ef database update --context UserDbContext

# Xem migrations đã tạo
dotnet ef migrations list --context UserDbContext

# Xóa migration cuối cùng (chưa apply)
dotnet ef migrations remove --context UserDbContext

# Tạo SQL script từ migrations
dotnet ef migrations script --context UserDbContext

# Tạo SQL script từ migration cụ thể
dotnet ef migrations script --from Migration1 --to Migration2 --context UserDbContext
```

### **Testing**

```bash
# Chạy unit tests
dotnet test

# Chạy tests với coverage
dotnet test /p:CollectCoverage=true

# Chạy tests với verbose output
dotnet test --verbosity detailed

# Chạy tests trong một project cụ thể
dotnet test tests/user-service.tests
```

### **Publishing**

```bash
# Publish cho production
dotnet publish -c Release -o ./publish

# Publish cho Linux
dotnet publish -c Release -r linux-x64 -o ./publish

# Publish cho Docker
dotnet publish -c Release -o ./publish
```

---

## **3. Frontend Commands**

### **Next.js Development**

```bash
# Di chuyển vào thư mục frontend
cd frontend

# Cài đặt dependencies
yarn install

# Hoặc với npm
npm install

# Chạy development server
yarn dev

# Build cho production
yarn build

# Chạy production server
yarn start

# Type checking
yarn type-check

# Linting
yarn lint

# Fix linting issues
yarn lint --fix
```

### **Package Management**

```bash
# Thêm package mới
yarn add package-name

# Thêm dev dependency
yarn add -D package-name

# Xóa package
yarn remove package-name

# Update packages
yarn upgrade

# Update package cụ thể
yarn upgrade package-name@latest
```

---

## **4. Python/AI Service Commands**

### **Setup Virtual Environment**

```bash
# Di chuyển vào thư mục AI service
cd services/ai-service

# Tạo virtual environment
python3 -m venv venv

# Activate virtual environment (macOS/Linux)
source venv/bin/activate

# Activate virtual environment (Windows)
venv\Scripts\activate

# Deactivate
deactivate
```

### **Dependencies Management**

```bash
# Cài đặt dependencies
pip install -r requirements.txt

# Cài đặt với production dependencies
pip install -r requirements.txt --upgrade

# Cài đặt package mới và cập nhật requirements.txt
pip install package-name
pip freeze > requirements.txt

# Cập nhật tất cả packages
pip list --outdated
pip install --upgrade package-name
```

### **Running AI Service**

```bash
# Chạy với uvicorn (development)
uvicorn main:app --reload

# Chạy với host và port cụ thể
uvicorn main:app --host 0.0.0.0 --port 8000

# Chạy với production settings
uvicorn main:app --host 0.0.0.0 --port 8000 --workers 4
```

### **Testing & Linting**

```bash
# Chạy tests với pytest
pytest

# Chạy tests với coverage
pytest --cov=. --cov-report=html

# Linting với flake8
flake8 .

# Format code với black
black .

# Check formatting
black --check .
```

---

## **5. Database Commands**

### **PostgreSQL Connection**

```bash
# Kết nối từ host machine
psql -h localhost -p 5432 -U spm_user -d spm_db

# Kết nối từ Docker container
docker-compose exec postgres psql -U spm_user -d spm_db

# Kết nối với password prompt
PGPASSWORD=spm_pass psql -h localhost -p 5432 -U spm_user -d spm_db
```

### **Database Operations**

```sql
-- Xem tất cả schemas
\dn

-- Xem tables trong schema
\dt spm_user.*

-- Xem table structure
\d spm_user.users

-- Xem data
SELECT * FROM spm_user.users;

-- Xem indexes
\di spm_user.*

-- Xem extensions
\dx
```

### **Database Backup & Restore**

```bash
# Backup database
docker-compose exec postgres pg_dump -U spm_user spm_db > backup.sql

# Restore database
docker-compose exec -T postgres psql -U spm_user spm_db < backup.sql

# Backup với timestamp
docker-compose exec postgres pg_dump -U spm_user spm_db > backup_$(date +%Y%m%d_%H%M%S).sql
```

### **Kafka Topics**

```bash
# List Kafka topics
docker-compose exec kafka kafka-topics --list --bootstrap-server localhost:9092

# Tạo topic mới
docker-compose exec kafka kafka-topics --create \
  --bootstrap-server localhost:9092 \
  --topic topic-name \
  --partitions 3 \
  --replication-factor 1

# Xem topic details
docker-compose exec kafka kafka-topics --describe \
  --bootstrap-server localhost:9092 \
  --topic user.created

# Consume messages từ topic
docker-compose exec kafka kafka-console-consumer \
  --bootstrap-server localhost:9092 \
  --topic user.created \
  --from-beginning

# Produce message vào topic
docker-compose exec kafka kafka-console-producer \
  --bootstrap-server localhost:9092 \
  --topic user.created
```

---

## **6. Git Commands**

### **Basic Git Operations**

```bash
# Clone repository
git clone <repository-url>
cd SPM

# Xem trạng thái
git status

# Add files
git add .
git add services/user-service/

# Commit
git commit -m "feat: implement user service"

# Push
git push origin main

# Pull latest changes
git pull origin main
```

### **Branching**

```bash
# Tạo branch mới
git checkout -b feature/user-management

# Chuyển branch
git checkout main

# Xem tất cả branches
git branch -a

# Merge branch
git merge feature/user-management

# Xóa branch
git branch -d feature/user-management
```

### **Commit Convention**

```bash
# Feature
git commit -m "feat: add user registration endpoint"

# Bug fix
git commit -m "fix: resolve JWT token expiration issue"

# Documentation
git commit -m "docs: update implementation plan"

# Refactor
git commit -m "refactor: improve password hashing service"

# Test
git commit -m "test: add unit tests for AuthController"
```

---

## **7. Development Workflow**

### **Setup Project Lần Đầu**

```bash
# 1. Clone repository
git clone <repository-url>
cd SPM

# 2. Copy environment file
cp .env.example .env
# Edit .env và set GEMINI_API_KEY, JWT_SECRET_KEY

# 3. Start infrastructure services
docker-compose up -d postgres zookeeper kafka

# 4. Wait for services to be ready (30 seconds)
sleep 30

# 5. Run database migrations
cd services/user-service
dotnet ef database update --context UserDbContext

# 6. Start all services
cd ../..
docker-compose up -d

# 7. Check logs
docker-compose logs -f
```

### **Daily Development**

```bash
# 1. Pull latest changes
git pull origin main

# 2. Start infrastructure (if not running)
docker-compose up -d postgres zookeeper kafka

# 3. Run service locally (development mode)
cd services/user-service
dotnet watch run

# Hoặc cho frontend
cd frontend
yarn dev
```

### **Testing Workflow**

```bash
# 1. Start test database
docker-compose up -d postgres-test

# 2. Run migrations for test DB
dotnet ef database update --context UserDbContext --connection "..."

# 3. Run tests
dotnet test

# 4. Check coverage
dotnet test /p:CollectCoverage=true
```

### **Deployment Preparation**

```bash
# 1. Build all services
docker-compose build --no-cache

# 2. Run tests
cd services/user-service && dotnet test
cd ../frontend && yarn build

# 3. Tag Docker images
docker tag spm-user-service:latest spm-user-service:v1.0.0

# 4. Push to registry (if using)
docker push spm-user-service:v1.0.0
```

---

## **8. Troubleshooting Commands**

### **Docker Issues**

```bash
# Xóa tất cả containers
docker-compose down
docker ps -a | grep spm | awk '{print $1}' | xargs docker rm

# Xóa tất cả images
docker images | grep spm | awk '{print $3}' | xargs docker rmi

# Xóa volumes
docker volume ls | grep spm | awk '{print $2}' | xargs docker volume rm

# Rebuild từ đầu
docker-compose down -v
docker-compose build --no-cache
docker-compose up -d
```

### **Database Issues**

```bash
# Reset database (WARNING: xóa tất cả dữ liệu)
docker-compose exec postgres psql -U spm_user -d spm_db -c "DROP SCHEMA spm_user CASCADE;"
docker-compose exec postgres psql -U spm_user -d spm_db -c "CREATE SCHEMA spm_user;"
dotnet ef database update --context UserDbContext
```

### **Port Conflicts**

```bash
# Kiểm tra port đang sử dụng
lsof -i :5000
lsof -i :5432
lsof -i :9092

# Kill process trên port
kill -9 $(lsof -t -i:5000)
```

### **Cache Issues**

```bash
# Clear .NET build cache
dotnet clean
rm -rf bin/ obj/

# Clear Next.js cache
cd frontend
rm -rf .next
yarn build
```

---

## **9. Useful Aliases**

Thêm vào `~/.zshrc` hoặc `~/.bashrc`:

```bash
# Docker aliases
alias dcup='docker-compose up -d'
alias dcdown='docker-compose down'
alias dclogs='docker-compose logs -f'
alias dcps='docker-compose ps'

# Project aliases
alias spm-db='docker-compose exec postgres psql -U spm_user -d spm_db'
alias spm-logs='docker-compose logs -f'
alias spm-restart='docker-compose restart'

# .NET aliases
alias dotnet-watch='dotnet watch run'
alias dotnet-test='dotnet test --verbosity normal'
```

Reload shell:

```bash
source ~/.zshrc
```

---

## **10. Environment Variables**

### **Required Variables**

```bash
# .env file
JWT_SECRET_KEY=your-super-secret-key-min-32-chars
GEMINI_API_KEY=your-gemini-api-key
POSTGRES_USER=spm_user
POSTGRES_PASSWORD=spm_pass
POSTGRES_DB=spm_db
KAFKA_BOOTSTRAP_SERVERS=localhost:9092
NEXT_PUBLIC_API_URL=http://localhost:5000
```

### **Set Variables**

```bash
# Export cho current session
export GEMINI_API_KEY=your-key

# Load từ .env file
source .env

# Hoặc sử dụng dotenv
python -m pip install python-dotenv
```

---

**END OF DOCUMENTATION**

**Updated:** 28/10/2025
