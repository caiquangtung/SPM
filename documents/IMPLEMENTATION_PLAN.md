# **Implementation Plan**

### **Hệ thống Web Quản lý Dự án và Theo dõi Tiến độ Thông minh**

**Phiên bản:** 1.0

**Ngày:** 28/10/2025

---

## **📋 Tổng quan**

### **Tech Stack Đã Đồng Thuận**

| Layer                | Technology              | Notes                                  |
| -------------------- | ----------------------- | -------------------------------------- |
| **Frontend**         | Next.js (React)         | TypeScript, Tailwind CSS               |
| **API Gateway**      | YARP (.NET 8)           | Routing, JWT auth, rate limiting       |
| **Backend Services** | .NET 8 (ASP.NET Core)   | User, Project, File, Notification      |
| **AI Service**       | .NET 8 (ASP.NET Core)   | Semantic Kernel, Gemini API            |
| **Database**         | **PostgreSQL 16+**      | Single DB, multiple schemas + pgvector |
| **Message Broker**   | Apache Kafka            | Event-driven architecture              |
| **Real-time**        | SignalR                 | WebSocket notifications                |
| **Containerization** | Docker + Docker Compose | Development & production               |
| **CI/CD**            | GitHub Actions          | Automated builds & deployments         |

---

## **🏗️ Architecture Overview**

```
┌─────────────────────────────────────────────────────────────┐
│                        Client Layer                         │
│                    Next.js (React/TS)                       │
└───────────────────────┬─────────────────────────────────────┘
                        │ HTTPS
┌───────────────────────▼─────────────────────────────────────┐
│                      API Gateway                            │
│                  YARP (.NET 8)                              │
│         - JWT Authentication                                │
│         - Rate Limiting                                    │
│         - Request Routing                                  │
└─────────┬───────────┬───────────┬───────────────────────────┘
          │           │           │
    ┌─────▼─────┐ ┌──▼──┐ ┌──────▼─────┐
    │   User    │ │File │ │Notification│
    │  Service  │ │Svc. │ │  Service   │
    │ (.NET 8)  │ │(NET)│ │   (NET)    │
    └─────┬─────┘ └──┬──┘ └──────┬─────┘
          │           │           │
    ┌─────▼───────────▼───────────▼─────────┐
    │         PostgreSQL Database           │
    │  spm_user │ spm_file │ spm_notification│
    └────────────┬──────────────────────────┘
                 │
    ┌────────────▼────────────────────────────┐
    │     Project Service                     │
    │     (.NET 8 + pgvector)                 │
    │                                         │
    │     PostgreSQL: spm_project             │
    │     - Task & Comment embeddings         │
    │     - Vector similarity search          │
    └────────────┬────────────────────────────┘
                 │
    ┌────────────▼────────────────────────────┐
    │      AI Service                         │
    │      (.NET 8)                           │
    │      - Semantic Kernel Integration      │
    │      - Gemini API integration           │
    │      - RAG pipeline                     │
    │      - Smart alerts                     │
    │                                         │
    │      PostgreSQL: spm_ai                 │
    └─────────────────────────────────────────┘

           ┌──────────────────┐
           │  Apache Kafka    │
           │  (Event Stream)  │
           └──────────────────┘
```

### **Database Schema Organization**

**Single Database:** `spm_db`

```
spm_db/
├── spm_user/          # User Service
│   ├── users
│   ├── email_verifications
│   └── refresh_tokens
│
├── spm_project/       # Project Service (+ pgvector)
│   ├── projects
│   ├── project_members
│   ├── tasks
│   ├── comments
│   ├── task_embeddings
│   └── comment_embeddings
│
├── spm_file/          # File Service
│   ├── files
│   └── task_attachments
│
├── spm_notification/  # Notification Service
│   └── notifications
│
└── spm_ai/            # AI Service
    ├── conversations
    ├── messages
    └── alerts
```

---

## **📅 Implementation Sprints**

### **Sprint 0: Infrastructure Setup** (Week 1-2)

#### **Phase 1: Project Structure** ✅

