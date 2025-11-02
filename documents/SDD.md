## **Tài liệu Thiết kế Hệ thống (SDD) - Phiên bản Đầy đủ**

### **Hệ thống Web Quản lý Dự án và Theo dõi Tiến độ Thông minh**

**Phiên bản:** 1.0

**Ngày:** 28/10/2025

---

### **1. Giới thiệu**

### **1.1. Mục đích**

Tài liệu này cung cấp một bản thiết kế kỹ thuật toàn diện cho "Hệ thống Web Quản lý Dự án và Theo dõi Tiến độ Thông minh". Nó chi tiết hóa kiến trúc hệ thống, thiết kế thành phần, lược đồ dữ liệu, giao thức giao tiếp, và các chiến lược triển khai. Đây là tài liệu gốc để đội ngũ kỹ thuật dựa vào đó để xây dựng và phát triển sản phẩm.

### **1.2. Phạm vi**

Thiết kế này bao gồm toàn bộ hệ thống, từ kiến trúc tổng thể, thiết kế chi tiết cho từng Microservice (bao gồm cả .NET và Python), thiết kế cơ sở dữ liệu, chiến lược bảo mật, đến quy trình CI/CD và giám sát vận hành.

---

### **2. Thiết kế Kiến trúc Hệ thống**

### **2.1. Phong cách Kiến trúc: Microservice Đa ngôn ngữ (Hybrid)**

Hệ thống sẽ được xây dựng dựa trên **Kiến trúc Microservice** để đáp ứng các yêu cầu về khả năng mở rộng, bảo trì và triển khai độc lập.

- **Lý do lựa chọn:**
  - **Khả năng mở rộng (Scalability):** Từng dịch vụ (ví dụ: AI Service, Notification Service) có thể được mở rộng độc lập tùy theo tải.
  - **Linh hoạt Công nghệ (Technology Flexibility):** AI Service sử dụng Python (FastAPI) phù hợp với các thư viện ML/AI, các service còn lại sử dụng .NET 8 (ASP.NET Core) để đảm bảo tính nhất quán và hiệu năng.
  - **Triển khai Độc lập (Independent Deployment):** Nâng cấp một dịch vụ không làm ảnh hưởng đến toàn bộ hệ thống, giúp quy trình CI/CD nhanh chóng và an toàn hơn.
  - **Tăng cường sự Tự chủ của Đội ngũ:** Các nhóm nhỏ có thể sở hữu và phát triển một hoặc nhiều dịch vụ.

### **2.2. Sơ đồ Kiến trúc Tổng quan**

## mermaid

config:
layout: dagre

---

flowchart TD
subgraph Client["Client"]
Browser["Web Browser (React/Next.js)"]
end
subgraph Gateway["Gateway"]
APIGateway["API Gateway (YARP / Kong)"]
end
subgraph subGraph2["Backend Services"]
UserService["<strong>User Service</strong><br>.NET 8 / ASP.NET Core<br><i>[PostgreSQL]</i>"]
ProjectService["<strong>Project Service</strong><br>.NET 8 / ASP.NET Core<br><i>[PostgreSQL + pgvector]</i>"]
FileService["<strong>File Service</strong><br>.NET 8 / ASP.NET Core<br><i>[PostgreSQL]</i>"]
NotificationSvc["<strong>Notification Svc.</strong><br>.NET 8 / SignalR<br><i>[PostgreSQL]</i>"]
AIService["<strong>AI Service</strong><br>Python / FastAPI<br><i>[PostgreSQL + pgvector + Gemini]</i>"]
end
subgraph subGraph3["Messaging & Event Streaming"]
Kafka["Apache Kafka Cluster"]
end
Browser -- HTTPS Requests --> APIGateway
APIGateway -- /api/auth/_ --> UserService
APIGateway -- /api/projects/_, /api/tasks/_ --> ProjectService
APIGateway -- /api/files/_ --> FileService
APIGateway -- /api/ai/\* --> AIService
ProjectService -- "Produces Events (Task Created, etc.)" --> Kafka
FileService -- "Produces Events (File Uploaded)" --> Kafka
Kafka -- Consumes Events --> NotificationSvc
Kafka -- Consumes Events for Indexing --> AIService
NotificationSvc -. WebSocket Push .-> Browser

### **2.3. Mô tả các Thành phần Chính**

