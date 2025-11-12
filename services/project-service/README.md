# Project Service

**Microservice quản lý Projects, Tasks, Comments với AI-powered Vector Search**

## 📋 Tổng quan

Project Service là một microservice được xây dựng bằng .NET 8, chịu trách nhiệm quản lý toàn bộ lifecycle của projects, tasks và comments trong hệ thống SPM. Service này tích hợp với Gemini Embedding API để tự động tạo embeddings và hỗ trợ vector similarity search cho semantic search.

## 🎯 Trách nhiệm chính

### 1. **Project Management**

- Tạo, đọc, cập nhật projects
- Quản lý project members và roles
- Lọc projects theo user

### 2. **Task Management**

- CRUD operations cho tasks
- Quản lý task status (ToDo, InProgress, Done, Blocked)
- Quản lý task priority (Low, Medium, High, Critical)
- Gán tasks cho users
- Set due dates

### 3. **Comment Management**

- Thêm comments vào tasks
- Lấy danh sách comments của một task
- Real-time collaboration support

### 4. **AI-Powered Features**

- **Auto-generate Embeddings**: Tự động tạo embeddings cho tasks và comments khi được tạo/cập nhật
- **Vector Similarity Search**: Tìm kiếm tasks tương tự dựa trên semantic similarity
- **Gemini Integration**: Sử dụng Gemini Embedding API (`embedding-001` model, 768 dimensions)

### 5. **Event-Driven Architecture**

- Publish events ra Kafka khi có thay đổi:
  - `project.created`
  - `project.task.created`
  - `project.task.updated`
  - `project.task.status.changed`
  - `project.comment.created`

## 🏗️ Kiến trúc

### **Technology Stack**

- **Framework**: .NET 8 (ASP.NET Core)
- **ORM**: Entity Framework Core 8.0
- **Database**: PostgreSQL 16+ với pgvector extension
- **Vector Storage**: pgvector (vector(768))
- **Message Broker**: Apache Kafka (Confluent.Kafka)
- **Authentication**: JWT Bearer Token
- **Validation**: FluentValidation
- **AI Integration**: Google Gemini Embedding API

### **Architecture Patterns**

- **Clean Architecture**: Controllers → Services → Repositories → Database
- **Repository Pattern**: Tách biệt data access logic
- **Service Layer**: Business logic và orchestration
- **DTO Pattern**: Request/Response DTOs cho API contracts
- **Event-Driven**: Kafka events cho async communication

## 📁 Cấu trúc Project

```
project-service/
├── Controllers/           # API Controllers
│   ├── ProjectsController.cs
│   ├── TasksController.cs
│   └── CommentsController.cs
├── Services/             # Business Logic
│   ├── Interfaces/
│   │   ├── IProjectService.cs
│   │   ├── ITaskService.cs
│   │   ├── ICommentService.cs
│   │   ├── IEmbeddingService.cs
│   │   └── IKafkaProducerService.cs
│   ├── ProjectService.cs
│   ├── TaskService.cs
│   ├── CommentService.cs
│   ├── EmbeddingService.cs
│   └── KafkaProducerService.cs
├── Repositories/          # Data Access
│   ├── Interfaces/
│   │   ├── IProjectRepository.cs
│   │   ├── ITaskRepository.cs
│   │   ├── ICommentRepository.cs
│   │   ├── ITaskEmbeddingRepository.cs
│   │   └── ICommentEmbeddingRepository.cs
│   ├── ProjectRepository.cs
│   ├── TaskRepository.cs
│   ├── CommentRepository.cs
│   ├── TaskEmbeddingRepository.cs
│   └── CommentEmbeddingRepository.cs
├── Models/               # Domain Entities
│   ├── Project.cs
│   ├── ProjectMember.cs
│   ├── ProjectTask.cs
│   ├── ProjectComment.cs
│   ├── TaskEmbedding.cs
│   ├── CommentEmbedding.cs
│   ├── TaskStatus.cs
│   ├── TaskPriority.cs
│   └── ProjectMemberRole.cs
├── DTOs/                 # Data Transfer Objects
│   ├── Projects/
│   │   ├── CreateProjectRequest.cs
│   │   └── ProjectResponse.cs
│   ├── Tasks/
│   │   ├── CreateTaskRequest.cs
│   │   ├── UpdateTaskStatusRequest.cs
│   │   ├── TaskResponse.cs
│   │   ├── SearchTasksRequest.cs
│   │   └── SearchResult.cs
│   └── Comments/
│       ├── CreateCommentRequest.cs
│       └── CommentResponse.cs
├── Data/                 # Database Context
│   └── ProjectDbContext.cs
├── Validators/           # FluentValidation Validators
│   ├── CreateProjectRequestValidator.cs
│   ├── CreateTaskRequestValidator.cs
│   ├── UpdateTaskStatusRequestValidator.cs
│   ├── CreateCommentRequestValidator.cs
│   └── SearchTasksRequestValidator.cs
├── Extensions/           # Extension Methods
│   └── ControllerExtensions.cs
├── Migrations/           # EF Core Migrations
└── Program.cs            # Application Entry Point
```

## 🔌 API Endpoints

### **Projects**

| Method | Endpoint             | Description                              | Auth Required |
| ------ | -------------------- | ---------------------------------------- | ------------- |
| GET    | `/api/projects`      | Lấy danh sách projects của user hiện tại | ✅            |
| GET    | `/api/projects/{id}` | Lấy thông tin chi tiết project           | ✅            |
| POST   | `/api/projects`      | Tạo project mới                          | ✅            |