```
/
├── services/
│   ├── api-gateway/           (.NET 8)
│   ├── user-service/          (.NET 8)
│   ├── project-service/       (.NET 8)
│   ├── file-service/          (.NET 8)
│   ├── notification-service/  (.NET 8)
│   ├── ai-service/            (.NET 8)
├── frontend/                  (Next.js + TypeScript)
├── infrastructure/
│   ├── docker/
│   ├── kafka/
│   └── scripts/
├── shared/
│   └── types/                 (Type definitions)
└── docker-compose.yml
```

#### **Phase 2: Docker Setup** ✅

**docker-compose.yml:**

```yaml
version: "3.8"

services:
  postgres:
    image: pgvector/pgvector:pg16
    environment:
      POSTGRES_USER: spm_user
      POSTGRES_PASSWORD: spm_pass
      POSTGRES_DB: spm_db
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./infrastructure/scripts/init.sql:/docker-entrypoint-initdb.d/init.sql
    ports:
      - "5432:5432"

  kafka:
    image: confluentinc/cp-kafka:7.5.0
    depends_on:
      - zookeeper
    environment:
      KAFKA_BROKER_ID: 1
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka:9092
    ports:
      - "9092:9092"

  zookeeper:
    image: confluentinc/cp-zookeeper:7.5.0
    environment:
      ZOOKEEPER_CLIENT_PORT: 2181

  api-gateway:
    build: ./services/api-gateway
    depends_on:
      - postgres
      - kafka
    ports:
      - "5000:8080"

  user-service:
    build: ./services/user-service
    depends_on:
      - postgres
      - kafka
    ports:
      - "5001:8080"

  project-service:
    build: ./services/project-service
    depends_on:
      - postgres
      - kafka
    ports:
      - "5002:8080"

  file-service:
    build: ./services/file-service
    depends_on:
      - postgres
      - kafka
    ports:
      - "5003:8080"

  notification-service:
    build: ./services/notification-service
    depends_on:
      - postgres
      - kafka
    ports:
      - "5004:8080"

  ai-service:
    build: ./services/ai-service
    depends_on:
      - postgres
    environment:
      GEMINI_API_KEY: ${GEMINI_API_KEY}
    ports:
      - "5005:8000"

  frontend:
    build: ./frontend
    depends_on:
      - api-gateway
    ports:
      - "3000:3000"

volumes:
  postgres_data:
```

#### **Phase 3: Kafka Topics Setup** ✅

**Topics:**

- `user.created`
- `user.updated`
- `project.created`
- `project.updated`
- `task.created`
- `task.updated`
- `task.status.changed`
- `task.assigned`
- `comment.created`
- `file.uploaded`
- `notification.send`

#### **Phase 4: Database Initialization** ✅

**init.sql:**

```sql
-- Enable pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Create schemas
CREATE SCHEMA IF NOT EXISTS spm_user;
CREATE SCHEMA IF NOT EXISTS spm_project;
CREATE SCHEMA IF NOT EXISTS spm_file;
CREATE SCHEMA IF NOT EXISTS spm_notification;
CREATE SCHEMA IF NOT EXISTS spm_ai;

-- Set default search_path
ALTER DATABASE spm_db SET search_path TO public, spm_user, spm_project, spm_file, spm_notification, spm_ai;
```

#### **Phase 5: CI/CD Setup** ✅

**GitHub Actions workflows:**

- `.github/workflows/build-dotnet.yml`
- `.github/workflows/build-dotnet.yml`
- `.github/workflows/build-frontend.yml`

---

### **Sprint 1: User Management** (Week 3-4)

#### **Backend Tasks** ✅

**User Service:**

- [x] Setup Entity Framework Core với PostgreSQL
- [x] Create User, EmailVerification, RefreshToken entities
- [x] Implement BCrypt password hashing
- [x] Implement JWT token generation (access + refresh)
- [x] Create AuthController với endpoints:
  - `POST /api/auth/register`
  - `POST /api/auth/verify-email`
  - `POST /api/auth/login`
  - `POST /api/auth/refresh`
- [x] Implement role-based authorization (Admin/PM/Member)
  - Roles implemented with `UserRole` enum in domain, persisted as string with EF Core conversion; JWT/DTO expose string values; DB CHECK constraint enforces allowed roles.