| **Thành phần**           | **Công nghệ**                  | **Database**          | **Trách nhiệm chính**                                             |
| ------------------------ | ------------------------------ | --------------------- | ----------------------------------------------------------------- |
| **Client**               | Next.js (React)                | -                     | Giao diện người dùng, quản lý trạng thái phía client.             |
| **API Gateway**          | YARP (.NET 8)                  | -                     | Điểm vào duy nhất, định tuyến, xác thực, rate limiting.           |
| **User Service**         | .NET 8 (ASP.NET Core)          | PostgreSQL            | Quản lý người dùng, hồ sơ, xác thực & phân quyền.                 |
| **Project Service**      | .NET 8 (ASP.NET Core)          | PostgreSQL + pgvector | Logic nghiệp vụ cốt lõi: dự án, công việc, bình luận, embeddings. |
| **File Service**         | .NET 8 (ASP.NET Core)          | PostgreSQL            | Quản lý upload/download file, lưu metadata.                       |
| **Notification Service** | .NET 8 (ASP.NET Core, SignalR) | PostgreSQL            | Gửi thông báo real-time tới client, lưu trữ notifications.        |
| **AI Service**           | Python (FastAPI)               | PostgreSQL + pgvector | Xử lý RAG, cung cấp tính năng hỏi-đáp AI, Gemini API.             |
| **Message Broker**       | Apache Kafka                   | -                     | Xương sống giao tiếp bất đồng bộ, tách rời các service.           |

---

### **3. Thiết kế Chi tiết các Thành phần**

### **3.1. User Service (.NET)**

- **Trách nhiệm:**
  - Xử lý đăng ký, đăng nhập (JWT).
  - Quản lý thông tin hồ sơ người dùng.
  - Cung cấp thông tin người dùng cho các service khác qua API.
- **Công nghệ:** .NET, ASP.NET Core, EF Core, ASP.NET Core Identity.
- **API Endpoints (Ví dụ):**
  - POST /api/auth/register
  - POST /api/auth/login
  - POST /api/auth/refresh
  - GET /api/users/me
- **Mô hình Dữ liệu (PostgreSQL):** AspNetUsers, AspNetRoles, etc. (do ASP.NET Core Identity quản lý).

### **3.2. Project Service (.NET)**

- **Trách nhiệm:**
  - CRUD (Create, Read, Update, Delete) cho Projects, Tasks, Comments.
  - Xử lý logic nghiệp vụ phức tạp (ví dụ: gán task, thay đổi trạng thái).
  - Tạo embeddings cho tasks và comments (sử dụng Gemini Embedding API).
  - Sản xuất (produce) các sự kiện nghiệp vụ ra Kafka.
- **Công nghệ:** .NET 8, ASP.NET Core, EF Core, Npgsql (với pgvector), Confluent.Kafka client.
- **API Endpoints (Ví dụ):**
  - GET /api/projects
  - POST /api/projects/{projectId}/tasks
  - GET /api/tasks/{taskId}
  - PUT /api/tasks/{taskId}/status
- **Mô hình Dữ liệu (PostgreSQL):** Projects, Tasks, Comments, TaskEmbeddings, CommentEmbeddings.
- **Sự kiện Kafka Sản xuất:** project.task.created, project.task.updated, project.comment.added.

### **3.3. File Service (.NET)**

- **Trách nhiệm:**
  - Xử lý upload file (multipart/form-data).
  - Lưu trữ file vào filesystem hoặc cloud storage (Docker volume).
  - Quản lý metadata file trong PostgreSQL.
  - Sản xuất (produce) sự kiện file.uploaded ra Kafka.
- **Công nghệ:** .NET 8, ASP.NET Core, EF Core, Confluent.Kafka client.
- **API Endpoints:**
  - POST /api/files/upload
  - GET /api/files/{fileId}
  - DELETE /api/files/{fileId}
- **Database:** PostgreSQL
- **Storage:** Local filesystem hoặc Azure Blob Storage / AWS S3.

### **3.4. Notification Service (.NET)**

- **Trách nhiệm:**
  - Tiêu thụ (consume) sự kiện từ Kafka.
  - Xác định người dùng cần nhận thông báo.
  - Lưu trữ notifications vào PostgreSQL.
  - Sử dụng SignalR để đẩy thông báo tới các client đang kết nối.
- **Công nghệ:** .NET 8, ASP.NET Core, SignalR, EF Core, Confluent.Kafka client.
- **SignalR Hub:** NotificationHub với các method như ReceiveNotification(string userId, object message).
- **Database:** PostgreSQL
- **Sự kiện Kafka Tiêu thụ:** project.task.created, project.task.updated, project.comment.added, file.uploaded.

### **3.5. AI Service (Python)**

