# Quick Fix Guide - Project Service

Hướng dẫn nhanh để fix các lỗi thường gặp. Xem [TROUBLESHOOTING.md](./TROUBLESHOOTING.md) để biết chi tiết.

---

## 🚨 Lỗi Gemini API Key

### Fix nhanh

1. **Set API key trong appsettings.json**:

```json
{
  "Gemini": {
    "ApiKey": "YOUR_API_KEY_HERE"
  }
}
```

2. **Hoặc set environment variable**:

```bash
export GEMINI_API_KEY=your_api_key_here
```

3. **Restart service**

---

## 🚨 Lỗi pgvector Extension

### Fix nhanh

1. **Connect to PostgreSQL**:

```bash
psql -h localhost -U spm_user -d spm_db
```

2. **Install extension**:

```sql
CREATE EXTENSION IF NOT EXISTS vector;
```

3. **Verify**:

```sql
SELECT * FROM pg_extension WHERE extname = 'vector';
```

---

## 🚨 Lỗi Embedding Generation Failed

### Fix nhanh

1. **Check logs**:

```bash
docker logs project-service | grep -i embedding
```

2. **Verify API key**:

```bash
curl "https://generativelanguage.googleapis.com/v1beta/models/embedding-001:embedContent?key=YOUR_KEY" \
  -H "Content-Type: application/json" \
  -d '{"model":"models/embedding-001","content":{"parts":[{"text":"test"}]}}'
```

3. **Regenerate embedding manually** (nếu có endpoint)

---

## 🚨 Lỗi Vector Search không hoạt động

### Fix nhanh

1. **Check embeddings exist**:

```sql
SELECT COUNT(*) FROM spm_project.task_embeddings;
```

2. **Check pgvector extension**:

```sql
SELECT * FROM pg_extension WHERE extname = 'vector';
```

3. **Test query**:

```sql
SELECT t.id, t.title
FROM spm_project.tasks t
INNER JOIN spm_project.task_embeddings te ON t.id = te.task_id
LIMIT 1;
```

---

## 🚨 Lỗi Migration với Vector Type

### Fix nhanh

1. **Verify package**:

```xml
<PackageReference Include="Pgvector.EntityFrameworkCore" Version="0.2.0" />
```

2. **Check Program.cs**:

```csharp
options.UseNpgsql(connectionString, npgsqlOptions =>
{
    npgsqlOptions.UseVector();
});
```

3. **Recreate migration**:

```bash
dotnet ef migrations remove
dotnet ef migrations add AddEmbeddings
dotnet ef database update
```

---

## 🚨 Lỗi Kafka Connection

### Fix nhanh

1. **Check Kafka is running**:

```bash
docker ps | grep kafka
```

2. **Test connection**:

```bash
docker exec -it kafka kafka-topics.sh --list --bootstrap-server localhost:9092
```

3. **Update configuration**:

```json
{
  "Kafka": {
    "BootstrapServers": "kafka:9092" // Docker network
    // hoặc "localhost:9092" cho local
  }
}
```

---

## 🚨 Lỗi JWT Authentication

### Fix nhanh

1. **Check JWT config**:

```json
{
  "JWT": {
    "SecretKey": "your-secret-key-at-least-32-characters",
    "Issuer": "spm-api-gateway",
    "Audience": "spm-services"
  }
}
```

2. **Verify token format**:

```
Authorization: Bearer <token>
```

3. **Get new token**:

```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"password"}'
```

---

## 🔧 Common Commands

### **Database**

```bash
# Connect to database
psql -h localhost -U spm_user -d spm_db

# Check schemas
\dn

# Check tables
\dt spm_project.*

# Check embeddings
SELECT COUNT(*) FROM spm_project.task_embeddings;
```

### **Migrations**

```bash
# Create migration
dotnet ef migrations add MigrationName --context ProjectDbContext

# Apply migration
dotnet ef database update --context ProjectDbContext

# Remove last migration
dotnet ef migrations remove --context ProjectDbContext
```

### **Docker**

```bash
# View logs
docker logs project-service

# Restart service
docker restart project-service

# Check service status
docker ps | grep project-service
```

### **Testing**

```bash
# Test embedding generation
curl -X POST http://localhost:5002/api/tasks \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"projectId":"...","title":"Test Task","description":"Test"}'

# Test search
curl -X POST http://localhost:5002/api/tasks/search \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"query":"test","topK":10}'
```

---

## 📚 Related Documentation

- [TROUBLESHOOTING.md](./TROUBLESHOOTING.md) - Detailed troubleshooting guide
- [ARCHITECTURE_DECISIONS.md](./ARCHITECTURE_DECISIONS.md) - Architecture decisions
- [README.md](../README.md) - Project Service overview

---

**Last Updated:** November 12, 2025