### **Tasks**

| Method | Endpoint                    | Description                             | Auth Required |
| ------ | --------------------------- | --------------------------------------- | ------------- |
| GET    | `/api/tasks?projectId={id}` | Lấy danh sách tasks của project         | ✅            |
| POST   | `/api/tasks`                | Tạo task mới                            | ✅            |
| PUT    | `/api/tasks/{id}/status`    | Cập nhật status của task                | ✅            |
| POST   | `/api/tasks/search`         | Tìm kiếm tasks tương tự (vector search) | ✅            |

### **Comments**

| Method | Endpoint                       | Description                     | Auth Required |
| ------ | ------------------------------ | ------------------------------- | ------------- |
| GET    | `/api/tasks/{taskId}/comments` | Lấy danh sách comments của task | ✅            |
| POST   | `/api/tasks/{taskId}/comments` | Thêm comment vào task           | ✅            |

## 🗄️ Database Schema

### **Schema: `spm_project`**

#### **Tables:**

1. **projects** - Quản lý projects
2. **project_members** - Quan hệ many-to-many giữa users và projects
3. **tasks** - Quản lý tasks
4. **comments** - Quản lý comments
5. **task_embeddings** - Vector embeddings cho tasks (768 dimensions)
6. **comment_embeddings** - Vector embeddings cho comments (768 dimensions)

### **Key Features:**

- **pgvector Extension**: Sử dụng `vector(768)` type cho embeddings
- **Indexes**: Indexes cho performance (có thể thêm IVFFlat/HNSW cho vector search)
- **Foreign Keys**: Relationships với user-service schema (`spm_user`)

## 🤖 AI Integration (Gemini Embedding API)

### **Embedding Generation**

- **Model**: `embedding-001` (768 dimensions)
- **Auto-generation**: Tự động tạo embeddings khi:
  - Tạo task mới (title + description)
  - Tạo comment mới (content)
- **Async Processing**: Fire-and-forget pattern để không block request

### **Vector Similarity Search**

- **Algorithm**: Cosine distance (`<=>`) trong PostgreSQL
- **Query Flow**:
  1. User gửi search query (text)
  2. Generate query embedding từ Gemini API
  3. Vector similarity search trong PostgreSQL
  4. Return top-K results với similarity scores

### **Configuration**

```json
{
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY",
    "EmbeddingApiUrl": "https://generativelanguage.googleapis.com/v1beta/models/embedding-001:embedContent"
  }
}
```

Hoặc sử dụng environment variable:

```bash
export GEMINI_API_KEY=your_api_key_here
```

## 📨 Kafka Events

Service publish các events sau ra Kafka:

| Event Topic                   | Event Type        | Payload                                       |
| ----------------------------- | ----------------- | --------------------------------------------- |
| `project.created`             | ProjectCreated    | `{ projectId, userId, name }`                 |
| `project.task.created`        | TaskCreated       | `{ taskId, projectId, userId, title }`        |
| `project.task.updated`        | TaskUpdated       | `{ taskId, projectId, userId, title }`        |
| `project.task.status.changed` | TaskStatusChanged | `{ taskId, projectId, oldStatus, newStatus }` |
| `project.comment.created`     | CommentCreated    | `{ commentId, taskId, userId, content }`      |

## 🔐 Authentication & Authorization

- **JWT Bearer Token**: Tất cả endpoints yêu cầu authentication
- **User ID Extraction**: Tự động extract `userId` từ JWT claims
- **Authorization**: Dựa trên user ownership (user chỉ có thể xem/sửa projects/tasks của mình)

## 🧪 Testing

### **Postman Collection**

- File: `Postman_Collection.json`
- Environment: Set `baseUrl` và `accessToken`

### **Example Request:**

```http
POST /api/tasks/search
Authorization: Bearer {token}
Content-Type: application/json

{
  "query": "authentication login",
  "topK": 10,
  "projectId": "optional-project-id"
}
```

## 🚀 Deployment

### **Docker**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
# ... Dockerfile configuration
```

### **Environment Variables**

- `ConnectionStrings__DefaultConnection`: PostgreSQL connection string
- `JWT__SecretKey`: JWT secret key
- `Gemini__ApiKey`: Gemini API key
- `Kafka__BootstrapServers`: Kafka bootstrap servers

### **Database Migration**

```bash
cd services/project-service
dotnet ef migrations add MigrationName
dotnet ef database update
```

## 📊 Performance Considerations

1. **Embedding Generation**: Async, non-blocking để không làm chậm API response
2. **Vector Search**: Có thể thêm IVFFlat hoặc HNSW indexes cho better performance
3. **Caching**: Có thể cache embeddings nếu content không thay đổi
4. **Background Jobs**: Có thể move embedding generation sang background job queue (Hangfire, etc.)

## 🔄 Future Enhancements

- [ ] Update task/comment embeddings khi content thay đổi
- [ ] Add IVFFlat/HNSW indexes cho vector search
- [ ] Implement comment search (similar to task search)
- [ ] Add pagination cho search results
- [ ] Add filtering options (status, priority, date range)
- [ ] Background job queue cho embedding generation reliability
- [ ] Metrics và monitoring cho embedding generation

## 📚 Related Documentation

- [SDD.md](../../documents/SDD.md) - System Design Document
- [DATABASE_DESIGN.md](../../documents/DATABASE_DESIGN.md) - Database Schema
- [IMPLEMENTATION_PLAN.md](../../documents/IMPLEMENTATION_PLAN.md) - Implementation Roadmap