- **Trách nhiệm:**
  - **RAG Pipeline:** Nhận câu hỏi từ người dùng, generate query embedding, vector similarity search trong PostgreSQL+pgvector.
  - **Question Answering:** Sử dụng Gemini API để generate responses dựa trên retrieved context.
  - **Report Generation:** Tạo báo cáo có cấu trúc từ natural language requests.
  - **Smart Alerts:** Background job phân tích rủi ro, tạo cảnh báo.
- **Công nghệ:** Python 3.11+, FastAPI, Google Gemini API (embedding + generation), psycopg2 (PostgreSQL client), pgvector extension.
- **Database:** PostgreSQL với pgvector extension (shared với Project Service hoặc separate DB).
- **API Endpoints:**
  - POST /api/ai/chat - Endpoint chính cho việc hỏi-đáp.
  - POST /api/ai/generate-report - Generate báo cáo từ natural language.
  - GET /api/ai/alerts - Lấy danh sách smart alerts.

---

### **4. Thiết kế Dữ liệu**

### **4.1. Lược đồ CSDL Quan hệ (PostgreSQL)**

- **Projects**: Id, Name, Description, CreatedAt
- **Tasks**: Id, ProjectId, Title, Description, Status, AssigneeId, DueDate, Priority
- **Comments**: Id, TaskId, AuthorId, Content, CreatedAt
- _(Các bảng khác như Users, Attachments, bảng nối Users_Projects)_

### **4.2. Cấu trúc Dữ liệu Vector với pgvector**

PostgreSQL với extension **pgvector** sẽ được sử dụng để lưu trữ và tìm kiếm embeddings:

**Schema trong Project Service Database:**

```sql
-- Task embeddings
CREATE TABLE task_embeddings (
    task_id UUID PRIMARY KEY REFERENCES tasks(id),
    embedding vector(768), -- Gemini embedding dimension
    created_at TIMESTAMP DEFAULT NOW()
);

-- Comment embeddings
CREATE TABLE comment_embeddings (
    comment_id UUID PRIMARY KEY REFERENCES comments(id),
    task_id UUID REFERENCES tasks(id),
    embedding vector(768),
    created_at TIMESTAMP DEFAULT NOW()
);

-- Create indexes for fast similarity search
CREATE INDEX ON task_embeddings USING ivfflat (embedding vector_cosine_ops);
CREATE INDEX ON comment_embeddings USING ivfflat (embedding vector_cosine_ops);
```

### **4.3. Luồng Dữ liệu cho RAG**

**Ghi (Indexing):**

1. User tạo task/comment trong Project Service.
2. Project Service gọi Gemini Embedding API để tạo embedding.
3. Lưu embedding vào bảng task_embeddings hoặc comment_embeddings trong PostgreSQL.
4. Project Service publish sự kiện Kafka (optional, cho AI Service tracking).

**Đọc (Querying):**

1. User gửi câu hỏi qua POST /api/ai/chat.
2. AI Service generate query embedding bằng Gemini Embedding API.
3. Thực hiện vector similarity search trong PostgreSQL với công thức cosine distance.
4. Lấy top-k documents (tasks, comments) có similarity cao nhất.
5. Combine context từ retrieved documents.
6. Gửi context + câu hỏi tới Gemini Generate API.
7. Trả về response cho user.

### **4.4. MongoDB Schema cho Notifications**

```javascript
{
  _id: ObjectId,
  user_id: UUID,
  type: "task_assigned" | "comment_added" | "status_changed" | "file_uploaded",
  title: String,
  message: String,
  related_entity: {
    type: "task" | "project" | "file",
    id: UUID
  },
  read: Boolean,
  created_at: ISODate
}

// Indexes
db.notifications.createIndex({ user_id: 1, read: 1, created_at: -1 });
db.notifications.createIndex({ created_at: 1 }, { expireAfterSeconds: 2592000 }); // TTL 30 days
```

### **4.5. MSSQL Schema cho File Service**

```sql
CREATE TABLE files (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    original_name NVARCHAR(500) NOT NULL,
    stored_name NVARCHAR(500) NOT NULL,
    mime_type NVARCHAR(100) NOT NULL,
    size BIGINT NOT NULL,
    storage_path NVARCHAR(1000) NOT NULL,
    uploaded_by UNIQUEIDENTIFIER NOT NULL,
    uploaded_at DATETIME2 DEFAULT GETDATE()
);

CREATE INDEX IX_files_uploaded_by ON files(uploaded_by);
CREATE INDEX IX_files_uploaded_at ON files(uploaded_at DESC);
```

---

### **5. Thiết kế Giao tiếp**

### **5.1. Đồng bộ: REST API**

