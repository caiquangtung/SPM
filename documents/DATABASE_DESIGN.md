# **Database Design Documentation**

### **Hệ thống Web Quản lý Dự án và Theo dõi Tiến độ Thông minh**

**Phiên bản:** 1.0

**Ngày:** 28/10/2025

---

## **Tổng quan**

Hệ thống sử dụng **Single PostgreSQL Database** với **Multiple Schemas** để quản lý dữ liệu cho tất cả microservices:

- **Database:** `spm_db` (PostgreSQL 16+)
- **Schemas:**
  - `spm_user` - User Service (authentication, users, refresh tokens)
  - `spm_project` - Project Service (projects, tasks, comments, embeddings)
  - `spm_file` - File Service (file metadata, attachments)
  - `spm_notification` - Notification Service (notifications)
  - `spm_ai` - AI Service (conversations, messages, alerts)

**Lợi ích:**

- Đơn giản hóa vận hành (chỉ một database server)
- Dễ dàng cross-schema queries và joins
- Consistent backup và recovery
- Tận dụng tốt pgvector extension cho RAG
- Tối ưu resource usage

---

## **Mục lục**

1. [User Service Database](#1-user-service-database)
2. [Project Service Database](#2-project-service-database)
3. [File Service Database](#3-file-service-database)
4. [Notification Service Database](#4-notification-service-database)
5. [AI Service Database](#5-ai-service-database)
6. [Database Relationships](#6-database-relationships)

---

## **1. User Service Database**

### **1.1. Tổng quan**

- **Database:** PostgreSQL 16+ (shared database instance)
- **Schema:** spm_user
- **ORM:** Entity Framework Core (Code-First)
- **Identity Provider:** ASP.NET Core Identity
- **Migrations:** EF Core Migrations

### **1.2. Schema**

#### **Table: Users**

Quản lý thông tin người dùng cơ bản.

```sql
-- Create schema
CREATE SCHEMA IF NOT EXISTS spm_user;

-- Create tables
CREATE TABLE spm_user.users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) NOT NULL UNIQUE,
    email_confirmed BOOLEAN DEFAULT FALSE,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(255),
    avatar_url TEXT,
    role VARCHAR(20) NOT NULL CHECK (role IN ('Admin', 'PM', 'Member')),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP
);

CREATE INDEX idx_users_email ON spm_user.users(email);
CREATE INDEX idx_users_role ON spm_user.users(role);
CREATE INDEX idx_users_is_active ON spm_user.users(is_active);
```

**Columns:**

| Column          | Type      | Constraints      | Description               |
| --------------- | --------- | ---------------- | ------------------------- |
| id              | UUID      | PRIMARY KEY      | Unique identifier         |
| email           | VARCHAR   | UNIQUE, NOT NULL | Email đăng nhập           |
| email_confirmed | BOOLEAN   | DEFAULT FALSE    | Trạng thái xác thực email |
| password_hash   | VARCHAR   | NOT NULL         | BCrypt hash mật khẩu      |
| full_name       | VARCHAR   | NULLABLE         | Họ tên đầy đủ             |
| avatar_url      | TEXT      | NULLABLE         | URL ảnh đại diện          |
| role            | VARCHAR   | NOT NULL, CHECK  | Vai trò: Admin/PM/Member  |
| is_active       | BOOLEAN   | DEFAULT TRUE     | Tài khoản hoạt động       |
| created_at      | TIMESTAMP | DEFAULT NOW()    | Thời điểm tạo             |
| updated_at      | TIMESTAMP | DEFAULT NOW()    | Thời điểm cập nhật cuối   |
| last_login_at   | TIMESTAMP | NULLABLE         | Thời điểm đăng nhập cuối  |

#### **Table: EmailVerifications**

Quản lý token xác thực email.

```sql
CREATE TABLE email_verifications (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token VARCHAR(255) NOT NULL UNIQUE,
    expires_at TIMESTAMP NOT NULL,
    verified_at TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_email_verifications_user_id ON email_verifications(user_id);
CREATE INDEX idx_email_verifications_token ON email_verifications(token);
CREATE INDEX idx_email_verifications_expires_at ON email_verifications(expires_at);
```

**Columns:**

| Column      | Type      | Constraints   | Description                   |
| ----------- | --------- | ------------- | ----------------------------- |
| id          | UUID      | PRIMARY KEY   | Unique identifier             |
| user_id     | UUID      | FK, NOT NULL  | Foreign key tới users         |
| token       | VARCHAR   | UNIQUE        | Verification token            |
| expires_at  | TIMESTAMP | NOT NULL      | Thời điểm hết hạn             |
| verified_at | TIMESTAMP | NULLABLE      | Thời điểm xác thực thành công |
| created_at  | TIMESTAMP | DEFAULT NOW() | Thời điểm tạo                 |

#### **Table: RefreshTokens**

Quản lý refresh tokens cho JWT authentication.

```sql
CREATE TABLE refresh_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token VARCHAR(255) NOT NULL UNIQUE,
    expires_at TIMESTAMP NOT NULL,
    revoked_at TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_refresh_tokens_user_id ON refresh_tokens(user_id);
CREATE INDEX idx_refresh_tokens_token ON refresh_tokens(token);
CREATE INDEX idx_refresh_tokens_expires_at ON refresh_tokens(expires_at);
```

**Columns:**

| Column     | Type      | Constraints   | Description           |
| ---------- | --------- | ------------- | --------------------- |
| id         | UUID      | PRIMARY KEY   | Unique identifier     |
| user_id    | UUID      | FK, NOT NULL  | Foreign key tới users |
| token      | VARCHAR   | UNIQUE        | Refresh token         |
| expires_at | TIMESTAMP | NOT NULL      | Thời điểm hết hạn     |
| revoked_at | TIMESTAMP | NULLABLE      | Thời điểm revoke      |
| created_at | TIMESTAMP | DEFAULT NOW() | Thời điểm tạo         |

---

## **2. Project Service Database**

### **2.1. Tổng quan**

- **Database:** PostgreSQL 16+ (shared database instance)
- **Schema:** spm_project
- **Extension:** pgvector (cho vector embeddings)
- **ORM:** Entity Framework Core (Code-First)

### **2.2. Schema**

#### **Table: Projects**

Quản lý thông tin dự án.

```sql
-- Create schema
CREATE SCHEMA IF NOT EXISTS spm_project;

-- Enable pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Create tables
CREATE TABLE spm_project.projects (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    created_by UUID NOT NULL REFERENCES spm_user.users(id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE
);

CREATE INDEX idx_projects_created_by ON spm_project.projects(created_by);
CREATE INDEX idx_projects_created_at ON spm_project.projects(created_at);
CREATE INDEX idx_projects_is_active ON spm_project.projects(is_active);
```

**Columns:**

| Column      | Type      | Constraints   | Description         |
| ----------- | --------- | ------------- | ------------------- |
| id          | UUID      | PRIMARY KEY   | Unique identifier   |
| name        | VARCHAR   | NOT NULL      | Tên dự án           |
| description | TEXT      | NULLABLE      | Mô tả dự án         |
| created_by  | UUID      | FK, NOT NULL  | Người tạo dự án     |
| created_at  | TIMESTAMP | DEFAULT NOW() | Thời điểm tạo       |
| updated_at  | TIMESTAMP | DEFAULT NOW() | Thời điểm cập nhật  |
| is_active   | BOOLEAN   | DEFAULT TRUE  | Dự án còn hoạt động |

#### **Table: ProjectMembers**

Bảng nối nhiều-nhiều giữa Projects và Users.

```sql
CREATE TABLE spm_project.project_members (
    project_id UUID NOT NULL REFERENCES spm_project.projects(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES spm_user.users(id) ON DELETE CASCADE,
    role VARCHAR(20) NOT NULL CHECK (role IN ('Owner', 'PM', 'Member')),
    joined_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (project_id, user_id)
);

CREATE INDEX idx_project_members_user_id ON spm_project.project_members(user_id);
CREATE INDEX idx_project_members_role ON spm_project.project_members(role);
```

**Columns:**

| Column     | Type      | Constraints | Description              |
| ---------- | --------- | ----------- | ------------------------ |
| project_id | UUID      | FK, PK      | Foreign key tới projects |
| user_id    | UUID      | FK, PK      | Foreign key tới users    |
| role       | VARCHAR   | NOT NULL    | Vai trò trong dự án      |
| joined_at  | TIMESTAMP | DEFAULT NOW | Thời điểm tham gia       |

#### **Table: Tasks**

Quản lý công việc trong dự án.

```sql
CREATE TABLE spm_project.tasks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    project_id UUID NOT NULL REFERENCES spm_project.projects(id) ON DELETE CASCADE,
    title VARCHAR(500) NOT NULL,
    description TEXT,
    assigned_to UUID REFERENCES spm_user.users(id),
    created_by UUID NOT NULL REFERENCES spm_user.users(id),
    status VARCHAR(20) NOT NULL CHECK (status IN ('ToDo', 'InProgress', 'InReview', 'Done', 'Cancelled')),
    priority VARCHAR(10) NOT NULL CHECK (priority IN ('Low', 'Medium', 'High', 'Critical')),
    due_date TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_tasks_project_id ON spm_project.tasks(project_id);
CREATE INDEX idx_tasks_assigned_to ON spm_project.tasks(assigned_to);
CREATE INDEX idx_tasks_created_by ON spm_project.tasks(created_by);
CREATE INDEX idx_tasks_status ON spm_project.tasks(status);
CREATE INDEX idx_tasks_priority ON spm_project.tasks(priority);
CREATE INDEX idx_tasks_due_date ON spm_project.tasks(due_date);
```

**Columns:**

| Column      | Type      | Constraints     | Description              |
| ----------- | --------- | --------------- | ------------------------ |
| id          | UUID      | PRIMARY KEY     | Unique identifier        |
| project_id  | UUID      | FK, NOT NULL    | Foreign key tới projects |
| title       | VARCHAR   | NOT NULL        | Tiêu đề task             |
| description | TEXT      | NULLABLE        | Mô tả chi tiết           |
| assigned_to | UUID      | FK, NULLABLE    | Người được giao          |
| created_by  | UUID      | FK, NOT NULL    | Người tạo task           |
| status      | VARCHAR   | NOT NULL, CHECK | Trạng thái hiện tại      |
| priority    | VARCHAR   | NOT NULL, CHECK | Độ ưu tiên               |
| due_date    | TIMESTAMP | NULLABLE        | Hạn chót                 |
| created_at  | TIMESTAMP | DEFAULT NOW()   | Thời điểm tạo            |
| updated_at  | TIMESTAMP | DEFAULT NOW()   | Thời điểm cập nhật       |

#### **Table: Comments**

Quản lý bình luận trên tasks.

```sql
CREATE TABLE spm_project.comments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    task_id UUID NOT NULL REFERENCES spm_project.tasks(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES spm_user.users(id),
    content TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_comments_task_id ON spm_project.comments(task_id);
CREATE INDEX idx_comments_user_id ON spm_project.comments(user_id);
CREATE INDEX idx_comments_created_at ON spm_project.comments(created_at);
```

**Columns:**

| Column     | Type      | Constraints   | Description           |
| ---------- | --------- | ------------- | --------------------- |
| id         | UUID      | PRIMARY KEY   | Unique identifier     |
| task_id    | UUID      | FK, NOT NULL  | Foreign key tới tasks |
| user_id    | UUID      | FK, NOT NULL  | Người comment         |
| content    | TEXT      | NOT NULL      | Nội dung bình luận    |
| created_at | TIMESTAMP | DEFAULT NOW() | Thời điểm tạo         |
| updated_at | TIMESTAMP | DEFAULT NOW() | Thời điểm cập nhật    |

#### **Table: TaskEmbeddings**

Lưu trữ vector embeddings cho task descriptions (RAG).

```sql
CREATE TABLE spm_project.task_embeddings (
    task_id UUID PRIMARY KEY REFERENCES spm_project.tasks(id) ON DELETE CASCADE,
    embedding vector(768) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create IVFFlat index for fast similarity search
CREATE INDEX idx_task_embeddings_vector ON spm_project.task_embeddings
    USING ivfflat (embedding vector_cosine_ops)
    WITH (lists = 100);
```

**Columns:**

| Column     | Type        | Constraints   | Description                |
| ---------- | ----------- | ------------- | -------------------------- |
| task_id    | UUID        | PK, FK        | Foreign key tới tasks      |
| embedding  | vector(768) | NOT NULL      | Vector embedding từ Gemini |
| created_at | TIMESTAMP   | DEFAULT NOW() | Thời điểm tạo              |

#### **Table: CommentEmbeddings**

Lưu trữ vector embeddings cho comment content (RAG).

```sql
CREATE TABLE spm_project.comment_embeddings (
    comment_id UUID PRIMARY KEY REFERENCES spm_project.comments(id) ON DELETE CASCADE,
    task_id UUID REFERENCES spm_project.tasks(id) ON DELETE CASCADE,
    embedding vector(768) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create IVFFlat index for fast similarity search
CREATE INDEX idx_comment_embeddings_vector ON spm_project.comment_embeddings
    USING ivfflat (embedding vector_cosine_ops)
    WITH (lists = 100);

CREATE INDEX idx_comment_embeddings_task_id ON spm_project.comment_embeddings(task_id);
```

**Columns:**

| Column     | Type        | Constraints   | Description                |
| ---------- | ----------- | ------------- | -------------------------- |
| comment_id | UUID        | PK, FK        | Foreign key tới comments   |
| task_id    | UUID        | FK            | Reference tới task         |
| embedding  | vector(768) | NOT NULL      | Vector embedding từ Gemini |
| created_at | TIMESTAMP   | DEFAULT NOW() | Thời điểm tạo              |

---

## **3. File Service Database**

### **3.1. Tổng quan**

- **Database:** PostgreSQL 16+ (shared database instance)
- **Schema:** spm_file
- **ORM:** Entity Framework Core (Code-First)
- **Storage:** Local filesystem hoặc cloud storage

### **3.2. Schema**

#### **Table: Files**

Quản lý metadata các file đã upload.

```sql
-- Create schema
CREATE SCHEMA IF NOT EXISTS spm_file;

-- Create tables
CREATE TABLE spm_file.files (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    original_name VARCHAR(500) NOT NULL,
    stored_name VARCHAR(500) NOT NULL,
    mime_type VARCHAR(100) NOT NULL,
    size BIGINT NOT NULL,
    storage_path VARCHAR(1000) NOT NULL,
    uploaded_by UUID NOT NULL,
    uploaded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE
);

CREATE INDEX idx_files_uploaded_by ON spm_file.files(uploaded_by);
CREATE INDEX idx_files_uploaded_at ON spm_file.files(uploaded_at DESC);
CREATE INDEX idx_files_is_deleted ON spm_file.files(is_deleted);
```

**Columns:**

| Column        | Type      | Constraints   | Description                 |
| ------------- | --------- | ------------- | --------------------------- |
| id            | UUID      | PRIMARY KEY   | Unique identifier           |
| original_name | VARCHAR   | NOT NULL      | Tên file gốc                |
| stored_name   | VARCHAR   | NOT NULL      | Tên file lưu trên server    |
| mime_type     | VARCHAR   | NOT NULL      | MIME type (image/png, etc.) |
| size          | BIGINT    | NOT NULL      | Kích thước file (bytes)     |
| storage_path  | VARCHAR   | NOT NULL      | Đường dẫn lưu trữ           |
| uploaded_by   | UUID      | NOT NULL      | User ID người upload        |
| uploaded_at   | TIMESTAMP | DEFAULT NOW() | Thời điểm upload            |
| is_deleted    | BOOLEAN   | DEFAULT FALSE | Soft delete flag            |

#### **Table: TaskAttachments**

Bảng nối nhiều-nhiều giữa Tasks và Files.

```sql
CREATE TABLE spm_file.task_attachments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    task_id UUID NOT NULL,
    file_id UUID NOT NULL REFERENCES spm_file.files(id) ON DELETE CASCADE,
    uploaded_by UUID NOT NULL,
    uploaded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_task_attachments_task_id ON spm_file.task_attachments(task_id);
CREATE INDEX idx_task_attachments_file_id ON spm_file.task_attachments(file_id);
```

**Columns:**

| Column      | Type      | Constraints   | Description           |
| ----------- | --------- | ------------- | --------------------- |
| id          | UUID      | PRIMARY KEY   | Unique identifier     |
| task_id     | UUID      | NOT NULL      | Reference tới task    |
| file_id     | UUID      | FK, NOT NULL  | Foreign key tới files |
| uploaded_by | UUID      | NOT NULL      | Người đính kèm        |
| uploaded_at | TIMESTAMP | DEFAULT NOW() | Thời điểm đính kèm    |

---

## **4. Notification Service Database**

### **4.1. Tổng quan**

- **Database:** PostgreSQL 16+ (shared database instance)
- **Schema:** spm_notification
- **ORM:** Entity Framework Core (Code-First)

### **4.2. Schema**

#### **Table: Notifications**

Quản lý thông báo cho người dùng.

```sql
-- Create schema
CREATE SCHEMA IF NOT EXISTS spm_notification;

-- Create tables
CREATE TABLE spm_notification.notifications (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    type VARCHAR(50) NOT NULL CHECK (type IN ('task_assigned', 'comment_added', 'status_changed', 'file_uploaded')),
    title VARCHAR(255) NOT NULL,
    message TEXT NOT NULL,
    related_entity_type VARCHAR(50),
    related_entity_id UUID,
    read BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_notifications_user_id_read_created ON spm_notification.notifications(user_id, read, created_at DESC);
CREATE INDEX idx_notifications_created_at ON spm_notification.notifications(created_at);

-- Auto-delete old notifications after 30 days (using pg_cron or application job)
```

**Columns:**

| Column              | Type      | Constraints   | Description               |
| ------------------- | --------- | ------------- | ------------------------- |
| id                  | UUID      | PRIMARY KEY   | Unique identifier         |
| user_id             | UUID      | NOT NULL      | Người dùng nhận thông báo |
| type                | VARCHAR   | NOT NULL      | Loại thông báo            |
| title               | VARCHAR   | NOT NULL      | Tiêu đề                   |
| message             | TEXT      | NOT NULL      | Nội dung                  |
| related_entity_type | VARCHAR   | NULLABLE      | Loại entity liên quan     |
| related_entity_id   | UUID      | NULLABLE      | ID entity liên quan       |
| read                | BOOLEAN   | DEFAULT FALSE | Đã đọc?                   |
| created_at          | TIMESTAMP | DEFAULT NOW() | Thời điểm tạo             |

**Notification Types:**

- `task_assigned`: Được gán task mới
- `comment_added`: Có bình luận mới trên task
- `status_changed`: Trạng thái task thay đổi
- `file_uploaded`: File được upload vào task

---

## **5. AI Service Database**

### **5.1. Tổng quan**

- **Database:** PostgreSQL 16+ (shared database instance)
- **Schema:** spm_ai
- **Extension:** pgvector (shared với Project Service)
- **ORM:** SQLAlchemy (Python) hoặc psycopg2

### **5.2. Schema**

#### **Table: Conversations**

Quản lý lịch sử cuộc hội thoại với AI.

```sql
-- Create schema
CREATE SCHEMA IF NOT EXISTS spm_ai;

-- Create tables
CREATE TABLE spm_ai.conversations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    project_id UUID REFERENCES spm_project.projects(id),
    title VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_conversations_user_id ON spm_ai.conversations(user_id);
CREATE INDEX idx_conversations_project_id ON spm_ai.conversations(project_id);
CREATE INDEX idx_conversations_updated_at ON spm_ai.conversations(updated_at DESC);
```

**Columns:**

| Column     | Type      | Constraints   | Description             |
| ---------- | --------- | ------------- | ----------------------- |
| id         | UUID      | PRIMARY KEY   | Unique identifier       |
| user_id    | UUID      | NOT NULL      | Người dùng              |
| project_id | UUID      | FK, NULLABLE  | Dự án (nếu có)          |
| title      | VARCHAR   | NULLABLE      | Tiêu đề hội thoại       |
| created_at | TIMESTAMP | DEFAULT NOW() | Thời điểm tạo           |
| updated_at | TIMESTAMP | DEFAULT NOW() | Thời điểm cập nhật cuối |

#### **Table: Messages**

Quản lý các tin nhắn trong conversation.

```sql
CREATE TABLE spm_ai.messages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    conversation_id UUID NOT NULL REFERENCES spm_ai.conversations(id) ON DELETE CASCADE,
    role VARCHAR(20) NOT NULL CHECK (role IN ('user', 'assistant')),
    content TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_messages_conversation_id ON spm_ai.messages(conversation_id);
CREATE INDEX idx_messages_created_at ON spm_ai.messages(created_at);
```

**Columns:**

| Column          | Type      | Constraints     | Description       |
| --------------- | --------- | --------------- | ----------------- |
| id              | UUID      | PRIMARY KEY     | Unique identifier |
| conversation_id | UUID      | FK, NOT NULL    | Foreign key       |
| role            | VARCHAR   | NOT NULL, CHECK | user/assistant    |
| content         | TEXT      | NOT NULL        | Nội dung tin nhắn |
| created_at      | TIMESTAMP | DEFAULT NOW()   | Thời điểm tạo     |

#### **Table: Alerts**

Quản lý cảnh báo thông minh từ AI.

```sql
CREATE TABLE spm_ai.alerts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    project_id UUID REFERENCES spm_project.projects(id) ON DELETE CASCADE,
    type VARCHAR(50) NOT NULL,
    severity VARCHAR(20) NOT NULL CHECK (severity IN ('Low', 'Medium', 'High', 'Critical')),
    message TEXT NOT NULL,
    data JSONB,
    resolved BOOLEAN DEFAULT FALSE,
    resolved_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_alerts_project_id ON spm_ai.alerts(project_id);
CREATE INDEX idx_alerts_resolved ON spm_ai.alerts(resolved);
CREATE INDEX idx_alerts_created_at ON spm_ai.alerts(created_at DESC);
CREATE INDEX idx_alerts_severity ON spm_ai.alerts(severity);
```

**Columns:**

| Column      | Type      | Constraints     | Description         |
| ----------- | --------- | --------------- | ------------------- |
| id          | UUID      | PRIMARY KEY     | Unique identifier   |
| project_id  | UUID      | FK, NULLABLE    | Dự án liên quan     |
| type        | VARCHAR   | NOT NULL        | Loại cảnh báo       |
| severity    | VARCHAR   | NOT NULL, CHECK | Mức độ nghiêm trọng |
| message     | TEXT      | NOT NULL        | Nội dung cảnh báo   |
| data        | JSONB     | NULLABLE        | Metadata bổ sung    |
| resolved    | BOOLEAN   | DEFAULT FALSE   | Đã xử lý?           |
| resolved_at | TIMESTAMP | NULLABLE        | Thời điểm resolve   |
| created_at  | TIMESTAMP | DEFAULT NOW()   | Thời điểm tạo       |

**Alert Types:**

- `task_overdue`: Task quá hạn
- `task_inactive`: Task không có hoạt động lâu
- `task_negative_sentiment`: Comment có sentiment tiêu cực
- `user_overload`: User bị quá tải công việc
- `project_delay`: Dự án có nguy cơ trễ tiến độ

---

## **6. Database Relationships**

### **6.1. ERD Overview**

```
┌─────────────┐
│   Users     │
│  (UUID id)  │
└──────┬──────┘
       │
       ├─────────────────────────────────────┐
       │                                     │
       │ 1:N                                │ 1:N
       │                                     │
┌──────▼──────┐                      ┌───────▼──────────┐
│  Projects   │                      │   Tasks          │
│  (UUID id)  │                      │  (UUID id)       │
│             │                      │                  │
│ N:N         │                      │ N:N              │
└──────┬──────┘                      └──────┬───────────┘
       │                                     │
       │ N:N                                │ 1:N
       │                                     │
┌──────▼──────┐                      ┌───────▼───────────┐
│ProjectMems  │                      │   Comments        │
│(proj_id,    │                      │  (UUID id)        │
│ user_id)    │                      │                   │
└─────────────┘                      └───────┬───────────┘
                                             │
                                             │ 1:1
                                             │
                                    ┌────────▼──────────────┐
                                    │ CommentEmbeddings    │
                                    │ (comment_id)         │
                                    └──────────────────────┘

┌─────────────┐
│   Files     │
│  (GUID id)  │
└──────┬──────┘
       │
       │ N:N
       │
┌──────▼──────────────┐
│ TaskAttachments     │
│ (task_id, file_id)  │
└─────────────────────┘

[Note: Notifications and AI tables are separate and reference user_id/project_id via application logic]
```

### **6.2. Foreign Key Relationships**

| From Table          | To Table      | Relationship   | Cascade           |
| ------------------- | ------------- | -------------- | ----------------- |
| email_verifications | users         | 1:N            | ON DELETE CASCADE |
| refresh_tokens      | users         | 1:N            | ON DELETE CASCADE |
| projects            | users         | N:1            | -                 |
| project_members     | projects      | N:1            | ON DELETE CASCADE |
| project_members     | users         | N:1            | ON DELETE CASCADE |
| tasks               | projects      | N:1            | ON DELETE CASCADE |
| tasks               | users         | N:1 (assigned) | -                 |
| tasks               | users         | N:1 (created)  | -                 |
| comments            | tasks         | N:1            | ON DELETE CASCADE |
| comments            | users         | N:1            | -                 |
| task_embeddings     | tasks         | 1:1            | ON DELETE CASCADE |
| comment_embeddings  | comments      | 1:1            | ON DELETE CASCADE |
| comment_embeddings  | tasks         | N:1            | ON DELETE CASCADE |
| task_attachments    | files         | N:1            | ON DELETE CASCADE |
| messages            | conversations | N:1            | ON DELETE CASCADE |
| alerts              | projects      | N:1            | ON DELETE CASCADE |

---

## **7. Migration Strategy**

### **7.1. Initial Setup**

**Single PostgreSQL Database với Multiple Schemas:**

```bash
# Create database
createdb spm_db

# Connect và setup
psql spm_db << EOF

-- Enable pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Create schemas
CREATE SCHEMA IF NOT EXISTS spm_user;
CREATE SCHEMA IF NOT EXISTS spm_project;
CREATE SCHEMA IF NOT EXISTS spm_file;
CREATE SCHEMA IF NOT EXISTS spm_notification;
CREATE SCHEMA IF NOT EXISTS spm_ai;

-- Set default search_path cho convenience
ALTER DATABASE spm_db SET search_path TO public, spm_user, spm_project, spm_file, spm_notification, spm_ai;

EOF
```

### **7.2. Running Migrations**

**Entity Framework Core (.NET):**

```bash
# User Service
cd services/user-service
dotnet ef migrations add InitialCreate --context UserDbContext
dotnet ef database update

# Project Service
cd services/project-service
dotnet ef migrations add InitialCreate --context ProjectDbContext
dotnet ef database update

# File Service
cd services/file-service
dotnet ef migrations add InitialCreate --context FileDbContext
dotnet ef database update

# Notification Service
cd services/notification-service
dotnet ef migrations add InitialCreate --context NotificationDbContext
dotnet ef database update
```

**Note:** Tất cả migrations chạy trên cùng một database (`spm_db`) nhưng với schemas khác nhau để tách biệt logical domains.

**Alembic (Python AI Service):**

```bash
cd services/ai-service
alembic init alembic
alembic revision --autogenerate -m "Initial schema"
alembic upgrade head
```

---

## **8. Database Backup & Recovery**

### **8.1. PostgreSQL Single Database Backup**

**Daily backup (full database):**

```bash
pg_dump -U postgres -h localhost -d spm_db > backup_spm_$(date +%Y%m%d).sql
```

**Schema-specific backup (if needed):**

```bash
pg_dump -U postgres -h localhost -d spm_db -n spm_user > backup_user_$(date +%Y%m%d).sql
pg_dump -U postgres -h localhost -d spm_db -n spm_project > backup_project_$(date +%Y%m%d).sql
pg_dump -U postgres -h localhost -d spm_db -n spm_file > backup_file_$(date +%Y%m%d).sql
pg_dump -U postgres -h localhost -d spm_db -n spm_notification > backup_notification_$(date +%Y%m%d).sql
pg_dump -U postgres -h localhost -d spm_db -n spm_ai > backup_ai_$(date +%Y%m%d).sql
```

**Restore:**

```bash
psql -U postgres -h localhost -d spm_db < backup_spm_20251028.sql
```

---

## **9. Performance Optimization**

### **9.1. Indexing Strategy**

- Tất cả foreign keys có index
- Filterable columns có index (status, role, priority, etc.)
- Timestamp columns có index cho sorting
- Vector columns có IVFFlat/HNSW index

### **9.2. Query Optimization**

- Sử dụng database-specific optimizations
- Connection pooling
- Prepared statements
- Batch operations cho bulk inserts

### **9.3. Maintenance**

- Vacuum PostgreSQL databases định kỳ
- Update statistics (ANALYZE)
- Monitor slow queries
- Archive old data (soft delete strategy)

---

## **10. Security Considerations**

1. **Credentials:** Lưu connection strings trong Secret Manager (Key Vault / Vault)
2. **Encryption:** Enable TLS cho tất cả database connections
3. **Access Control:** Database users với least privilege permissions
4. **SQL Injection Prevention:** Sử dụng parameterized queries / ORM
5. **Audit Logging:** Track schema changes và sensitive data access

---

**Kết thúc tài liệu**
