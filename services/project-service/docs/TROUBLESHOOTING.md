# Project Service - Troubleshooting Guide

Hướng dẫn chi tiết cách fix các lỗi thường gặp khi setup và chạy Project Service.

---

## 📋 Mục lục

- [1. Lỗi Gemini API](#1-lỗi-gemini-api)
- [2. Lỗi Embedding Generation Failed](#2-lỗi-embedding-generation-failed)
- [3. Lỗi Vector Search không hoạt động](#3-lỗi-vector-search-không-hoạt-động)
- [4. Lỗi pgvector Extension](#4-lỗi-pgvector-extension)
- [5. Lỗi Migration với Vector Type](#5-lỗi-migration-với-vector-type)
- [6. Lỗi Kafka Producer](#6-lỗi-kafka-producer)
- [7. Lỗi JWT Authentication](#7-lỗi-jwt-authentication)

---

## 1. Lỗi Gemini API

### 🔴 Triệu chứng

```
InvalidOperationException: Gemini API key is not configured
```

hoặc

```
HttpRequestException: Gemini API returned 401: Unauthorized
```

### ✅ Giải pháp

#### **1.1. Kiểm tra API Key**

**Cách 1: appsettings.json**

```json
{
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY_HERE"
  }
}
```

**Cách 2: Environment Variable**

```bash
export GEMINI_API_KEY=your_api_key_here
```

**Cách 3: Docker Compose**

```yaml
environment:
  - GEMINI_API_KEY=${GEMINI_API_KEY}
```

#### **1.2. Verify API Key**

Test API key với curl:

```bash
curl "https://generativelanguage.googleapis.com/v1beta/models/embedding-001:embedContent?key=YOUR_API_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "models/embedding-001",
    "content": {"parts": [{"text": "test"}]}
  }'
```

#### **1.3. Rate Limiting**

Nếu gặp `429 Too Many Requests`:

- Implement retry logic với exponential backoff
- Reduce embedding generation frequency
- Use batch requests nếu Gemini API hỗ trợ

---

## 2. Lỗi Embedding Generation Failed

### 🔴 Triệu chứng

- Task/Comment được tạo nhưng không có embedding
- Search không tìm thấy task mới tạo
- Logs show: "Error generating embedding"

### ✅ Giải pháp

#### **2.1. Kiểm tra Logs**

```bash
# Check application logs
docker logs project-service

# Look for embedding errors
grep -i "embedding" logs/app.log
```

#### **2.2. Verify Embedding Service**

Test embedding service manually:

```csharp
// In Program.cs or test
var embeddingService = serviceProvider.GetService<IEmbeddingService>();
var embedding = await embeddingService.GenerateEmbeddingAsync("test text");
```

#### **2.3. Regenerate Embeddings**

Tạo endpoint để regenerate embeddings:

```csharp
[HttpPost("tasks/{id}/regenerate-embedding")]
public async Task<IActionResult> RegenerateEmbedding(Guid id)
{
    var task = await _tasks.GetByIdAsync(id);
    if (task == null) return NotFound();

    // Regenerate embedding
    await _embeddingService.RegenerateEmbeddingAsync(task);
    return Ok();
}
```

#### **2.4. Check Network Connectivity**

```bash
# Test Gemini API connectivity
curl -I https://generativelanguage.googleapis.com
```

---

## 3. Lỗi Vector Search không hoạt động

### 🔴 Triệu chứng

- Search endpoint trả về empty results
- Error: "operator does not exist: vector <=> vector"
- Search chậm hoặc timeout

### ✅ Giải pháp

#### **3.1. Verify pgvector Extension**

```sql
-- Check if extension is installed
SELECT * FROM pg_extension WHERE extname = 'vector';

-- If not, install it
CREATE EXTENSION IF NOT EXISTS vector;
```

#### **3.2. Check Embeddings Exist**

```sql
-- Check if embeddings exist
SELECT COUNT(*) FROM spm_project.task_embeddings;

-- Check if task has embedding
SELECT t.id, t.title, te.embedding IS NOT NULL as has_embedding
FROM spm_project.tasks t
LEFT JOIN spm_project.task_embeddings te ON t.id = te.task_id
LIMIT 10;
```

#### **3.3. Test Vector Query**

```sql
-- Test cosine distance operator
SELECT
    t.id,
    t.title,
    1 - (te.embedding <=> '[0.1,0.2,...]'::vector) as similarity
FROM spm_project.tasks t
INNER JOIN spm_project.task_embeddings te ON t.id = te.task_id
ORDER BY te.embedding <=> '[0.1,0.2,...]'::vector
LIMIT 10;
```

#### **3.4. Create Indexes for Performance**

```sql
-- Create IVFFlat index for faster search
CREATE INDEX idx_task_embeddings_vector ON spm_project.task_embeddings
USING ivfflat (embedding vector_cosine_ops)
WITH (lists = 100);
```

**Note**: IVFFlat index cần có data trước khi tạo (ít nhất vài trăm rows).

---

## 4. Lỗi pgvector Extension

### 🔴 Triệu chứng

```
ERROR: extension "vector" does not exist
```

hoặc

```
ERROR: could not open extension control file
```

### ✅ Giải pháp

#### **4.1. Install pgvector Extension**

**PostgreSQL 16+**:

```sql
-- Connect to database
\c spm_db

-- Install extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Verify installation
SELECT * FROM pg_extension WHERE extname = 'vector';
```

#### **4.2. Docker Setup**

Nếu dùng Docker, đảm bảo PostgreSQL image có pgvector:

```yaml
postgres:
  image: pgvector/pgvector:pg16
  # hoặc
  image: ankane/pgvector:v0.5.0
```

#### **4.3. Manual Installation**

Nếu PostgreSQL không có pgvector:

```bash
# Install pgvector from source
git clone --branch v0.5.0 https://github.com/pgvector/pgvector.git
cd pgvector
make
make install
```

---

## 5. Lỗi Migration với Vector Type

### 🔴 Triệu chứng

```
Unable to create a 'DbContext' of type 'ProjectDbContext'
The 'Vector' property could not be mapped to the database type 'vector(768)'
```

### ✅ Giải pháp

#### **5.1. Verify pgvector Package**

```xml
<!-- project-service.csproj -->
<PackageReference Include="Pgvector.EntityFrameworkCore" Version="0.2.0" />
```

#### **5.2. Check Program.cs Configuration**

```csharp
options.UseNpgsql(connectionString, npgsqlOptions =>
{
    npgsqlOptions.UseVector(); // Enable pgvector support
});
```

#### **5.3. Recreate Migration**

```bash
# Remove old migration
dotnet ef migrations remove

# Create new migration
dotnet ef migrations add AddEmbeddings --context ProjectDbContext

# Apply migration
dotnet ef database update --context ProjectDbContext
```

#### **5.4. Verify Migration**

Check migration file có đúng:

```csharp
migrationBuilder.HasPostgresExtension("vector");
entity.Property(e => e.Embedding)
    .HasColumnType("vector(768)")
    .IsRequired();
```

---

## 6. Lỗi Kafka Producer

### 🔴 Triệu chứng

```
Confluent.Kafka.KafkaException: Local: Broker transport failure
```

hoặc

```
Failed to publish event: project.task.created
```

### ✅ Giải pháp

#### **6.1. Check Kafka Connection**

```bash
# Test Kafka connectivity
docker exec -it kafka kafka-topics.sh --list --bootstrap-server localhost:9092
```

#### **6.2. Verify Configuration**

```json
{
  "Kafka": {
    "BootstrapServers": "kafka:9092" // hoặc "localhost:9092" cho local
  }
}
```

#### **6.3. Check Topics Exist**

```bash
# List topics
docker exec -it kafka kafka-topics.sh --list --bootstrap-server localhost:9092

# Create topic if missing
docker exec -it kafka kafka-topics.sh --create \
  --topic project.task.created \
  --bootstrap-server localhost:9092 \
  --partitions 3 \
  --replication-factor 1
```

#### **6.4. Test Producer**

```csharp
// Manual test
var producer = new KafkaProducerService(configuration, logger);
await producer.PublishTaskCreatedAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Test");
```

---

## 7. Lỗi JWT Authentication

### 🔴 Triệu chứng

```
401 Unauthorized
```

hoặc

```
Invalid token
```

### ✅ Giải pháp

#### **7.1. Verify JWT Configuration**

```json
{
  "JWT": {
    "SecretKey": "your-secret-key-at-least-32-characters",
    "Issuer": "spm-api-gateway",
    "Audience": "spm-services"
  }
}
```

#### **7.2. Check Token Format**

Token phải có format:

```
Authorization: Bearer <token>
```

#### **7.3. Verify Token Claims**

Token phải có claim `userId`:

```csharp
// In user-service, ensure token has userId claim
new Claim("userId", user.Id.ToString())
```

#### **7.4. Test Authentication**

```bash
# Get token from user-service
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"password"}'

# Use token in project-service
curl -X GET http://localhost:5002/api/projects \
  -H "Authorization: Bearer <token>"
```

---

## 🔍 Debugging Tips

### **Enable Detailed Logging**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### **Check Database Connection**

```bash
# Test PostgreSQL connection
psql -h localhost -U spm_user -d spm_db

# Check schema
\dn
\dt spm_project.*
```

### **Monitor Embedding Generation**

```csharp
// Add logging in EmbeddingService
_logger.LogInformation("Generating embedding for text: {Text}", text);
_logger.LogInformation("Embedding generated: {Dimensions} dimensions", embeddingArray.Length);
```

---

## 📚 Related Documentation

- [ARCHITECTURE_DECISIONS.md](./ARCHITECTURE_DECISIONS.md) - Architecture decisions
- [QUICK_FIX.md](./QUICK_FIX.md) - Quick fixes
- [README.md](../README.md) - Project Service overview

---

**Last Updated:** November 12, 2025
