# User Service

User Management Service for SPM System - .NET 8

## 📋 Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Design Patterns](#design-patterns)
- [Architectural Principles](#architectural-principles)
- [API Endpoints](#api-endpoints)
- [Database Migrations](#database-migrations)
- [Configuration](#configuration)
- [Running Locally](#running-locally)
- [Troubleshooting](#troubleshooting)

---

## ✨ Features

- User registration and authentication
- JWT-based authentication (access + refresh tokens)
- Email verification
- Role-based authorization (Admin/PM/Member)
- BCrypt password hashing
- Kafka event publishing

---

## 🏗️ Architecture

This microservice follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────────┐
│         Controllers (API Layer)         │
│     - Handles HTTP requests/responses   │
│     - Request validation                │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│         Services (Business Logic)       │
│     - Password hashing                  │
│     - JWT token generation              │
│     - Kafka event publishing            │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│      Repositories (Data Access)         │
│     - Database operations               │
│     - Entity queries                    │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│       Data (Entity Framework Core)       │
│     - DbContext                          │
│     - Database schema configuration      │
└─────────────────────────────────────────┘
```

---

## 📁 Project Structure

```
user-service/
├── Controllers/              # API Controllers
│   └── AuthController.cs    # Authentication endpoints
│
├── Services/                 # Business Logic Layer
│   ├── IPasswordService.cs  # Password hashing interface
│   ├── PasswordService.cs   # BCrypt implementation
│   ├── ITokenService.cs     # JWT token interface
│   ├── TokenService.cs      # JWT generation & validation
│   ├── IKafkaProducerService.cs
│   └── KafkaProducerService.cs # Event publishing
│
├── Repositories/             # Data Access Layer
│   ├── IUserRepository.cs
│   ├── UserRepository.cs
│   ├── IEmailVerificationRepository.cs
│   ├── EmailVerificationRepository.cs
│   ├── IRefreshTokenRepository.cs
│   └── RefreshTokenRepository.cs
│
├── Models/                  # Domain Entities
│   ├── User.cs
│   ├── EmailVerification.cs
│   └── RefreshToken.cs
│
├── Data/                    # Data Access Configuration
│   └── UserDbContext.cs     # EF Core DbContext
│
├── DTOs/                    # Data Transfer Objects
│   └── AuthRequest.cs       # Request/Response models
│
├── Validators/              # Input Validation (Future)
│
├── Program.cs               # Application entry point
├── appsettings.json        # Configuration
└── Dockerfile              # Container definition
```

### **Layer Responsibilities:**

| Layer            | Responsibility                          | Dependencies             |
| ---------------- | --------------------------------------- | ------------------------ |
| **Controllers**  | HTTP handling, request/response mapping | Services, DTOs           |
| **Services**     | Business logic, domain operations       | Repositories, Models     |
| **Repositories** | Data persistence, queries               | Data (DbContext), Models |
| **Models**       | Domain entities, business objects       | None                     |
| **Data**         | ORM configuration, database setup       | Models                   |

---

## 🎨 Design Patterns

### **1. Repository Pattern**

**Purpose:** Abstraction layer between business logic and data access.

**Implementation:**

- Interfaces (`IUserRepository`, `IEmailVerificationRepository`, `IRefreshTokenRepository`)
- Concrete implementations (`UserRepository`, `EmailVerificationRepository`, `RefreshTokenRepository`)

**Benefits:**

- ✅ Easy to mock for unit testing
- ✅ Decouples controllers from EF Core
- ✅ Centralized data access logic
- ✅ Flexible to swap data sources

**Example:**

```csharp
// Interface
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
}

// Usage in Controller
public AuthController(IUserRepository userRepository)
{
    _userRepository = userRepository;
}

// Usage
var user = await _userRepository.GetByEmailAsync(email);
```

### **2. Dependency Injection (DI)**

**Purpose:** Inversion of Control (IoC) for loose coupling.

**Implementation:**

- All dependencies registered in `Program.cs`
- Constructor injection used throughout
- Lifetime: `Scoped` (per HTTP request)

**Benefits:**

- ✅ Easy to test (mock dependencies)
- ✅ Loose coupling between components
- ✅ Centralized configuration

**Example:**

```csharp
// Registration in Program.cs
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Injection in Controller
public AuthController(
    IUserRepository userRepository,
    ITokenService tokenService)
{
    _userRepository = userRepository;
    _tokenService = tokenService;
}
```

### **3. Service Layer Pattern**

**Purpose:** Encapsulate business logic separate from data access.

**Implementation:**

- Business operations in `Services/` folder
- Repositories handle data, Services handle logic
- Controllers coordinate between services

**Example Flow:**

```
Controller → Service (business logic) → Repository (data access) → Database
```

### **4. DTO Pattern**

**Purpose:** Separate API contracts from domain models.

**Implementation:**

- `DTOs/` folder contains request/response models
- Domain entities never exposed directly to API
- Mapping between DTOs and entities in controllers

**Example:**

```csharp
// DTO (API Contract)
public class RegisterRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

// Domain Model (Internal)
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
}
```

---

## 🎯 Architectural Principles

### **1. SOLID Principles**

#### **S - Single Responsibility Principle**

- Each class has one reason to change
- `PasswordService` only handles password operations
- `TokenService` only handles JWT operations
- `UserRepository` only handles user data access

#### **O - Open/Closed Principle**

- Open for extension, closed for modification
- Interfaces allow adding new implementations without changing existing code
- Example: Can add `InMemoryUserRepository` without changing controllers

#### **L - Liskov Substitution Principle**

- Derived classes can replace base classes
- Repository implementations are interchangeable via interfaces

#### **I - Interface Segregation Principle**

- Clients shouldn't depend on methods they don't use
- Separate interfaces: `IUserRepository`, `IEmailVerificationRepository`, etc.
- No fat interfaces

#### **D - Dependency Inversion Principle**

- Depend on abstractions, not concretions
- Controllers depend on `IUserRepository`, not `UserRepository`
- Services depend on `ITokenService`, not `TokenService`

### **2. Clean Architecture**

**Dependency Rule:** Dependencies point inward

```
Controllers → Services → Repositories → Data
     ↓           ↓           ↓          ↓
  (Outer)    (Middle)    (Inner)   (Inner)
```

**Benefits:**

- ✅ Framework independence (can swap EF Core, ASP.NET Core)
- ✅ Testability (each layer testable independently)
- ✅ UI independence (business logic doesn't depend on API)
- ✅ Database independence (can change from PostgreSQL to MongoDB)

### **3. Separation of Concerns**

**Clear boundaries between:**

- **Presentation** (Controllers) - HTTP handling
- **Business Logic** (Services) - Domain rules
- **Data Access** (Repositories) - Database operations
- **Data Model** (Models) - Domain entities

### **4. Domain-Driven Design (DDD) Concepts**

- **Entities:** `User`, `EmailVerification`, `RefreshToken`
- **Value Objects:** Email validation, token generation
- **Aggregates:** User as aggregate root
- **Repositories:** Abstraction for aggregates

---

## 🔌 API Endpoints

### Authentication

| Method | Endpoint                 | Description          | Auth Required |
| ------ | ------------------------ | -------------------- | ------------- |
| `POST` | `/api/auth/register`     | Register new user    | ❌            |
| `POST` | `/api/auth/login`        | User login           | ❌            |
| `POST` | `/api/auth/verify-email` | Verify email address | ❌            |
| `POST` | `/api/auth/refresh`      | Refresh access token | ❌            |

### Request/Response Examples

**Register:**

```json
POST /api/auth/register
{
  "email": "user@example.com",
  "password": "password123",
  "fullName": "John Doe"
}
```

**Login:**

```json
POST /api/auth/login
{
  "email": "user@example.com",
  "password": "password123"
}

Response:
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "base64token...",
  "expiresAt": "2025-10-28T12:00:00Z",
  "user": {
    "id": "uuid",
    "email": "user@example.com",
    "role": "Member"
  }
}
```

---

## 📮 Postman Collection

Để test các API endpoints, bạn có thể sử dụng Postman collection đã được chuẩn bị sẵn.

### Files

- `SPM-User-Service.postman_collection.json` - Collection chứa tất cả các API endpoints
- `SPM-User-Service.postman_environment.json` - Environment variables cho local development
- `POSTMAN_GUIDE.md` - Hướng dẫn chi tiết cách sử dụng

### Quick Start

1. Import collection và environment vào Postman
2. Chọn environment **"SPM User Service - Local"**
3. Đảm bảo User Service đang chạy tại `http://localhost:5001`
4. Bắt đầu test các endpoints

### Features

- ✅ Tự động lưu tokens vào environment variables
- ✅ Automated tests cho mỗi request
- ✅ Pre-configured request bodies
- ✅ Environment variables cho base URL và tokens

Xem chi tiết tại [POSTMAN_GUIDE.md](./POSTMAN_GUIDE.md)

---

## 🗄️ Database Migrations

### Create Migration

```bash
dotnet ef migrations add MigrationName --context UserDbContext
```

### Apply Migrations

```bash
dotnet ef database update --context UserDbContext
```

### Create Initial Migration

```bash
dotnet ef migrations add InitialCreate --context UserDbContext
```

### Database Schema

- **Schema:** `spm_user`
- **Tables:**
  - `users` - User accounts
  - `email_verifications` - Email verification tokens
  - `refresh_tokens` - JWT refresh tokens

---

## ⚙️ Configuration

### appsettings.json

File `appsettings.json` chứa cấu hình mặc định cho tất cả environments:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=spm_db;Username=spm_user;Password=spm_pass"
  },
  "JWT": {
    "SecretKey": "your-secret-key-min-32-chars",
    "Issuer": "spm-api-gateway",
    "Audience": "spm-services",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "Kafka": {
    "BootstrapServers": "kafka:9092"
  },
  "CORS": {
    "AllowedOrigins": ["http://localhost:3000", "https://localhost:3000"]
  }
}
```

### appsettings.Development.json

File `appsettings.Development.json` được sử dụng khi `ASPNETCORE_ENVIRONMENT=Development`.

**⚠️ Lưu ý**: File này đã được ignore trong `.gitignore` để:

- Tránh commit sensitive data (mặc dù là dev password)
- Cho phép mỗi developer có config riêng
- Tránh conflict khi merge

**Setup cho developer mới**:

1. Copy file example:

   ```bash
   cp appsettings.Development.json.example appsettings.Development.json
   ```

2. Hoặc tạo file mới với nội dung:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=spm_db;Username=spm_user;Password=spm_pass"
     },
     "Kafka": {
       "BootstrapServers": "localhost:29092"
     }
   }
   ```

3. Điều chỉnh connection string nếu cần (ví dụ: port khác, database name khác)

### Environment Variables

Các environment variables có thể override settings trong appsettings:

- `ASPNETCORE_ENVIRONMENT` - Development/Production/Staging
- `ConnectionStrings__DefaultConnection` - Database connection string
- `JWT__SecretKey` - JWT secret key (override từ appsettings)
- `JWT_SECRET_KEY` - JWT secret key (alternative format)
- `Kafka__BootstrapServers` - Kafka bootstrap servers
- `GEMINI_API_KEY` - Gemini API key (if needed)

---

## 🚀 Running Locally

### Prerequisites

- .NET 8 SDK
- PostgreSQL (or Docker)
- Kafka (optional, for event publishing)

### Development

```bash
# Restore dependencies
dotnet restore

# Run database migrations
dotnet ef database update --context UserDbContext

# Run service
dotnet run
```

Service will be available at `http://localhost:5001`

### Docker

```bash
# Build and run
docker-compose up -d user-service

# View logs
docker-compose logs -f user-service
```

---

## 📚 Swagger UI

When running in Development mode, Swagger UI is available at:
`http://localhost:5001/swagger`

---

## 🧪 Testing Strategy

### Unit Tests

Test each layer independently:

- **Services:** Mock repositories
- **Repositories:** Mock DbContext or use InMemory database
- **Controllers:** Mock services

### Integration Tests

Test complete flows:

- Database integration
- API endpoint integration
- Kafka event publishing

---

## 📝 Code Style

- **Naming:** PascalCase for classes, camelCase for variables
- **Async:** All I/O operations use async/await
- **Error Handling:** Try-catch with proper logging
- **Validation:** Input validation in controllers

---

## 🔄 Event-Driven Architecture

**Kafka Events Published:**

- `user.created` - When new user registers
- `user.updated` - When user information changes

**Event Schema:**

```json
{
  "userId": "uuid",
  "email": "user@example.com",
  "role": "Member",
  "timestamp": "2025-10-28T12:00:00Z"
}
```

---

## 🔒 Security

- **Password Hashing:** BCrypt with 12 salt rounds
- **JWT Tokens:** HS256 algorithm
- **Token Expiration:** 15 minutes (access), 7 days (refresh)
- **Email Verification:** Required before account activation
- **Role-Based Access:** Admin/PM/Member roles

---

## 📈 Performance Considerations

- **Connection Pooling:** EF Core handles automatically
- **Async Operations:** All database calls are async
- **Indexes:** Email, Role, IsActive fields indexed
- **Lazy Loading:** Disabled (explicit loading preferred)

---

## 🤝 Contributing

When adding new features:

1. Create interface first (I*Service, I*Repository)
2. Implement interface in appropriate layer
3. Register in `Program.cs` DI container
4. Add unit tests
5. Update this README

---

## 🔧 Troubleshooting

Nếu gặp lỗi khi setup hoặc chạy service, xem [TROUBLESHOOTING.md](./docs/TROUBLESHOOTING.md) để biết cách fix các lỗi thường gặp:

### Các lỗi thường gặp:

1. **Lỗi DNS "Name or service not known"**

   - Nguyên nhân: Services không ở cùng Docker network
   - Giải pháp: Thêm `networks: - spm-network` vào docker-compose.yml

2. **Thiếu tables trong schema spm_user**

   - Nguyên nhân: Chưa tạo và apply migrations
   - Giải pháp: Tạo migration và apply vào database

3. **Các lỗi khác**
   - JWT SecretKey không được config
   - Connection string null
   - CORS errors
   - Email already exists

**Documentation**:

- [QUICK_FIX.md](./docs/QUICK_FIX.md) - Hướng dẫn fix nhanh
- [TROUBLESHOOTING.md](./docs/TROUBLESHOOTING.md) - Hướng dẫn chi tiết

---

**Last Updated:** 2025-11-10

## 🏷️ Roles (Enum)

- Domain model uses an enum `UserRole` (`Admin`, `PM`, `Member`) for type-safety.
- Persistence and API expose role as string for readability and compatibility.
- EF Core mapping converts enum <-> string:

```csharp
modelBuilder.Entity<User>(entity =>
{
    entity.Property(u => u.Role)
        .HasConversion<string>()
        .HasMaxLength(20)
        .IsRequired();
    entity.ToTable(t => t.HasCheckConstraint(
        "CK_User_Role",
        "role IN ('Admin', 'PM', 'Member')"));
});
```

- JWT claim uses `ClaimTypes.Role` with `user.Role.ToString()`.
- DTOs (`UserDto`) serialize `Role` as string.

### Migration Note

- If your `users.role` column is already `TEXT/VARCHAR`, adding enum + string conversion does not require a column type change (no migration needed beyond the model snapshot). If switching from an integer-backed enum, create a migration to change the column to `TEXT`.