- [x] Publish `user.created`, `user.updated` Kafka events
- [x] Write unit tests

#### **Frontend Tasks**

**Pages:**

- [x] `/register` - Registration form với validation
- [x] `/login` - Login form
- [x] `/verify-email/[token]` - Email verification
- [x] `/profile` - User profile editor

**Components:**

- [x] `AuthProvider` - Global auth state
- [x] `ProtectedRoute` - Route guard
- [x] Form validation với react-hook-form
- [x] Error handling & toast notifications

---

### **Sprint 2: Project & Task Management** (Week 5-7)

#### **Backend Tasks**

**Project Service:**

- [x] Setup EF Core với pgvector support
- [x] Create Project, Task, Comment entities
- [x] Create TaskEmbedding, CommentEmbedding entities
- [x] Implement Gemini Embedding API integration
- [x] Auto-generate embeddings on create/update
- [x] Implement vector similarity search
- [x] Create ProjectController, TaskController:
  - `GET /api/projects` - List user's projects
  - `POST /api/projects` - Create project
  - `GET /api/projects/{id}` - Get project details
  - `POST /api/tasks` - Create task
  - `GET /api/tasks` - List tasks (with filters)
  - `PUT /api/tasks/{id}/status` - Update status
  - `POST /api/tasks/{id}/comments` - Add comment
- [x] Publish Kafka events
- [ ] Write unit tests

**File Service:**

- [x] Setup EF Core với PostgreSQL
- [x] Create File, TaskAttachment entities
- [x] Implement multipart file upload
- [x] Store files in Docker volume
- [x] Create FileController:
  - `POST /api/files/upload` - Upload file
  - `GET /api/files/{id}` - Download file
  - `GET /api/files/{id}/download` - Download file content
  - `DELETE /api/files/{id}` - Delete file
  - `GET /api/files/my-files` - List user's files
- [x] Create TaskAttachmentsController:
  - `POST /api/tasks/{taskId}/attachments` - Attach file to task
  - `GET /api/tasks/{taskId}/attachments` - Get task attachments
  - `DELETE /api/tasks/{taskId}/attachments/{attachmentId}` - Detach file
- [x] Publish `file.uploaded` events
- [x] Write unit tests

#### **Frontend Tasks**

**Pages:**

- [x] `/dashboard` - User dashboard (placeholder completed)
- [ ] `/projects` - Projects list
- [ ] `/projects/[id]` - Project Kanban board
- [ ] `/projects/[id]/list` - Tasks list view

**Components:**

- [ ] `KanbanBoard` - Drag-drop task board (react-beautiful-dnd)
- [ ] `TaskCard` - Task display card
- [ ] `TaskDetailModal` - Full task details
- [ ] `TaskForm` - Create/edit task
- [ ] `CommentSection` - Comments UI
- [ ] `FileUpload` - File attachment UI
- [ ] Filters & sorting controls

---

### **Sprint 3: Notification System** (Week 8)

#### **Backend Tasks**

**Notification Service:**

- [ ] Setup EF Core với PostgreSQL
- [ ] Create Notification entity
- [ ] Implement SignalR Hub
- [ ] Setup Kafka consumers for events
- [ ] Create NotificationController:
  - `GET /api/notifications` - List notifications
  - `PUT /api/notifications/{id}/read` - Mark as read
  - `PUT /api/notifications/read-all` - Mark all read
- [ ] Implement real-time push via WebSocket
- [ ] Setup pg_cron for TTL cleanup
- [ ] Write unit tests

#### **Frontend Tasks**

**Components:**

- [ ] `NotificationBell` - Bell icon với badge
- [ ] `NotificationDropdown` - Dropdown list
- [ ] `NotificationList` - Full notification page
- [ ] WebSocket connection hook
- [ ] Real-time notification updates

---

### **Sprint 4: AI Service** (Week 9-11)

#### **Backend Tasks**

**AI Service (.NET 8):**

