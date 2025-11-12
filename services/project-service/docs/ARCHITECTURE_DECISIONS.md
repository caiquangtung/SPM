# Architecture Decisions - Project Service

Tài liệu này giải thích các quyết định kiến trúc quan trọng trong Project Service, bao gồm lý do tại sao chúng ta chọn các giải pháp cụ thể.

---

## 📋 Mục lục

- [1. Tại sao sử dụng Vector type từ Pgvector thay vì float[]?](#1-tại-sao-sử-dụng-vector-type-từ-pgvector-thay-vì-float)
- [2. Tại sao tách embeddings ra tables riêng?](#2-tại-sao-tách-embeddings-ra-tables-riêng)
- [3. Tại sao auto-generate embeddings với fire-and-forget pattern?](#3-tại-sao-auto-generate-embeddings-với-fire-and-forget-pattern)
- [4. Tại sao sử dụng cosine distance cho vector similarity search?](#4-tại-sao-sử-dụng-cosine-distance-cho-vector-similarity-search)
- [5. Tại sao không await embedding generation trong transaction?](#5-tại-sao-không-await-embedding-generation-trong-transaction)

---

## 1. Tại sao sử dụng Vector type từ Pgvector thay vì float[]?

### 🎯 Quyết định: Sử dụng `Vector` type từ `Pgvector.EntityFrameworkCore` package

### ✅ Lý do chọn giải pháp hiện tại

#### 1. **Type Safety & Clarity**

**Vấn đề với float[]**:

- Không rõ ràng đây là vector embedding
- Dễ nhầm lẫn với array thông thường
- Không có type checking tại compile time

**Giải pháp hiện tại**:

```csharp
// ✅ Rõ ràng, type-safe
public Vector Embedding { get; set; }

// ❌ Không rõ ràng
public float[] Embedding { get; set; }
```

#### 2. **Tích hợp tốt hơn với PostgreSQL**

**Vấn đề với float[]**:

- Cần manual mapping với `vector(768)` type trong PostgreSQL
- Khó sử dụng các toán tử vector của pgvector
- Phải convert thủ công khi query

**Giải pháp hiện tại**:

- `Vector` type tự động map với `vector(768)` trong PostgreSQL
- Hỗ trợ trực tiếp các toán tử vector (`<=>`, `<->`, `<#>`)
- EF Core tự động handle conversion

#### 3. **Performance & Optimization**

**Vấn đề với float[]**:

- Không tối ưu cho vector operations
- Phải serialize/deserialize thủ công

**Giải pháp hiện tại**:

- `Vector` type được optimize cho vector operations
- Native support cho pgvector indexes (IVFFlat, HNSW)
- Better performance khi query với vector operators

### 📦 Package Used

```xml
<PackageReference Include="Pgvector.EntityFrameworkCore" Version="0.2.0" />
```

### 🔄 Khi nào nên xem xét thay đổi?

- Nếu pgvector package không được maintain
- Nếu cần support multiple vector dimensions động
- Nếu cần custom vector operations không được hỗ trợ

---

## 2. Tại sao tách embeddings ra tables riêng?

### 🎯 Quyết định: Tách `task_embeddings` và `comment_embeddings` thành tables riêng

### ✅ Lý do chọn giải pháp hiện tại

#### 1. **Separation of Concerns**

**Vấn đề nếu embed trong cùng table**:

- Mixing business data với AI/ML data
- Khó maintain và scale
- Embeddings có thể null (chưa generate)

**Giải pháp hiện tại**:

```sql
-- Business data
CREATE TABLE tasks (
    id UUID PRIMARY KEY,
    title VARCHAR(255),
    description TEXT,
    ...
);

-- AI/ML data (separate)
CREATE TABLE task_embeddings (
    task_id UUID PRIMARY KEY REFERENCES tasks(id),
    embedding vector(768),
    created_at TIMESTAMP
);
```

#### 2. **Performance Optimization**

**Lợi ích**:

- Embeddings không load khi query tasks thông thường
- Có thể tạo indexes riêng cho embeddings (IVFFlat, HNSW)
- Faster queries khi không cần embeddings

#### 3. **Flexibility**

**Lợi ích**:

- Có thể regenerate embeddings mà không ảnh hưởng business data
- Có thể support multiple embedding models (nếu cần)
- Dễ dàng add metadata cho embeddings (model version, etc.)

### 🔄 Khi nào nên xem xét thay đổi?

- Nếu embeddings luôn required (không null)
- Nếu performance không phải concern
- Nếu muốn simplify schema

---

## 3. Tại sao auto-generate embeddings với fire-and-forget pattern?

### 🎯 Quyết định: Generate embeddings async, không await trong transaction

### ✅ Lý do chọn giải pháp hiện tại

#### 1. **Non-blocking API Response**

**Vấn đề nếu await embedding generation**:

- API response chậm (phải đợi Gemini API call)
- User experience kém
- Timeout risk nếu Gemini API chậm

**Giải pháp hiện tại**:

```csharp
// Fire-and-forget pattern
_ = GenerateAndSaveEmbeddingAsync(entity, cancellationToken);

// Transaction commit ngay lập tức
await transaction.CommitAsync(cancellationToken);
```

#### 2. **Resilience**

**Lợi ích**:

- Task/Comment vẫn được tạo dù embedding generation fail
- Embedding có thể regenerate sau
- Không block business flow

#### 3. **Scalability**

**Lợi ích**:

- Không block database transaction
- Có thể handle nhiều requests đồng thời
- Embedding generation có thể scale độc lập

### ⚠️ Trade-offs

**Nhược điểm**:

- Embedding có thể chưa sẵn sàng ngay sau khi tạo task
- Search có thể không tìm thấy task mới tạo ngay lập tức
- Cần handle errors silently

### 🔄 Khi nào nên xem xét thay đổi?

- Nếu cần embeddings ngay lập tức cho search
- Nếu muốn guarantee embedding generation
- Nếu có background job queue (Hangfire, etc.)

---

## 4. Tại sao sử dụng cosine distance cho vector similarity search?

### 🎯 Quyết định: Sử dụng cosine distance (`<=>`) operator

### ✅ Lý do chọn giải pháp hiện tại

#### 1. **Semantic Similarity**

**Cosine distance phù hợp cho**:

- Text embeddings (Gemini embeddings)
- Semantic similarity search
- Normalized vectors (length-independent)

**Công thức**:

```
cosine_distance = 1 - cosine_similarity
cosine_similarity = dot(A, B) / (||A|| * ||B||)
```

#### 2. **PostgreSQL Support**

**pgvector operators**:

- `<=>` - Cosine distance (chúng ta dùng)
- `<->` - L2/Euclidean distance
- `<#>` - Negative inner product

**Ví dụ query**:

```sql
SELECT * FROM tasks
ORDER BY embedding <=> query_embedding::vector
LIMIT 10;
```

#### 3. **Performance**

**Lợi ích**:

- Native support trong PostgreSQL
- Có thể tạo indexes (IVFFlat với cosine_ops)
- Fast similarity search với large datasets

### 🔄 Khi nào nên xem xét thay đổi?

- Nếu cần Euclidean distance cho use case cụ thể
- Nếu embeddings không normalized
- Nếu cần custom distance metric

---

## 5. Tại sao không await embedding generation trong transaction?

### 🎯 Quyết định: Generate embeddings sau khi commit transaction

### ✅ Lý do chọn giải pháp hiện tại

#### 1. **Transaction Isolation**

**Vấn đề nếu await trong transaction**:

- Transaction lock database lâu hơn
- Risk of deadlock
- Blocking other operations

**Giải pháp hiện tại**:

```csharp
// 1. Save task trong transaction
_tasks.CreateAsync(entity);
await _db.SaveChangesAsync(cancellationToken);
await transaction.CommitAsync(cancellationToken);

// 2. Generate embedding sau khi commit (outside transaction)
_ = GenerateAndSaveEmbeddingAsync(entity, cancellationToken);
```

#### 2. **Error Handling**

**Lợi ích**:

- Embedding generation fail không rollback task creation
- Task vẫn được tạo thành công
- Embedding có thể regenerate sau

#### 3. **Performance**

**Lợi ích**:

- Transaction commit nhanh
- Không block database
- Better concurrency

### ⚠️ Trade-offs

**Nhược điểm**:

- Embedding có thể fail mà không biết
- Cần monitoring để detect failed embeddings
- Eventual consistency (embeddings có thể chưa ready)

### 🔄 Khi nào nên xem xét thay đổi?

- Nếu cần guarantee embedding generation
- Nếu có background job queue
- Nếu muốn retry mechanism tốt hơn

---

## 📚 Related Documentation

- [TROUBLESHOOTING.md](./TROUBLESHOOTING.md) - Common issues and solutions
- [EMBEDDINGS_GUIDE.md](./EMBEDDINGS_GUIDE.md) - Embeddings guide (if exists)
- [VECTOR_SEARCH.md](./VECTOR_SEARCH.md) - Vector search guide (if exists)

---

**Last Updated:** November 12, 2025
