# Project Service - Roles & Responsibilities

Tài liệu này mô tả chi tiết các nhiệm vụ và trách nhiệm của Project Service trong hệ thống SPM.

---

## 📋 Mục lục

- [1. Tổng quan](#1-tổng-quan)
- [2. Trách nhiệm chính](#2-trách-nhiệm-chính)
- [3. Mối quan hệ với các Services khác](#3-mối-quan-hệ-với-các-services-khác)
- [4. Phân biệt với AI Service](#4-phân-biệt-với-ai-service)
- [5. Data Flow](#5-data-flow)
- [6. API Responsibilities](#6-api-responsibilities)
- [7. Database Responsibilities](#7-database-responsibilities)

---

## 1. Tổng quan

**Project Service** là microservice cốt lõi của hệ thống SPM, chịu trách nhiệm quản lý toàn bộ lifecycle của projects, tasks và comments. Service này đóng vai trò là **Data Indexing Layer** cho AI features, tự động tạo và lưu trữ embeddings để hỗ trợ semantic search và RAG (Retrieval-Augmented Generation).

### **Vị trí trong kiến trúc:**

```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │
┌──────▼──────────────────┐
│   API Gateway (YARP)    │
└──────┬──────────────────┘
       │
┌──────▼──────────────────┐
│   Project Service       │ ← Chúng ta đây
│  - CRUD Projects/Tasks  │
│  - Embedding Generation │
│  - Vector Search        │
└──────┬──────────────────┘
       │
┌──────▼──────────────────┐
│   PostgreSQL + pgvector │
└─────────────────────────┘
```

---

## 2. Trách nhiệm chính

### **2.1. Project Management**

#### **Chức năng:**

- ✅ **CRUD Operations**: Tạo, đọc, cập nhật, xóa projects
- ✅ **Project Members**: Quản lý thành viên và roles trong project
- ✅ **Project Filtering**: Lọc projects theo user ownership
- ✅ **Project Metadata**: Quản lý name, description, created_by, timestamps

#### **API Endpoints:**

- `GET /api/projects` - Lấy danh sách projects của user
- `GET /api/projects/{id}` - Lấy chi tiết project
- `POST /api/projects` - Tạo project mới

#### **Business Rules:**

- User chỉ có thể xem/sửa projects mà họ là owner hoặc member
- Project phải có name (required)
- Project có thể có description (optional)

---

### **2.2. Task Management**

#### **Chức năng:**

- ✅ **CRUD Operations**: Tạo, đọc, cập nhật tasks
- ✅ **Task Status**: Quản lý status (ToDo, InProgress, Done, Blocked)
- ✅ **Task Priority**: Quản lý priority (Low, Medium, High, Critical)
- ✅ **Task Assignment**: Gán tasks cho users
- ✅ **Due Dates**: Quản lý deadlines
- ✅ **Task Filtering**: Lọc tasks theo project, status, assignee

#### **API Endpoints:**

- `GET /api/tasks?projectId={id}` - Lấy danh sách tasks của project
- `POST /api/tasks` - Tạo task mới
- `PUT /api/tasks/{id}/status` - Cập nhật task status
- `POST /api/tasks/search` - Vector similarity search (semantic search)

#### **Business Rules:**

- Task phải thuộc về một project
- Task có thể có assignee (optional)
- Task status có thể thay đổi theo workflow
- Task có thể có due date (optional)

#### **Auto-Generate Embeddings:**

- Khi tạo task mới → Tự động generate embedding từ title + description
- Embedding được lưu vào `task_embeddings` table
- Fire-and-forget pattern (không block API response)

---

### **2.3. Comment Management**

#### **Chức năng:**

- ✅ **Add Comments**: Thêm comments vào tasks
- ✅ **List Comments**: Lấy danh sách comments của task
- ✅ **Comment Metadata**: Quản lý author, content, timestamps
- ✅ **Real-time Support**: Comments có thể được push real-time qua SignalR (Notification Service)

#### **API Endpoints:**

- `GET /api/tasks/{taskId}/comments` - Lấy danh sách comments
- `POST /api/tasks/{taskId}/comments` - Thêm comment mới

#### **Business Rules:**

- Comment phải thuộc về một task
- Comment phải có content (required)
- Comment author được tự động set từ JWT token

#### **Auto-Generate Embeddings:**

- Khi tạo comment mới → Tự động generate embedding từ content
- Embedding được lưu vào `comment_embeddings` table
- Fire-and-forget pattern

---

### **2.4. AI-Powered Features (Data Indexing Layer)**

#### **A. Embedding Generation**

**Trách nhiệm:**

- ✅ Gọi Gemini Embedding API để tạo embeddings
- ✅ Lưu embeddings vào PostgreSQL (pgvector)
- ✅ Handle errors gracefully (không fail task/comment creation nếu embedding fail)

**Flow:**

```
Create Task/Comment
    ↓
Generate Embedding (Gemini API)
    ↓
Save to task_embeddings/comment_embeddings
    ↓
Ready for Vector Search
```

**Technical Details:**

- Model: `embedding-001` (768 dimensions)
- Async, fire-and-forget pattern
- Error handling: Log errors, không block business flow

#### **B. Vector Similarity Search**

**Trách nhiệm:**

- ✅ Generate query embedding từ search text
- ✅ Perform vector similarity search trong PostgreSQL
- ✅ Return top-K results với similarity scores

**Flow:**

```
User Search Query
    ↓
Generate Query Embedding (Gemini API)
    ↓
Vector Similarity Search (PostgreSQL + pgvector)
    ↓
Return Similar Tasks/Comments
```

**Technical Details:**

- Algorithm: Cosine distance (`<=>`) operator
- Support filtering by projectId (optional)
- Configurable topK (default: 10, max: 100)

**Use Cases:**

- Semantic search: Tìm tasks/comments tương tự về mặt ngữ nghĩa
- Duplicate detection: Phát hiện tasks/comments trùng lặp
- Related content: Tìm nội dung liên quan

---

### **2.5. Event-Driven Architecture**

#### **Trách nhiệm:**

- ✅ Publish events ra Kafka khi có thay đổi
- ✅ Events được consume bởi Notification Service và AI Service

#### **Events Published:**

| Event Topic                   | Trigger                  | Payload                                       | Consumers                        |
| ----------------------------- | ------------------------ | --------------------------------------------- | -------------------------------- |
| `project.created`             | Khi tạo project mới      | `{ projectId, userId, name }`                 | Notification Service             |
| `project.task.created`        | Khi tạo task mới         | `{ taskId, projectId, userId, title }`        | Notification Service, AI Service |
| `project.task.updated`        | Khi cập nhật task        | `{ taskId, projectId, userId, title }`        | Notification Service, AI Service |
| `project.task.status.changed` | Khi thay đổi task status | `{ taskId, projectId, oldStatus, newStatus }` | Notification Service             |
| `project.comment.created`     | Khi thêm comment         | `{ commentId, taskId, userId, content }`      | Notification Service, AI Service |

#### **Event Flow:**

```
Project Service
    ↓ (Publish Event)
Kafka
    ↓ (Consume)
┌─────────────────┬──────────────────┐
│ Notification    │ AI Service       │
│ Service         │ (for indexing)   │
│ (Send alerts)   │                  │
└─────────────────┴──────────────────┘
```

---

## 3. Mối quan hệ với các Services khác

### **3.1. User Service**

**Mối quan hệ:**

- ✅ **Dependency**: Project Service phụ thuộc vào User Service cho authentication
- ✅ **JWT Tokens**: Validate JWT tokens từ User Service
- ✅ **User IDs**: Sử dụng userId từ JWT claims (không cần gọi API)

**Không làm:**

- ❌ Không quản lý users
- ❌ Không validate user existence (assume user exists nếu có valid JWT)

---

### **3.2. AI Service**

**Mối quan hệ:**

- ✅ **Data Provider**: Project Service cung cấp embeddings cho AI Service
- ✅ **Shared Database**: Cả hai đọc từ cùng PostgreSQL database
- ✅ **Event Consumer**: AI Service consume events từ Project Service (qua Kafka)

**Phân chia trách nhiệm:**

| Feature                    | Project Service          | AI Service                  |
| -------------------------- | ------------------------ | --------------------------- |
| **Embedding Generation**   | ✅ Tạo embeddings        | ❌ Không tạo                |
| **Embedding Storage**      | ✅ Lưu embeddings        | ❌ Chỉ đọc                  |
| **Vector Search (Simple)** | ✅ Search tasks/comments | ❌ Không có                 |
| **Vector Search (RAG)**    | ❌ Không có              | ✅ Retrieve context cho RAG |
| **Question Answering**     | ❌ Không có              | ✅ Generate answers         |
| **Report Generation**      | ❌ Không có              | ✅ Generate reports         |
| **Smart Alerts**           | ❌ Không có              | ✅ Analyze và alert         |

**Data Flow:**

```
Project Service (Indexing)
  ↓
Create Task → Generate Embedding → Save to DB
  ↓
AI Service (Retrieval)
  ↓
User Question → Generate Query Embedding → Vector Search → Retrieve Context → Generate Answer
```

---

### **3.3. Notification Service**

**Mối quan hệ:**

- ✅ **Event Producer**: Project Service publish events → Notification Service consume
- ✅ **Indirect Communication**: Không gọi trực tiếp, communicate qua Kafka

**Flow:**

```
Project Service
  ↓ (Publish: project.task.created)
Kafka
  ↓ (Consume)
Notification Service
  ↓ (Send notification)
User (via SignalR)
```

---

### **3.4. File Service**

**Mối quan hệ:**

- ✅ **Indirect**: File Service có thể publish events khi upload file
- ✅ **Future Integration**: Có thể link files với tasks/projects (chưa implement)

---

## 4. Phân biệt với AI Service

### **4.1. Tại sao không tách Embedding Service riêng?**

#### **Lý do chọn embedding trong Project Service:**

1. **Tight Coupling với Business Data**

   - Embeddings phụ thuộc trực tiếp vào task/comment content
   - Cần được tạo ngay khi task/comment được tạo
   - Tách ra sẽ tạo thêm network calls và complexity

2. **Data Locality & Performance**

   - Embeddings và business data ở cùng database
   - Có thể query cùng lúc (JOIN operations)
   - Giảm network latency

3. **Transaction Consistency**

   - Có thể đảm bảo consistency trong cùng transaction
   - Không cần distributed transaction

4. **Simplicity**
   - Đơn giản hóa architecture
   - Dễ maintain và debug

#### **Nếu tách Embedding Service riêng:**

**Vấn đề:**

- Thêm network hop (latency)
- Phức tạp hóa transaction (distributed transaction)
- Risk: task tạo thành công nhưng embedding fail
- Cần sync giữa 2 services

---

### **4.2. Phân chia trách nhiệm với AI Service**

#### **Project Service = Data Indexing Layer**

- **Mục đích**: Tạo và lưu embeddings (data preparation)
- **Input**: Task/Comment content
- **Output**: Embeddings trong database
- **Use Cases**: Simple semantic search

#### **AI Service = AI Processing Layer**

- **Mục đích**: Sử dụng embeddings để RAG và generate responses
- **Input**: User questions, embeddings từ database
- **Output**: AI-generated answers, reports, alerts
- **Use Cases**: Question answering, report generation, smart alerts

#### **So sánh:**

| Aspect                  | Project Service            | AI Service                   |
| ----------------------- | -------------------------- | ---------------------------- |
| **Primary Role**        | Data Management + Indexing | AI Processing                |
| **Gemini API Usage**    | Embedding API only         | Embedding API + Generate API |
| **Database Operations** | Write embeddings           | Read embeddings              |
| **User Interaction**    | Direct API calls           | RAG pipeline                 |
| **Complexity**          | Simple CRUD + Embeddings   | Complex AI logic             |

---

## 5. Data Flow

### **5.1. Task Creation Flow**

```
User Request
    ↓
POST /api/tasks
    ↓
TaskService.CreateAsync()
    ↓
┌─────────────────────────┬──────────────────────────┐
│ Save Task to DB         │ Generate Embedding       │
│ (in transaction)        │ (async, fire-and-forget) │
└─────────────────────────┴──────────────────────────┘
    ↓                              ↓
Commit Transaction          Save Embedding to DB
    ↓                              ↓
Publish Event (Kafka)       Ready for Search
    ↓
Notification Service
```

### **5.2. Vector Search Flow**

```
User Search Request
    ↓
POST /api/tasks/search
    ↓
TaskService.SearchSimilarAsync()
    ↓
Generate Query Embedding (Gemini API)
    ↓
Vector Similarity Search (PostgreSQL)
    ↓
Return Top-K Results
```

### **5.3. RAG Flow (AI Service sử dụng embeddings)**

```
User Question → AI Service
    ↓
Generate Query Embedding (Gemini API)
    ↓
Vector Search in PostgreSQL (read embeddings từ Project Service)
    ↓
Retrieve Top-K Relevant Tasks/Comments
    ↓
Combine Context + Question
    ↓
Send to Gemini Generate API
    ↓
Return AI Response
```

---

## 6. API Responsibilities

### **6.1. Authentication & Authorization**

**Trách nhiệm:**

- ✅ Validate JWT tokens từ User Service
- ✅ Extract userId từ JWT claims
- ✅ All endpoints require authentication (except health check)

**Implementation:**

- JWT Bearer Token authentication
- `[Authorize]` attribute trên controllers
- `ControllerExtensions.GetUserId()` để extract userId

---

### **6.2. Input Validation**

**Trách nhiệm:**

- ✅ Validate all request DTOs
- ✅ Return clear error messages
- ✅ Use FluentValidation

**Validation Rules:**

- Project: name required, max length
- Task: title required, valid projectId, valid status/priority
- Comment: content required, max length
- Search: query required, topK within range

---

### **6.3. Error Handling**

**Trách nhiệm:**

- ✅ Handle business errors (return appropriate HTTP status codes)
- ✅ Handle technical errors (log và return generic error)
- ✅ Don't expose internal errors to clients

**Error Types:**

- `400 Bad Request`: Validation errors
- `401 Unauthorized`: Authentication failed
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Unexpected errors

---

## 7. Database Responsibilities

### **7.1. Schema Management**

**Trách nhiệm:**

- ✅ Manage `spm_project` schema
- ✅ Create and maintain tables: projects, tasks, comments, embeddings
- ✅ Manage relationships và foreign keys

**Tables:**

- `projects` - Project data
- `project_members` - Project membership
- `tasks` - Task data
- `comments` - Comment data
- `task_embeddings` - Task embeddings (vector(768))
- `comment_embeddings` - Comment embeddings (vector(768))

---

### **7.2. Migrations**

**Trách nhiệm:**

- ✅ Create EF Core migrations
- ✅ Apply migrations to database
- ✅ Version control migrations

**Commands:**

```bash
dotnet ef migrations add MigrationName --context ProjectDbContext
dotnet ef database update --context ProjectDbContext
```

---

### **7.3. Indexes**

**Trách nhiệm:**

- ✅ Create indexes cho performance
- ✅ Vector indexes cho similarity search (IVFFlat, HNSW)

**Current Indexes:**

- Projects: `idx_projects_created_by`, `idx_projects_created_at`
- Tasks: `idx_tasks_project_id`, `idx_tasks_assigned_to`
- Comments: `idx_comments_task_id`, `idx_comments_user_id`

**Future Indexes:**

- Vector indexes: `idx_task_embeddings_vector` (IVFFlat với cosine_ops)

---

## 📚 Related Documentation

- [README.md](../README.md) - Project Service overview
- [ARCHITECTURE_DECISIONS.md](./ARCHITECTURE_DECISIONS.md) - Architecture decisions
- [TROUBLESHOOTING.md](./TROUBLESHOOTING.md) - Troubleshooting guide
- [QUICK_FIX.md](./QUICK_FIX.md) - Quick fixes

---

## 🎯 Summary

**Project Service là:**

- ✅ **Data Management Service**: Quản lý projects, tasks, comments
- ✅ **Data Indexing Service**: Tạo và lưu embeddings cho AI features
- ✅ **Search Service**: Cung cấp vector similarity search
- ✅ **Event Producer**: Publish events cho other services

**Project Service không phải:**

- ❌ **AI Processing Service**: Không generate AI responses
- ❌ **User Management Service**: Không quản lý users
- ❌ **Notification Service**: Không send notifications trực tiếp
- ❌ **File Management Service**: Không quản lý files

**Key Principle:**

> **"Project Service manages data and indexes it for AI. AI Service uses that indexed data to generate intelligent responses."**

---

**Last Updated:** November 12, 2025