- [ ] Setup ASP.NET Core Web API project
- [ ] Install `Microsoft.SemanticKernel` & `Microsoft.SemanticKernel.Connectors.Google`
- [ ] Reuse `EmbeddingService` logic from Project Service (extract to shared lib or duplicate)
- [ ] Create Conversation, Message, Alert entities (EF Core)
- [ ] Implement RAG pipeline using Semantic Kernel:
  - Memory store integration (PostgreSQL pgvector)
  - Prompt template management
  - Context orchestration
- [ ] Create AIController:
  - `POST /api/ai/chat` - Chat endpoint
  - `POST /api/ai/generate-report` - Report generation
  - `GET /api/ai/alerts` - List alerts
- [ ] Implement smart alerts background job (HostedService)
- [ ] Write unit tests

#### **Frontend Tasks**

**Components:**

- [ ] `AIChat` - Chatbot interface
- [ ] `ReportGenerator` - Report request form
- [ ] `AlertsPanel` - Display alerts
- [ ] Floating chat button
- [ ] Markdown rendering for reports

---

### **Sprint 5: Polish & Testing** (Week 12)

#### **Security Hardening**

- [ ] HTTPS enforcement
- [ ] Input validation (FluentValidation)
- [ ] SQL injection prevention
- [ ] XSS protection
- [ ] CSRF tokens
- [ ] Rate limiting
- [ ] Security headers

#### **Performance Optimization**

- [ ] Database indexes optimization
- [ ] Query optimization
- [ ] Caching layer (Redis - optional)
- [ ] Connection pooling
- [ ] Frontend code splitting
- [ ] Asset optimization

#### **Testing**

- [ ] Unit tests (>80% coverage)
- [ ] Integration tests
- [ ] E2E tests (Playwright/Cypress)
- [ ] Load testing (100 concurrent users)
- [ ] Security testing

#### **Documentation**

- [x] API documentation (Swagger/OpenAPI) - Swagger enabled in User Service
- [x] Architecture diagrams - In IMPLEMENTATION_PLAN.md
- [x] Deployment guide - In README.md and COMMANDS.md
- [ ] User manual
- [x] Development setup guide - In README.md and COMMANDS.md

---

## **🔧 Technical Decisions**

### **Why Single PostgreSQL Database?**

✅ **Operational Simplicity:**

- Single backup strategy
- Single monitoring dashboard
- Single connection management
- 3x less operational overhead

✅ **Developer Experience:**

- EF Core works seamlessly
- Easy cross-domain queries
- Better debugging
- Consistent tooling

✅ **Performance:**

- Adequate for all workloads
- No real bottlenecks
- Efficient resource usage

✅ **RAG Integration:**

- Embeddings in same database
- Cross-schema vector search
- Simplified architecture

### **Why Microservices?**

✅ **Independent Scaling:**

- AI Service có thể scale riêng (CPU intensive)
- Project Service có thể scale riêng
- Others scale as needed

✅ **Unified Technology Stack (.NET 8):**

- **Consistency:** Toàn bộ backend dùng .NET 8, giảm context switching cho team.
- **Code Reuse:** Tái sử dụng logic (Authentication, Logging, EmbeddingService) giữa các service.
- **DevOps:** Dùng chung CI/CD pipeline và Docker configuration.

✅ **Team Autonomy:**

- Parallel development
- Independent deployments

✅ **Fault Isolation:**

- Bugs in one service không affect others
- Easier troubleshooting

---

## **📊 Success Metrics**

### **Performance Targets**

- ✅ Page load time < 2 seconds
- ✅ AI response time < 8 seconds
- ✅ Support 100 concurrent users
- ✅ API response time < 200ms (p95)

### **Quality Targets**

- ✅ Test coverage > 80%
- ✅ Zero critical security vulnerabilities
- ✅ Uptime > 99.5%
- ✅ Zero data loss

---

## **🚀 Deployment**

### **Development**

```bash
docker-compose up -d
```

### **Production**

- Docker Swarm hoặc Kubernetes
- PostgreSQL replica set
- Kafka cluster
- Load balancer (Nginx/Traefik)
- Monitoring (Prometheus + Grafana)

---

## **📊 Implementation Progress**

### **Completed Sprints**

#### **✅ Sprint 0: Infrastructure Setup** (Completed)

