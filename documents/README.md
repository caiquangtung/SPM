# Smart Project Management System Documentation

**Hệ thống Web Quản lý Dự án và Theo dõi Tiến độ Thông minh**

Version: 1.1 | Date: 19/04/2026

---

## 📚 Tài liệu

### **Core Documents**

| Document                                               | Description                                                        |
| ------------------------------------------------------ | ------------------------------------------------------------------ |
| **[SRS.md](./SRS.md)**                                 | Đặc tả Yêu cầu Phần mềm (Functional & Non-functional Requirements) |
| **[SDD.md](./SDD.md)**                                 | Thiết kế Hệ thống (Architecture, Components, APIs)                 |
| **[DATABASE_DESIGN.md](./DATABASE_DESIGN.md)**         | Database Schema Documentation (Schemas, Tables, Indexes)           |
| **[IMPLEMENTATION_PLAN.md](./IMPLEMENTATION_PLAN.md)** | Implementation Roadmap & Sprint Breakdown                          |

### **Decision Documents**

| Document                                               | Description                               |
| ------------------------------------------------------ | ----------------------------------------- |
| **[DATABASE_COMPARISON.md](./DATABASE_COMPARISON.md)** | Multi-DB vs Single DB Analysis & Decision |

### **Technical Documentation**

| Document                         | Description                                         |
| -------------------------------- | --------------------------------------------------- |
| **[KAFKA.md](./KAFKA.md)**       | Kafka Integration & Event-Driven Architecture Guide |
| **[COMMANDS.md](./COMMANDS.md)** | Common Commands Reference                           |

---

## 🎯 Project Overview

### **What Is This System?**

A smart project management web application with **AI-powered assistant** using RAG (Retrieval-Augmented Generation) technology.

**Key Features:**

- 👥 User management & authentication
- 📋 Project & task management (Kanban board)
- 💬 Real-time collaboration
- 🤖 AI assistant (natural language queries)
- 📊 Auto-generated reports
- ⚠️ Smart risk alerts
- 📁 File attachments

### **Technology Stack**

| Layer                | Technology                                              |
| -------------------- | ------------------------------------------------------- |
| **Frontend**         | Next.js + React + TypeScript                            |
| **Backend**          | .NET 8 (ASP.NET Core)                                   |
| **AI Service**       | Python (FastAPI)                                        |
| **Database**         | PostgreSQL 16+ (single DB, multiple schemas + pgvector) |
| **Message Broker**   | Apache Kafka                                            |
| **Real-time**        | SignalR (WebSocket)                                     |
| **Containerization** | Docker + Docker Compose                                 |
| **CI/CD**            | GitHub Actions                                          |

---

## 🏗️ Architecture

**Microservices Architecture** with **Single PostgreSQL Database**:

```
┌─────────────┐
│  Next.js    │
│   Frontend  │
└──────┬──────┘
       │
┌──────▼────────────────────────────────────┐
│          API Gateway (YARP)               │
└─┬──────┬──────┬──────┬──────┬────────────┘
  │      │      │      │      │
  ▼      ▼      ▼      ▼      ▼
 User  Proj   File  Notif   AI
  │      │      │      │      │
  └──────┴──────┴──────┴──────┘
         ┌─────────────┐
         │ PostgreSQL  │
         │ (spm_db)    │
         │ + pgvector  │
         └─────────────┘

         ┌─────────────┐
         │   Kafka     │
         │  (Events)   │
         └─────────────┘
```

**Database Organization:**

- `spm_user` - Users, authentication
- `spm_project` - Projects, tasks, comments, embeddings
- `spm_file` - File metadata
- `spm_notification` - Notifications
- `spm_ai` - Conversations, alerts

---

## 🚀 Quick Start

### **Prerequisites**

- Docker & Docker Compose
- .NET 8 SDK
- Python 3.11+
- Node.js 18+

### **Run Locally**

```bash
# Clone repository
git clone <repo-url>

# Start all services
docker-compose up -d

# Access application
# Frontend: http://localhost:3000
# API Gateway: http://localhost:5000
# PostgreSQL: localhost:5432
```

---

## 📖 Reading Guide

### **For Project Managers**

1. Start with [SRS.md](./SRS.md) for requirements
2. Review [IMPLEMENTATION_PLAN.md](./IMPLEMENTATION_PLAN.md) for timeline

### **For Developers**

1. Read [SDD.md](./SDD.md) for architecture
2. Check [DATABASE_DESIGN.md](./DATABASE_DESIGN.md) for schema
3. Follow [IMPLEMENTATION_PLAN.md](./IMPLEMENTATION_PLAN.md) for implementation

### **For Architects**

1. Study [SDD.md](./SDD.md) architecture decisions
2. Review [DATABASE_COMPARISON.md](./DATABASE_COMPARISON.md) for tech choices
3. Validate [DATABASE_DESIGN.md](./DATABASE_DESIGN.md) schema design

---

## 🤝 Contributing

**Development Workflow:**

1. Create feature branch from `main`
2. Implement following coding standards
3. Write tests (>80% coverage)
4. Submit pull request
5. Code review required

---

## 📝 Status

**Current Phase:** Delivery - Sprint 2 Complete, Sprint 3 Next

**Next Steps:**

- [x] Setup project structure
- [x] Implement infrastructure (Docker, Kafka)
- [x] Build User Service (Sprint 1)
- [x] Build Project & File Services (Sprint 2 implementation)
- [x] Complete Sprint 2 test pass and sign-off
- [ ] Start Sprint 3 (Notification Service)
- [ ] Start Sprint 4 (AI Service hardening/integration)

---

## 📞 Contact

**Project Lead:** [Your Name]

**Team:** [Team Members]

**Email:** [contact@example.com]

---

**Last Updated:** April 19, 2026

**Version:** 1.1