- **Chuẩn:** RESTful, sử dụng JSON.
- **Versioning:** /api/v1/...
- **Authentication:** JWT Bearer Token trong Authorization header.
- **Error Handling:** Sử dụng các mã trạng thái HTTP chuẩn (400, 401, 403, 404, 500) và payload lỗi JSON có cấu trúc.

### **5.2. Bất đồng bộ: Apache Kafka**

- **Topic Naming Convention:** [domain].[entity].[verb] (e.g., project.task.created).
- **Message Format (JSON):**codeJSON
  `{
  "eventId": "uuid-v4",
  "eventType": "project.task.created",
  "timestamp": "iso-8601-timestamp",
  "payload": {
    // Dữ liệu cụ thể của sự kiện
    "taskId": 123,
    "title": "...",
    // ...
  }
}`

### **5.3. Real-time: SignalR**

- Client sẽ thiết lập một kết nối WebSocket tới Notification Service sau khi đăng nhập.
- Service sẽ sử dụng UserId để đẩy thông báo chỉ tới client cụ thể.

---

### **6. Thiết kế Bảo mật**

- **Authentication & Authorization:**
  - Sử dụng ASP.NET Core Identity với JWT. Access token có thời gian sống ngắn (~15 phút), Refresh token có thời gian sống dài hơn (~7 ngày).
  - Áp dụng Authorization Policies ([Authorize(Policy = "IsProjectMember")]) để kiểm tra quyền truy cập chi tiết tại các API endpoint.
- **Data Security:**
  - **In Transit:** Tất cả giao tiếp phải qua HTTPS/TLS.
  - **At Rest:** Mật khẩu được băm bằng bcrypt. Cân nhắc mã hóa CSDL (Transparent Data Encryption - TDE) nếu cần.
- **Network Security:**
  - Triển khai các service trong một mạng riêng ảo (VPC).
  - Sử dụng Network Policies trong Kubernetes để giới hạn giao tiếp giữa các pod/service .
- **Secret Management:**
  - Tuyệt đối không lưu trữ secret (chuỗi kết nối, API key) trong mã nguồn.
  - Sử dụng Azure Key Vault, AWS Secrets Manager, hoặc HashiCorp Vault.

---

### **7. Thiết kế DevOps và Triển khai**

### **7.1. Containerization**

- Tất cả microservices được containerized bằng Docker.
- Docker Compose được sử dụng để orchestrate toàn bộ hệ thống trong môi trường development và testing.
- Production deployment có thể sử dụng Kubernetes (K8s) để điều phối, tự động scale (HPA), và quản lý vòng đời của các container.

**Docker images:**

- `.NET 8 services`: Base image `mcr.microsoft.com/dotnet/aspnet:8.0-alpine`
- `Python AI Service`: Base image `python:3.11-slim`
- `Frontend (Next.js)`: Multi-stage build với Node.js và nginx

### **7.2. CI/CD Pipeline (GitHub Actions)**

**Workflow cho .NET Services:**

```yaml
name: Build and Deploy User Service

on:
  push:
    branches: [main]
    paths: ["services/user-service/**"]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: "8.0.x"

      - name: Restore dependencies
        run: dotnet restore ./services/user-service

      - name: Build
        run: dotnet build --no-restore -c Release ./services/user-service

      - name: Test
        run: dotnet test --no-build --verbosity normal ./services/user-service

      - name: Build Docker image
        run: docker build -t user-service:latest ./services/user-service
```

**Workflow cho Python AI Service:**

```yaml
name: Build and Deploy AI Service

on:
  push:
    branches: [main]
    paths: ["services/ai-service/**"]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup Python
        uses: actions/setup-python@v4
        with:
          python-version: "3.11"

      - name: Install dependencies
        run: |
          cd services/ai-service
          pip install -r requirements.txt

      - name: Run tests
        run: |
          cd services/ai-service
          pytest tests/

      - name: Build Docker image
        run: docker build -t ai-service:latest ./services/ai-service
```

### **7.3. Giám sát và Ghi Log (Monitoring & Logging)**

- **Logging:** Các service sẽ ghi log có cấu trúc (structured logging, e.g., Serilog) ra stdout. Log sẽ được thu thập bởi một agent (Fluentd) và đẩy về một hệ thống quản lý log tập trung như ELK Stack (Elasticsearch, Logstash, Kibana).
- **Monitoring:** Prometheus sẽ được sử dụng để thu thập các metrics từ các service (thông qua dotnet-monitor hoặc các thư viện client). Grafana sẽ được dùng để trực quan hóa các metrics này trên các dashboard.
- **Alerting:** Alertmanager sẽ được cấu hình để gửi cảnh báo (qua Slack, Email) khi các ngưỡng quan trọng bị vi phạm.