**Phase 1: Project Structure** ✅

- Created complete project structure with all service directories
- Organized infrastructure, shared, and frontend folders

**Phase 2: Docker Setup** ✅

- Created `docker-compose.yml` with all services configured
- Added Dockerfiles for all services (.NET 8, Python, Next.js)
- Configured health checks and dependencies
- Set up volumes and networks

**Phase 3: Kafka Topics Setup** ✅

- Created `infrastructure/kafka/topics-init.sh` script
- Defined all 11 Kafka topics for event-driven architecture

**Phase 4: Database Initialization** ✅

- Created `infrastructure/scripts/init.sql`
- Configured PostgreSQL with pgvector extension
- Set up all 5 schemas (spm_user, spm_project, spm_file, spm_notification, spm_ai)

**Phase 5: CI/CD Setup** ✅

- Created GitHub Actions workflows for .NET services
- Created GitHub Actions workflow for Python AI service
- Created GitHub Actions workflow for Next.js frontend
- Added `.gitignore` and `.env.example`

**Key Files Created:**

```
docker-compose.yml
infrastructure/scripts/init.sql
infrastructure/kafka/topics-init.sh
.github/workflows/build-dotnet.yml
.github/workflows/build-python.yml
.github/workflows/build-frontend.yml
services/*/Dockerfile (6 files)
.gitignore
.env.example
README.md
```

#### **✅ Sprint 1: User Management** (Completed)

**Backend Implementation** ✅

- Created User Service project structure (.NET 8)
- Implemented Entity Framework Core with PostgreSQL
- Created entities: `User`, `EmailVerification`, `RefreshToken`
- Implemented BCrypt password hashing service
- Implemented JWT token service (access + refresh tokens)
- Created `AuthController` with endpoints:
  - `POST /api/auth/register` - User registration
  - `POST /api/auth/verify-email` - Email verification
  - `POST /api/auth/login` - User login
  - `POST /api/auth/refresh` - Token refresh
- Implemented role-based authorization (Admin/PM/Member)
- Created Kafka producer service for `user.created` and `user.updated` events
- Configured JWT authentication middleware

**Frontend Implementation** ✅

- Set up Next.js 14 project with TypeScript and Tailwind CSS
- Created authentication context (`AuthContext`) with React hooks
- Implemented `ProtectedRoute` component for route guarding
- Created pages:
  - `/login` - Login form with validation
  - `/register` - Registration form with validation
  - `/verify-email/[token]` - Email verification page
  - `/profile` - User profile page
  - `/dashboard` - Dashboard placeholder
- Implemented API client with axios and automatic token refresh
- Added form validation using `react-hook-form`
- Integrated toast notifications with `react-hot-toast`
- Configured cookie-based token storage

**QA**

- Smoke tests completed; Sprint 1 DoD met (register → verify → login → refresh-on-401 → profile → logout).

**Key Files Created:**

```
services/user-service/
  ├── user-service.csproj
  ├── Program.cs
  ├── appsettings.json
  ├── Models/
  │   ├── User.cs
  │   ├── EmailVerification.cs
  │   └── RefreshToken.cs
  ├── Data/
  │   └── UserDbContext.cs
  ├── Services/
  │   ├── IPasswordService.cs
  │   ├── PasswordService.cs
  │   ├── ITokenService.cs
  │   ├── TokenService.cs
  │   ├── IKafkaProducerService.cs
  │   └── KafkaProducerService.cs
  ├── Controllers/
  │   └── AuthController.cs
  ├── DTOs/
  │   └── AuthRequest.cs
  └── .dockerignore

frontend/
  ├── package.json
  ├── tsconfig.json
  ├── next.config.js
  ├── tailwind.config.js
  ├── postcss.config.js
  ├── app/
  │   ├── layout.tsx
  │   ├── page.tsx
  │   ├── globals.css
  │   ├── login/page.tsx
  │   ├── register/page.tsx
  │   ├── verify-email/[token]/page.tsx
  │   ├── profile/page.tsx
  │   └── dashboard/page.tsx
  ├── components/
  │   └── ProtectedRoute.tsx
  ├── contexts/
  │   └── AuthContext.tsx
  └── lib/
      ├── api.ts
      └── auth.ts
```

