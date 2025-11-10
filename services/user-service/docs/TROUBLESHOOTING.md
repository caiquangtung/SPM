# User Service - Troubleshooting Guide

Hướng dẫn chi tiết cách fix các lỗi thường gặp khi setup và chạy User Service.

---

## 📋 Mục lục

- [Lỗi DNS "Name or service not known"](#1-lỗi-dns-name-or-service-not-known)
- [Thiếu tables trong schema spm_user](#2-thiếu-tables-trong-schema-spm_user)
- [Verification & Testing](#3-verification--testing)
- [Các lỗi khác](#4-các-lỗi-khác)

---

## 1. Lỗi DNS "Name or service not known"

### 🔴 Triệu chứng

Khi chạy User Service trong Docker, gặp lỗi:

```json
{
  "success": false,
  "message": "Name or service not known",
  "errorCode": "INTERNAL_ERROR",
  "data": "... System.Net.Dns.GetHostEntryOrAddressesCore ..."
}
```

Hoặc trong logs:

```
System.Net.Sockets.SocketException: Name or service not known
   at System.Net.Dns.GetHostEntryOrAddressesCore(String hostName, ...)
   at Npgsql.Internal.NpgsqlConnector.Connect(NpgsqlTimeout timeout)
```

### 🔍 Nguyên nhân

**Vấn đề**: Các services (`postgres`, `zookeeper`, `kafka`) và `user-service` không ở cùng một Docker network, nên không thể resolve hostname của nhau.

**Chi tiết**:

- `postgres`, `zookeeper`, `kafka`: Đang ở network mặc định (`spm_default`)
- `user-service`: Đang ở network `spm_spm-network` (được khai báo trong docker-compose.yml)
- Khi `user-service` cố gắng kết nối đến `postgres:5432`, DNS resolution fail vì chúng không ở cùng network

### ✅ Giải pháp

#### Bước 1: Kiểm tra network hiện tại

```bash
# Kiểm tra network của postgres
docker inspect spm-postgres --format='{{range $net,$v := .NetworkSettings.Networks}}{{$net}} {{end}}'

# Kiểm tra network của user-service
docker inspect spm-user-service --format='{{range $net,$v := .NetworkSettings.Networks}}{{$net}} {{end}}'
```

**Kết quả mong đợi (sai)**:

- `postgres`: `spm_default`
- `user-service`: `spm_spm-network`

**Kết quả mong đợi (đúng)**:

- Cả hai đều ở `spm_spm-network`

#### Bước 2: Cập nhật docker-compose.yml

Thêm `networks: - spm-network` vào các services: `postgres`, `zookeeper`, và `kafka`.

**File**: `docker-compose.yml`

```yaml
services:
  postgres:
    image: pgvector/pgvector:pg16
    container_name: spm-postgres
    environment:
      POSTGRES_USER: spm_user
      POSTGRES_PASSWORD: spm_pass
      POSTGRES_DB: spm_db
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./infrastructure/scripts/init.sql:/docker-entrypoint-initdb.d/init.sql
    ports:
      - "5432:5432"
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U spm_user -d spm_db"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks: # ← THÊM DÒNG NÀY
      - spm-network # ← THÊM DÒNG NÀY

  zookeeper:
    image: confluentinc/cp-zookeeper:7.5.0
    container_name: spm-zookeeper
    environment:
      ZOOKEEPER_CLIENT_PORT: 2181
      ZOOKEEPER_TICK_TIME: 2000
    ports:
      - "2181:2181"
    networks: # ← THÊM DÒNG NÀY
      - spm-network # ← THÊM DÒNG NÀY

  kafka:
    image: confluentinc/cp-kafka:7.5.0
    container_name: spm-kafka
    depends_on:
      - zookeeper
    environment:
      KAFKA_BROKER_ID: 1
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      # ... other configs
    ports:
      - "9092:9092"
      - "29092:29092"
    healthcheck:
      # ... healthcheck config
    networks: # ← THÊM DÒNG NÀY
      - spm-network # ← THÊM DÒNG NÀY

  user-service:
    build: ./services/user-service
    container_name: spm-user-service
    depends_on:
      postgres:
        condition: service_healthy
      kafka:
        condition: service_healthy
    ports:
      - "5001:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=spm_db;Username=spm_user;Password=spm_pass
      - Kafka__BootstrapServers=kafka:9092
    networks: # ← ĐÃ CÓ SẴN
      - spm-network # ← ĐÃ CÓ SẴN

# Đảm bảo network được khai báo ở cuối file
networks:
  spm-network:
    driver: bridge
```

#### Bước 3: Restart các services

```bash
# Dừng tất cả services
docker-compose down

# Khởi động lại với cấu hình mới
docker-compose up -d

# Kiểm tra status
docker-compose ps
```

#### Bước 4: Verify network connectivity

```bash
# Kiểm tra postgres có thể resolve từ user-service không
docker exec spm-user-service sh -c "getent hosts postgres"

# Kết quả mong đợi:
# 172.18.0.X      postgres

# Kiểm tra kafka có thể resolve không
docker exec spm-user-service sh -c "getent hosts kafka"

# Kết quả mong đợi:
# 172.18.0.Y      kafka
```

#### Bước 5: Kiểm tra logs

```bash
# Kiểm tra logs của user-service
docker logs spm-user-service --tail 50

# Kết quả mong đợi: Không còn lỗi DNS, service start thành công
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: http://[::]:8080
```

### ✅ Kết quả

Sau khi fix, các services sẽ:

- ✅ Cùng ở một network (`spm-network`)
- ✅ Có thể resolve hostname của nhau
- ✅ User Service có thể kết nối đến Postgres và Kafka
- ✅ Không còn lỗi DNS resolution

---

## 2. Thiếu tables trong schema spm_user

### 🔴 Triệu chứng

Khi gọi API, gặp lỗi:

```
System.InvalidOperationException: The required column 'Id' was not present in the results of a 'FromSql' operation.
```

Hoặc khi kiểm tra database:

```sql
SELECT table_name FROM information_schema.tables WHERE table_schema = 'spm_user';
-- Kết quả: (0 rows) - Không có tables nào
```

### 🔍 Nguyên nhân

**Vấn đề**: Database schema `spm_user` đã tồn tại nhưng chưa có tables vì:

1. Chưa tạo migrations
2. Chưa apply migrations vào database
3. `Program.cs` đang dùng `EnsureCreated()` thay vì `Migrate()`

**Chi tiết**:

- Schema `spm_user` đã được tạo bởi init script
- Nhưng tables (`users`, `email_verifications`, `refresh_tokens`) chưa được tạo
- EF Core cần migrations để tạo tables từ DbContext

### ✅ Giải pháp

#### Bước 1: Kiểm tra database hiện tại

```bash
# Kiểm tra schema có tồn tại không
docker exec spm-postgres psql -U spm_user -d spm_db -c "\dn"

# Kết quả mong đợi: Có schema `spm_user`

# Kiểm tra tables trong schema
docker exec spm-postgres psql -U spm_user -d spm_db -c \
  "SELECT table_name FROM information_schema.tables WHERE table_schema = 'spm_user';"

# Kết quả (sai): (0 rows) - Không có tables
# Kết quả (đúng): Có 3 tables: users, email_verifications, refresh_tokens
```

#### Bước 2: Kiểm tra migrations folder

```bash
cd services/user-service
ls -la Migrations/

# Kết quả (sai): Không có folder Migrations hoặc folder rỗng
# Kết quả (đúng): Có các file migration
```

#### Bước 3: Tạo migration

**Prerequisites**:

- Đã cài đặt .NET 8 SDK
- Đã cài đặt EF Core tools: `dotnet tool install --global dotnet-ef`

```bash
cd services/user-service

# Tạo migration
dotnet ef migrations add InitialCreate --context UserDbContext

# Kết quả mong đợi:
# Build started...
# Build succeeded.
# Done. To undo this action, use 'ef migrations remove'
```

**Files được tạo**:

```
Migrations/
  ├── 20241110163142_InitialCreate.cs
  ├── 20241110163142_InitialCreate.Designer.cs
  └── UserDbContextModelSnapshot.cs
```

#### Bước 4: Apply migration

**Option A: Apply migration từ local machine (Recommended)**

```bash
cd services/user-service

# Set environment để dùng connection string từ appsettings.Development.json
export ASPNETCORE_ENVIRONMENT=Development

# Apply migration
dotnet ef database update --context UserDbContext

# Kết quả mong đợi:
# Build started...
# Build succeeded.
# Applying migration '20241110163142_InitialCreate'.
# Done.
```

**Option B: Apply migration từ Docker container**

```bash
# Copy migration files vào container (nếu chưa có)
docker cp services/user-service/Migrations spm-user-service:/src/Migrations

# Chạy migration từ container
docker exec spm-user-service dotnet ef database update --context UserDbContext
```

#### Bước 5: Verify tables đã được tạo

```bash
# Kiểm tra tables
docker exec spm-postgres psql -U spm_user -d spm_db -c \
  "SELECT table_name FROM information_schema.tables WHERE table_schema = 'spm_user' ORDER BY table_name;"

# Kết quả mong đợi:
#      table_name
# ---------------------
#  email_verifications
#  refresh_tokens
#  users
# (3 rows)

# Kiểm tra structure của table users
docker exec spm-postgres psql -U spm_user -d spm_db -c \
  "\d spm_user.users"

# Kết quả mong đợi: Hiển thị các columns (Id, Email, PasswordHash, etc.)
```

#### Bước 6: Cập nhật Program.cs để auto-migrate (Optional)

Để tự động apply migrations khi start service (chỉ cho development):

**File**: `services/user-service/Program.cs`

```csharp
// Apply pending migrations automatically (for development)
// In production, run migrations separately using: dotnet ef database update
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            logger.LogInformation("Applying pending database migrations...");
            db.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations.");
            // Don't throw - allow service to start even if migrations fail
            // This allows for manual migration fixes
        }
    }
}
```

**Lưu ý**:

- Chỉ enable auto-migrate trong Development environment
- Trong Production, nên chạy migrations manually hoặc qua CI/CD pipeline
- Nếu migration fail, service vẫn sẽ start (không throw exception)

#### Bước 7: Rebuild và restart service

```bash
# Rebuild service với code mới
docker-compose build user-service

# Restart service
docker-compose up -d user-service

# Kiểm tra logs
docker logs spm-user-service --tail 50

# Kết quả mong đợi:
# info: Program[0]
#       Applying pending database migrations...
# info: Program[0]
#       No migrations were applied. The database is already up to date.
# info: Program[0]
#       Database migrations applied successfully.
```

### ✅ Kết quả

Sau khi fix, database sẽ có:

- ✅ Schema `spm_user` với đầy đủ tables
- ✅ Table `users` với các columns: Id, Email, PasswordHash, Role, etc.
- ✅ Table `email_verifications` với các columns: Id, UserId, Token, ExpiresAt, etc.
- ✅ Table `refresh_tokens` với các columns: Id, UserId, Token, ExpiresAt, etc.
- ✅ Indexes và foreign keys đã được tạo
- ✅ Check constraints (ví dụ: role IN ('Admin', 'PM', 'Member'))

---

## 3. Verification & Testing

### ✅ Kiểm tra kết nối database

```bash
# Test API Register
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Password123!",
    "fullName": "Test User"
  }'

# Kết quả mong đợi:
# {
#   "success": true,
#   "message": "User registered successfully. Please check your email to verify your account.",
#   "data": {
#     "userId": "uuid"
#   }
# }
```

### ✅ Kiểm tra tables đã có data

```bash
# Kiểm tra user đã được tạo
docker exec spm-postgres psql -U spm_user -d spm_db -c \
  'SELECT "Email", "EmailConfirmed", role FROM spm_user.users LIMIT 5;'

# Kết quả mong đợi:
#        Email       | EmailConfirmed |  role
# -------------------+----------------+--------
#  test@example.com | f              | Member
# (1 row)
```

### ✅ Kiểm tra API Login

```bash
# Test API Login
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Password123!"
  }'

# Kết quả mong đợi:
# {
#   "success": true,
#   "message": "Login successful",
#   "data": {
#     "accessToken": "eyJhbGc...",
#     "refreshToken": "base64token...",
#     "expiresAt": "2025-11-10T...",
#     "user": {
#       "id": "uuid",
#       "email": "test@example.com",
#       "role": "Member"
#     }
#   }
# }
```

### ✅ Kiểm tra logs

```bash
# Kiểm tra logs của user-service
docker logs spm-user-service --tail 100 | grep -E "(listening|migrations|error|Error|Exception)"

# Kết quả mong đợi:
# - Không có lỗi DNS
# - Không có lỗi database connection
# - Service đang listen trên port 8080
# - Migrations đã được apply (nếu có)
```

---

## 4. Các lỗi khác

### 🔴 Lỗi: "JWT SecretKey is not configured"

**Triệu chứng**:

```
System.InvalidOperationException: JWT SecretKey is not configured.
```

**Giải pháp**:

1. Kiểm tra `appsettings.json` có `JWT:SecretKey` không
2. Hoặc set environment variable `JWT__SecretKey`
3. Đảm bảo SecretKey có ít nhất 32 ký tự

```bash
# Set environment variable trong docker-compose.yml
environment:
  - JWT__SecretKey=your-super-secret-key-min-32-chars-change-in-production
```

### 🔴 Lỗi: "Connection string is null"

**Triệu chứng**:

```
System.ArgumentNullException: Connection string is null
```

**Giải pháp**:

1. Kiểm tra `appsettings.json` có `ConnectionStrings:DefaultConnection` không
2. Hoặc set environment variable `ConnectionStrings__DefaultConnection`

```bash
# Set environment variable trong docker-compose.yml
environment:
  - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=spm_db;Username=spm_user;Password=spm_pass
```

### 🔴 Lỗi: CORS

**Triệu chứng**:

```
Access to XMLHttpRequest at 'http://localhost:5001/api/auth/login' from origin 'http://localhost:3000' has been blocked by CORS policy
```

**Giải pháp**:

1. Kiểm tra `appsettings.json` có `CORS:AllowedOrigins` không
2. Đảm bảo frontend URL được thêm vào allowed origins

```json
{
  "CORS": {
    "AllowedOrigins": ["http://localhost:3000", "https://localhost:3000"]
  }
}
```

### 🔴 Lỗi: "Email already exists"

**Triệu chứng**:

```json
{
  "success": false,
  "message": "Email already exists",
  "errorCode": "EMAIL_EXISTS"
}
```

**Giải pháp**:

- Đây không phải lỗi hệ thống, mà là business logic validation
- Sử dụng email khác hoặc xóa user cũ từ database

```bash
# Xóa user cũ (nếu cần)
docker exec spm-postgres psql -U spm_user -d spm_db -c \
  "DELETE FROM spm_user.users WHERE \"Email\" = 'test@example.com';"
```

---

## 📚 Tham khảo

- [Docker Networking](https://docs.docker.com/network/)
- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [PostgreSQL Schemas](https://www.postgresql.org/docs/current/ddl-schemas.html)
- [User Service README](./README.md)

---

## 🔄 Checklist

Khi gặp lỗi, kiểm tra:

- [ ] Tất cả services đều ở cùng network (`spm-network`)
- [ ] Database connection string đúng
- [ ] Migrations đã được tạo và apply
- [ ] Tables đã được tạo trong schema `spm_user`
- [ ] JWT SecretKey đã được config (ít nhất 32 ký tự)
- [ ] CORS đã được config đúng
- [ ] Services đang chạy và healthy
- [ ] Logs không có lỗi

---

**Last Updated**: 2025-11-10
