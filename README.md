# SPM - Smart Project Management System

Hệ thống Web Quản lý Dự án và Theo dõi Tiến độ Thông minh

## 🏗️ Architecture

- **Frontend:** Next.js (React + TypeScript + Tailwind CSS)
- **API Gateway:** YARP (.NET 8)
- **Backend Services:** .NET 8 (ASP.NET Core)
  - User Service
  - Project Service
  - File Service
  - Notification Service
- **AI Service:** Python (FastAPI)
- **Database:** PostgreSQL 16+ với pgvector extension
- **Message Broker:** Apache Kafka
- **Real-time:** SignalR (WebSocket notifications)

## 🚀 Quick Start

### Prerequisites

- Docker & Docker Compose
- .NET 8 SDK (for local development)
- Node.js 20+ (for local development)
- Python 3.11+ (for AI service local development)

### Setup

1. **Clone repository:**

```bash
git clone <repository-url>
cd SPM
```

2. **Configure environment variables:**

```bash
cp .env.example .env
# Edit .env and set GEMINI_API_KEY and JWT_SECRET_KEY
```

3. **Start all services:**

```bash
docker-compose up -d
```

4. **Run database migrations:**

```bash
# For User Service
cd services/user-service
dotnet ef database update
```

5. **Access the application:**

- **API Gateway**: http://localhost:5000 (Single entry point - **USE THIS**)
- Frontend: http://localhost:3000
- Swagger UI (Direct access - dev only):
  - User Service: http://localhost:5001/swagger
  - Project Service: http://localhost:5002/swagger
  - File Service: http://localhost:5003/swagger

6. **Quick test API Gateway:**

```bash
# Run automated test
cd services/api-gateway
./quick-start.sh
```

## 📁 Project Structure

```
/
├── services/
│   ├── api-gateway/          # YARP API Gateway
│   ├── user-service/          # User Management Service
│   ├── project-service/       # Project & Task Management
│   ├── file-service/          # File Upload Service
│   ├── notification-service/  # Notification Service
│   └── ai-service/            # AI Service (Python)
├── frontend/                  # Next.js Frontend
├── infrastructure/
│   ├── docker/               # Docker configs
│   ├── kafka/                # Kafka scripts
│   └── scripts/                # Database init scripts
├── shared/                    # Shared types & utilities
└── docker-compose.yml         # Docker Compose configuration
```

## 🔧 Development

### Running Individual Services

**User Service:**

```bash
cd services/user-service
dotnet run
```

**Frontend:**

```bash
cd frontend
yarn install
yarn dev
```

**AI Service:**

```bash
cd services/ai-service
pip install -r requirements.txt
uvicorn main:app --reload
```

### Database Migrations

**User Service:**

```bash
cd services/user-service
dotnet ef migrations add MigrationName
dotnet ef database update
```

## 📝 API Endpoints

### User Service (Port 5001)

- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login
- `POST /api/auth/verify-email` - Verify email
- `POST /api/auth/refresh` - Refresh access token

## 🔐 Authentication

The system uses JWT tokens for authentication:

- Access tokens expire in 15 minutes
- Refresh tokens expire in 7 days

## 📚 Documentation

### Core Documents

- [Implementation Plan](./documents/IMPLEMENTATION_PLAN.md)
- [Database Design](./documents/DATABASE_DESIGN.md)
- [System Design Document](./documents/SDD.md)
- [Requirements](./documents/SRS.md)

### Technical Guides

- [Kafka Integration Guide](./documents/KAFKA.md) - Event-Driven Architecture với Kafka
- [Commands Reference](./documents/COMMANDS.md) - Tất cả các lệnh thường dùng

## 🧪 Testing

```bash
# Run .NET tests
cd services/user-service
dotnet test

# Run frontend tests
cd frontend
yarn test
```

## 📄 License

[Your License Here]