**Configuration Details:**

- JWT tokens: 15-minute access tokens, 7-day refresh tokens
- Password hashing: BCrypt with salt rounds of 12
- Database schema: `spm_user` with proper indexes and constraints
- CORS: Configured for frontend at `http://localhost:3000`
- API Base URL: Configurable via `NEXT_PUBLIC_API_URL`

### 🚧 Sprint 2: Project & Task Management (In Progress)

**Backend Progress** ✅

**Project Service:**

- Integrated Gemini Embedding API and automatic embedding generation for tasks & comments
- Added vector similarity search endpoint (`POST /api/tasks/search`) backed by pgvector
- Ensured Kafka events are published for project/task/comment lifecycle updates
- Documented service responsibilities & troubleshooting (`docs/ARCHITECTURE_DECISIONS.md`, `docs/TROUBLESHOOTING.md`, `docs/QUICK_FIX.md`, `docs/RESPONSIBILITIES.md`)
- Created `appsettings.example.json` templates and updated `.gitignore` to keep secrets out of git

**File Service:**

- Implemented complete file upload, storage, and management system
- Created File and TaskAttachment entities with EF Core
- Implemented multipart file upload with validation (max 100 MB)
- Configured Docker volume storage at `/app/storage`
- Created FilesController with upload, download, delete, and list endpoints
- Created TaskAttachmentsController for linking files to tasks
- Integrated Kafka event publishing for `file.uploaded` events
- Implemented JWT authentication and ownership validation
- Added soft delete functionality for files
- Created comprehensive README documentation

**Outstanding Work**

- Project Service unit tests
- File Service unit tests
- Frontend project/task UI (Kanban board, task detail, comments, file attachments)

**Key Files Created (File Service):**

```
services/file-service/
  ├── file-service.csproj
  ├── Program.cs
  ├── appsettings.example.json
  ├── Models/
  │   ├── File.cs
  │   └── TaskAttachment.cs
  ├── Data/
  │   └── FileDbContext.cs
  ├── Repositories/
  │   ├── Interfaces/
  │   │   ├── IFileRepository.cs
  │   │   └── ITaskAttachmentRepository.cs
  │   ├── FileRepository.cs
  │   └── TaskAttachmentRepository.cs
  ├── Services/
  │   ├── Interfaces/
  │   │   ├── IFileService.cs
  │   │   ├── ITaskAttachmentService.cs
  │   │   └── IKafkaProducerService.cs
  │   ├── FileService.cs
  │   ├── TaskAttachmentService.cs
  │   └── KafkaProducerService.cs
  ├── Controllers/
  │   ├── FilesController.cs
  │   └── TaskAttachmentsController.cs
  ├── DTOs/
  │   ├── ApiResponse.cs
  │   ├── FileResponse.cs
  │   ├── TaskAttachmentResponse.cs
  │   └── AttachFileToTaskRequest.cs
  ├── Validators/
  │   ├── ValidationExtensions.cs
  │   └── AttachFileToTaskRequestValidator.cs
  ├── Extensions/
  │   └── ControllerExtensions.cs
  ├── Middleware/
  │   └── GlobalExceptionHandlerMiddleware.cs
  ├── README.md
  └── .dockerignore
```

**Configuration Details (File Service):**

- Database schema: `spm_file` with proper indexes and constraints
- File storage: Docker volume mounted at `/app/storage`
- Maximum file size: 100 MB (configurable)
- JWT authentication: Required for all endpoints
- Kafka events: `file.uploaded` published after successful upload
- Soft delete: Files are marked as deleted, not physically removed immediately

### **Next Steps**

The following sprints are ready for implementation:

1. **Sprint 2: Project & Task Management** - Implement project service with pgvector support
2. **Sprint 3: Notification System** - SignalR real-time notifications
3. **Sprint 4: AI Service** - Python FastAPI service with Gemini integration
4. **Sprint 5: Polish & Testing** - Security, performance, and comprehensive testing

---

**END OF PLAN**

**Sprint 0 & Sprint 1 Completed! 🎉**
