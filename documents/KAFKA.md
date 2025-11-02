# Kafka Integration Documentation

## 📋 Mục lục

- [Tổng quan](#tổng-quan)
- [🔧 Thành phần chính của Kafka](#-thành-phần-chính-của-kafka)
- [⚙️ Kafka hoạt động như thế nào?](#️-kafka-hoạt-động-như-thế-nào)
- [📊 Flow chi tiết: Từ Producer đến Consumer](#-flow-chi-tiết-từ-producer-đến-consumer)
- [Kiến trúc Kafka trong SPM](#kiến-trúc-kafka-trong-spm)
- [Cấu hình](#cấu-hình)
- [Implementation](#implementation)
- [Events & Topics](#events--topics)
- [Sử dụng Kafka Producer](#sử-dụng-kafka-producer)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

---

## Tổng quan

SPM System sử dụng **Apache Kafka** như một message broker để implement **Event-Driven Architecture**. Kafka cho phép các microservices giao tiếp với nhau một cách asynchronous, loose coupling, và scalable.

### Tại sao sử dụng Kafka?

✅ **Event-Driven Architecture**: Services có thể react với events từ các services khác  
✅ **Loose Coupling**: Services không phụ thuộc trực tiếp vào nhau  
✅ **Scalability**: Dễ dàng scale horizontal  
✅ **Reliability**: Kafka đảm bảo message delivery  
✅ **Real-time Processing**: Xử lý events trong real-time

---

## 🔧 Thành phần chính của Kafka

### 1. Producer (Nhà sản xuất)

**Vai trò**: Gửi dữ liệu (messages/events) vào Kafka

**Đặc điểm**:

- Không cần biết Consumer nào sẽ nhận message
- Chỉ cần biết Topic name để gửi
- Tự động retry khi gặp lỗi
- Có thể gửi messages bất đồng bộ (async)

**Trong SPM**:

- `User Service` publish `user.created` event
- `Project Service` publish `task.created` event

### 2. Consumer (Người tiêu dùng)

**Vai trò**: Nhận và xử lý dữ liệu từ Kafka

**Đặc điểm**:

- Subscribe vào một hoặc nhiều Topics
- Đọc messages theo thứ tự (ordering)
- Có thể đọc từ đầu (from-beginning) hoặc tiếp tục từ vị trí đã đọc (offset)
- Có thể xử lý messages theo nhóm (consumer group)

**Trong SPM**:

- `Notification Service` consume `user.created` để gửi welcome email
- `AI Service` consume `task.created` để indexing

### 3. Broker (Máy chủ Kafka)

**Vai trò**: Lưu trữ và phân phối messages

**Đặc điểm**:

- Lưu messages trên ổ đĩa (persistent storage)
- Có thể chạy nhiều brokers (cluster) để scale và fault tolerance
- Quản lý partitions của topics
- Đảm bảo replication cho high availability

**Trong SPM**:

- Single broker chạy trên Docker (port 9092)
- Production nên có 3+ brokers

### 4. Topic (Kênh phân loại)

**Vai trò**: Phân loại và tổ chức messages

**Đặc điểm**:

- Giống như "channel" hoặc "category" của messages
- Tên topic thường theo format: `<entity>.<action>` (ví dụ: `user.created`)
- Messages được lưu trữ trong topic theo thứ tự
- Có thể giữ messages trong một khoảng thời gian (retention period)

**Ví dụ Topics trong SPM**:

- `user.created` - Chứa tất cả events khi user được tạo
- `task.assigned` - Chứa events khi task được assign

### 5. Partition (Phân vùng)

**Vai trò**: Chia nhỏ topic để tăng hiệu năng và khả năng song song

**Đặc điểm**:

- Mỗi topic có thể có nhiều partitions
- Messages trong partition được đánh số thứ tự (offset)
- Cho phép parallel processing (nhiều consumers cùng đọc)
- Đảm bảo ordering trong cùng một partition

**Ví dụ**:

```
Topic: user.created
├── Partition 0: [msg1, msg2, msg3, ...]
├── Partition 1: [msg1, msg2, msg3, ...]
└── Partition 2: [msg1, msg2, msg3, ...]
```

**Lợi ích**:

- **Scalability**: Có thể có nhiều consumers đọc song song từ các partitions khác nhau
- **Performance**: Tăng throughput (lượng messages xử lý được mỗi giây)
- **Load balancing**: Kafka tự động phân bổ messages vào các partitions

### 6. Zookeeper (Điều phối viên)

**Vai trò**: Quản lý metadata và điều phối các brokers

**Chức năng**:

- Lưu metadata về brokers, topics, partitions
- Điều phối leader election cho partitions
- Theo dõi health của brokers
- Quản lý consumer groups và offsets

**Lưu ý**:

- Kafka phiên bản mới (2.8+) có thể chạy không cần Zookeeper (KRaft mode)
- SPM hiện tại sử dụng Kafka với Zookeeper (đơn giản hơn cho development)

---

## ⚙️ Kafka hoạt động như thế nào?

### Luồng hoạt động cơ bản

```
┌──────────┐     1. Publish      ┌─────────┐     2. Store      ┌──────────┐
│ Producer │ ──────────────────> │ Broker  │ ───────────────>  │  Topic   │
│          │    (Message)        │         │    (Partition)    │          │
└──────────┘                     └────┬────┘                   └────┬─────┘
                                      │                             │
                                      │ 3. Subscribe                │ 4. Read
                                      │                             │
┌──────────┐     5. Process      ┌────▼────┐     6. Acknowledge     │
│ Consumer │ <────────────────── │ Broker  │ <──────────────────────┘
│          │    (Message)        │         │
└──────────┘                     └─────────┘
```

### Các bước chi tiết:

1. **Producer gửi message**:

   - Producer tạo message với dữ liệu cần gửi
   - Producer gửi message đến Kafka Broker, chỉ định Topic name
   - Broker nhận message và lưu vào partition phù hợp

2. **Broker lưu trữ message**:

   - Message được lưu vào một partition của topic
   - Mỗi message được gán một số offset (số thứ tự) trong partition
   - Messages được lưu trên ổ đĩa (disk), không chỉ trong memory

3. **Consumer subscribe topic**:

   - Consumer đăng ký (subscribe) vào một hoặc nhiều topics
   - Consumer có thể đọc từ đầu hoặc tiếp tục từ vị trí đã đọc (offset)

4. **Consumer đọc messages**:

   - Broker gửi messages cho consumer theo yêu cầu
   - Consumer xử lý messages (business logic)
   - Consumer commit offset sau khi xử lý xong (acknowledge)

5. **Kafka đảm bảo delivery**:
   - Messages được giữ lại trong Kafka (retention period)
   - Consumer có thể đọc lại messages nếu cần
   - Nếu consumer crash, có thể tiếp tục đọc từ offset đã lưu

### Đặc điểm quan trọng

#### 1. **Dữ liệu được lưu trữ lâu dài**

Kafka không xóa messages ngay sau khi consumer đọc. Messages được giữ lại theo retention period (mặc định 7 ngày, có thể cấu hình).

**Lợi ích**:

- Consumer có thể đọc lại messages
- Có thể replay events để xử lý lại
- Audit trail và debugging dễ dàng

#### 2. **Thứ tự dữ liệu được đảm bảo**

Messages trong cùng một partition được đảm bảo thứ tự (ordering).

**Ví dụ**:

```
Partition 0: [msg1 → msg2 → msg3]
Consumer sẽ luôn đọc theo thứ tự: msg1, msg2, msg3
```

**Lưu ý**: Thứ tự chỉ đảm bảo trong cùng partition. Giữa các partitions có thể không có thứ tự.

#### 3. **Tính chịu lỗi (Fault Tolerance)**

- Messages được replicate sang nhiều brokers
- Nếu một broker crash, các brokers khác tiếp tục hoạt động
- Consumer có thể reconnect và tiếp tục đọc

#### 4. **Khả năng mở rộng ngang (Horizontal Scalability)**

- Có thể thêm brokers để tăng capacity
- Có thể tăng số partitions để tăng throughput
- Nhiều consumers có thể đọc song song từ các partitions khác nhau

---

## 📊 Flow chi tiết: Từ Producer đến Consumer

### Ví dụ cụ thể: User Registration Flow

Giả sử một user đăng ký tài khoản mới trong SPM System:

#### Bước 1: User Service nhận request

```csharp
// User Service - AuthController
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterRequest request)
{
    // 1. Tạo user trong database
    var user = await _userRepository.CreateAsync(request);

    // 2. Commit transaction (QUAN TRỌNG!)
    await transaction.CommitAsync();

    // 3. Publish event đến Kafka
    await _kafkaProducer.PublishUserCreatedAsync(
        user.Id,
        user.Email,
        user.Role
    );

    return Ok("User registered successfully");
}
```

#### Bước 2: Producer tạo và gửi message

```27:49:services/user-service/Services/KafkaProducerService.cs
    public async Task PublishUserCreatedAsync(Guid userId, string email, string role)
    {
        try
        {
            var message = new
            {
                UserId = userId,
                Email = email,
                Role = role,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<Null, string> { Value = json };

            await _producer.ProduceAsync("user.created", kafkaMessage);
            _logger.LogInformation("Published user.created event for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish user.created event for user {UserId}", userId);
        }
    }
```

**Điều gì xảy ra**:

1. Producer tạo message object với dữ liệu user
2. Serialize thành JSON string
3. Gửi đến Kafka Broker với topic `user.created`
4. Broker chọn partition (dựa trên key hoặc round-robin)
5. Message được lưu vào partition với một offset number

#### Bước 3: Kafka Broker lưu message

```
Topic: user.created
Partition 0:
  Offset 0: {"userId": "123", "email": "user1@example.com", ...}
  Offset 1: {"userId": "124", "email": "user2@example.com", ...}
  Offset 2: {"userId": "125", "email": "user3@example.com", ...} ← New message
```

#### Bước 4: Consumers đọc và xử lý

**Consumer 1: Notification Service**

```csharp
// Notification Service - KafkaConsumerService (Background Service)
public class UserCreatedConsumer : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe("user.created");

        while (!stoppingToken.IsCancellationRequested)
        {
            var result = consumer.Consume(stoppingToken);

            // Parse message
            var eventData = JsonSerializer.Deserialize<UserCreatedEvent>(result.Message.Value);

            // Business logic: Gửi welcome email
            await _emailService.SendWelcomeEmailAsync(
                eventData.Email,
                eventData.UserId
            );

            // Commit offset (đánh dấu đã xử lý xong)
            consumer.Commit(result);
        }
    }
}
```

**Consumer 2: Project Service**

```csharp
// Project Service - UserCreatedConsumer
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    consumer.Subscribe("user.created");

    while (!stoppingToken.IsCancellationRequested)
    {
        var result = consumer.Consume(stoppingToken);
        var eventData = JsonSerializer.Deserialize<UserCreatedEvent>(result.Message.Value);

        // Business logic: Tạo default workspace cho user mới
        await _workspaceService.CreateDefaultWorkspaceAsync(eventData.UserId);

        consumer.Commit(result);
    }
}
```

### Flow Diagram hoàn chỉnh

```
┌─────────────────┐
│  Client (Web)   │
└────────┬────────┘
         │ HTTP POST /api/auth/register
         ▼
┌─────────────────┐
│  User Service   │
│  AuthController │
└────────┬────────┘
         │
         │ 1. Create user in DB
         │ 2. Commit transaction
         │
         ▼
┌─────────────────────┐      ┌──────────────────┐
│ KafkaProducerService │─────>│  Kafka Broker    │
└─────────────────────┘      │  Topic:           │
         │                  │  user.created     │
         │ 3. Publish event │  Partition 0      │
         │                  │  [offset 2]       │
         │                  └────────┬──────────┘
         │                           │
         │                           │ 4. Store message
         │                           │    (Persistent)
         │                           │
         │                           │ 5. Notify consumers
         │                           │
         ├───────────────────────────┴──────────────┐
         │                                          │
         ▼                                          ▼
┌─────────────────────┐                  ┌─────────────────────┐
│ Notification       │                  │ Project Service     │
│ Service            │                  │                     │
│                     │                  │                     │
│ 6. Consume event    │                  │ 6. Consume event    │
│ 7. Send welcome     │                  │ 7. Create default   │
│    email            │                  │    workspace        │
└─────────────────────┘                  └─────────────────────┘
```

### Consumer Groups

Khi nhiều instances của cùng một service chạy (ví dụ: 3 instances của Notification Service), chúng nên thuộc cùng một **Consumer Group**.

**Lợi ích**:

- **Load balancing**: Mỗi instance chỉ xử lý một phần messages
- **Fault tolerance**: Nếu một instance crash, các instances khác tiếp tục
- **Scalability**: Dễ dàng thêm/bớt instances

**Ví dụ**: Topic `user.created` có 3 partitions

```
Consumer Group: notification-service
├── Instance 1 → Partition 0
├── Instance 2 → Partition 1
└── Instance 3 → Partition 2
```

Mỗi instance chỉ đọc từ một partition, không có duplicate processing.

### Offset Management

**Offset** là số thứ tự của message trong partition. Consumer sử dụng offset để:

- Track vị trí đã đọc đến đâu
- Resume từ vị trí đã dừng (sau khi restart)
- Đảm bảo không bỏ sót messages

**Ví dụ**:

```
Partition 0:
Offset 0: message A
Offset 1: message B
Offset 2: message C
Offset 3: message D

Consumer đã đọc đến offset 2 → Commit offset = 2
Nếu consumer restart → Tiếp tục đọc từ offset 3 (message D)
```

---

## Kiến trúc Kafka trong SPM

### Infrastructure Setup

Kafka được triển khai qua Docker Compose với các components:

```
┌─────────────┐
│  Zookeeper  │  (Coordinator & Metadata Storage)
└──────┬──────┘
       │
┌──────▼──────┐
│    Kafka    │  (Message Broker)
│   Broker    │  Port: 9092
└──────┬──────┘
       │
   ┌───┴───┐
   │       │
┌──▼──┐ ┌─▼───┐
│Producer│ │Consumer│
└──────┘ └─────┘
```

### Docker Compose Configuration

```yaml
zookeeper:
  image: confluentinc/cp-zookeeper:7.5.0
  container_name: spm-zookeeper
  environment:
    ZOOKEEPER_CLIENT_PORT: 2181
    ZOOKEEPER_TICK_TIME: 2000
  ports:
    - "2181:2181"

kafka:
  image: confluentinc/cp-kafka:7.5.0
  container_name: spm-kafka
  depends_on:
    - zookeeper
  environment:
    KAFKA_BROKER_ID: 1
    KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
    KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka:9092,PLAINTEXT_HOST://localhost:9092
    KAFKA_LISTENER_SECURITY_PROTOCOL_MAP: PLAINTEXT:PLAINTEXT,PLAINTEXT_HOST:PLAINTEXT
    KAFKA_INTER_BROKER_LISTENER_NAME: PLAINTEXT
    KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1
  ports:
    - "9092:9092"
  healthcheck:
    test:
      [
        "CMD-SHELL",
        "kafka-broker-api-versions --bootstrap-server localhost:9092 || exit 1",
      ]
    interval: 30s
    timeout: 10s
    retries: 5
```

### Ports

- **Kafka Broker**: `9092` (internal & external)
- **Zookeeper**: `2181`

---

## Cấu hình

### 1. Services nào cần Kafka?

**Không phải tất cả microservices đều cần Kafka!** Chỉ những service có events để publish hoặc cần consume events mới cần Kafka.

#### Services cần **Producer** (Publish Events):

| Service             | Events Published                                                                                                | Lý do                               |
| ------------------- | --------------------------------------------------------------------------------------------------------------- | ----------------------------------- |
| **User Service**    | `user.created`, `user.updated`                                                                                  | Thông báo khi user đăng ký/cập nhật |
| **Project Service** | `project.created`, `project.updated`, `task.created`, `task.assigned`, `task.status.changed`, `comment.created` | Thông báo về project/task changes   |
| **File Service**    | `file.uploaded`                                                                                                 | Thông báo khi file được upload      |

#### Services cần **Consumer** (Consume Events):

| Service                  | Events Consumed                                                           | Lý do                                      |
| ------------------------ | ------------------------------------------------------------------------- | ------------------------------------------ |
| **Notification Service** | `user.created`, `task.assigned`, `task.status.changed`, `comment.created` | Gửi notifications/emails khi có events     |
| **AI Service**           | `task.created`, `comment.created`, `file.uploaded`                        | Index content cho RAG, generate embeddings |
| **Project Service**      | `user.created`                                                            | Tạo default workspace khi user mới đăng ký |

#### Services **KHÔNG CẦN** Kafka:

| Service         | Lý do                                       |
| --------------- | ------------------------------------------- |
| **API Gateway** | Chỉ route requests, không có business logic |
| **Frontend**    | Client-side, giao tiếp qua API              |

### 2. Service Configuration

Chỉ các services cần Kafka mới cần cấu hình trong `appsettings.json`:

```json
{
  "Kafka": {
    "BootstrapServers": "kafka:9092"
  }
}
```

**Environment Variables** (production):

```bash
KAFKA_BOOTSTRAP_SERVERS=kafka:9092
```

### 3. Dependency Injection

#### Cho Producer Services:

Kafka Producer Service được đăng ký trong `Program.cs`:

```csharp
// Only if service needs to publish events
builder.Services.AddScoped<IKafkaProducerService, KafkaProducerService>();
```

**Lifetime**: `Scoped` - mỗi HTTP request có một producer instance

#### Cho Consumer Services:

Consumer sẽ được implement như background service (sẽ thêm sau):

```csharp
// Future: For services that need to consume events
builder.Services.AddHostedService<KafkaConsumerService>();
```

---

## Implementation

### Quyết định Implementation

**Câu hỏi**: Mỗi microservice đều phải implement KafkaProducerService?

**Trả lời**: ❌ **KHÔNG!** Chỉ implement khi service **thực sự cần** publish events.

**Rule of thumb**:

- ✅ **CÓ Producer** nếu service tạo/update entities mà services khác cần biết
- ❌ **KHÔNG CÓ Producer** nếu service chỉ xử lý internal logic
- ✅ **CÓ Consumer** nếu service cần react với events từ services khác

### 1. Kafka Producer Service

**Chỉ implement trong services cần publish events** (User Service, Project Service, File Service).

**Interface**: `IKafkaProducerService.cs`

```csharp
public interface IKafkaProducerService
{
    Task PublishUserCreatedAsync(Guid userId, string email, string role);
    Task PublishUserUpdatedAsync(Guid userId, string email, string role);
}
```

**Implementation**: `KafkaProducerService.cs`

```csharp
public class KafkaProducerService : IKafkaProducerService, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<KafkaProducerService> _logger;
    private readonly IConfiguration _configuration;

    public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "kafka:9092";
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task PublishUserCreatedAsync(Guid userId, string email, string role)
    {
        var message = new
        {
            UserId = userId,
            Email = email,
            Role = role,
            Timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(message);
        var kafkaMessage = new Message<Null, string> { Value = json };

        await _producer.ProduceAsync("user.created", kafkaMessage);
    }
}
```

### 2. NuGet Package

**Chỉ thêm vào services cần Kafka**:

```xml
<!-- For Producer Services -->
<PackageReference Include="Confluent.Kafka" Version="2.3.0" />

<!-- For Consumer Services (future) -->
<PackageReference Include="Confluent.Kafka" Version="2.3.0" />
```

### 3. Shared Kafka Library (Recommended)

Để tránh duplicate code, nên tạo shared library:

```
shared/
└── kafka-common/
    ├── KafkaProducerService.cs
    ├── IKafkaProducerService.cs
    ├── KafkaConsumerService.cs (future)
    └── EventSchemas/
        ├── UserCreatedEvent.cs
        ├── TaskAssignedEvent.cs
        └── ...
```

**Benefits**:

- ✅ Reuse code giữa các services
- ✅ Consistent event schemas
- ✅ Easier maintenance
- ✅ Type-safe events

**Current Implementation**: Mỗi service implement riêng (sẽ refactor sau)

### 4. Resource Management

Service implement `IDisposable` để đảm bảo Kafka producer được dispose đúng cách:

```csharp
public void Dispose()
{
    Dispose(true);
    GC.SuppressFinalize(this);
}

protected virtual void Dispose(bool disposing)
{
    if (!_disposed && disposing)
    {
        _producer?.Dispose();
        _disposed = true;
    }
}
```

---

## Events & Topics

### Available Topics

Danh sách topics được định nghĩa trong `infrastructure/kafka/topics-init.sh`:

| Topic Name            | Description               | Partition | Replication |
| --------------------- | ------------------------- | --------- | ----------- |
| `user.created`        | User đăng ký mới          | 3         | 1           |
| `user.updated`        | User được cập nhật        | 3         | 1           |
| `project.created`     | Project mới được tạo      | 3         | 1           |
| `project.updated`     | Project được cập nhật     | 3         | 1           |
| `task.created`        | Task mới được tạo         | 3         | 1           |
| `task.updated`        | Task được cập nhật        | 3         | 1           |
| `task.status.changed` | Trạng thái task thay đổi  | 3         | 1           |
| `task.assigned`       | Task được assign          | 3         | 1           |
| `comment.created`     | Comment mới được tạo      | 3         | 1           |
| `file.uploaded`       | File được upload          | 3         | 1           |
| `notification.send`   | Notification cần được gửi | 3         | 1           |

### Event Schema

#### `user.created`

```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "role": "Member",
  "timestamp": "2025-01-20T10:30:00Z"
}
```

**Published when**:

- User đăng ký thành công
- Được publish sau khi transaction commit để tránh rollback

**Consumers**:

- `notification-service`: Gửi welcome email
- `project-service`: Tạo default workspace

#### `user.updated`

```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "role": "PM",
  "timestamp": "2025-01-20T10:35:00Z"
}
```

**Published when**:

- Email được verify
- Role được thay đổi
- User profile được cập nhật

**Consumers**:

- `notification-service`: Gửi notification về thay đổi
- `project-service`: Update permissions

### Topic Creation Script

```bash
#!/bin/bash
# infrastructure/kafka/topics-init.sh

TOPICS=(
  "user.created"
  "user.updated"
  # ... more topics
)

KAFKA_BOOTSTRAP_SERVER=kafka:9092

for TOPIC in "${TOPICS[@]}"; do
  kafka-topics --create \
    --bootstrap-server $KAFKA_BOOTSTRAP_SERVER \
    --topic $TOPIC \
    --partitions 3 \
    --replication-factor 1 \
    --if-not-exists
done
```

**Run script**:

```bash
docker exec -it spm-kafka bash
./infrastructure/kafka/topics-init.sh
```

---

## Sử dụng Kafka Producer

### Trong Controller

```csharp
public class AuthController : ControllerBase
{
    private readonly IKafkaProducerService _kafkaProducer;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // ... create user logic ...

        // Commit transaction first
        await transaction.CommitAsync();

        // Publish event AFTER transaction (to avoid rollback)
        await _kafkaProducer.PublishUserCreatedAsync(
            user.Id,
            user.Email,
            user.Role
        );

        return OkResponse(new { userId = user.Id }, "User registered successfully");
    }
}
```

### Best Practice: Transaction Management

✅ **DO**: Publish events sau khi transaction commit  
❌ **DON'T**: Publish events trước khi transaction commit

**Lý do**: Nếu transaction rollback, event đã được publish sẽ không thể undo, dẫn đến data inconsistency.

```csharp
// ✅ CORRECT
using var transaction = await _dbContext.Database.BeginTransactionAsync();
try
{
    // Database operations
    await _userRepository.CreateAsync(user);
    await transaction.CommitAsync(); // Commit first

    // Publish after commit
    await _kafkaProducer.PublishUserCreatedAsync(...);
}
catch (Exception ex)
{
    await transaction.RollbackAsync();
    throw;
}

// ❌ WRONG
await _kafkaProducer.PublishUserCreatedAsync(...); // Too early!
using var transaction = await _dbContext.Database.BeginTransactionAsync();
// ...
```

### Error Handling

Producer service có error handling để không làm gián đoạn main flow:

```csharp
public async Task PublishUserCreatedAsync(Guid userId, string email, string role)
{
    try
    {
        // ... publish logic ...
        _logger.LogInformation("Published user.created event for user {UserId}", userId);
    }
    catch (Exception ex)
    {
        // Log error but don't throw
        _logger.LogError(ex, "Failed to publish user.created event for user {UserId}", userId);
    }
}
```

**Lý do**: Kafka publish failure không nên làm fail HTTP request. Event có thể được retry sau.

---

## Best Practices

### 1. Event Naming Convention

✅ **DO**: Sử dụng format `<entity>.<action>`

- `user.created`
- `user.updated`
- `task.assigned`

❌ **DON'T**:

- `userCreate` (không có dot separator)
- `createUser` (verb trước)

### 2. Event Schema Versioning

Để tương lai có thể evolve schema, nên thêm version:

```json
{
  "version": "1.0",
  "userId": "...",
  "email": "...",
  "timestamp": "..."
}
```

### 3. Idempotency

Events nên có unique ID để tránh duplicate processing:

```csharp
var message = new
{
    EventId = Guid.NewGuid(), // Unique event ID
    UserId = userId,
    // ...
};
```

### 4. Timestamp

Luôn include UTC timestamp:

```csharp
Timestamp = DateTime.UtcNow
```

### 5. Producer Configuration

**Production** settings:

```csharp
var config = new ProducerConfig
{
    BootstrapServers = bootstrapServers,
    Acks = Acks.All, // Wait for all replicas
    Retries = 3,
    MaxInFlight = 5,
    CompressionType = CompressionType.Snappy,
    EnableIdempotence = true
};
```

---

## Troubleshooting

### 1. Kafka không kết nối được

**Lỗi**: `Connection refused` hoặc `No broker available`

**Giải pháp**:

```bash
# Kiểm tra Kafka đang chạy
docker ps | grep kafka

# Check logs
docker logs spm-kafka

# Test connection
docker exec -it spm-kafka kafka-broker-api-versions --bootstrap-server localhost:9092
```

### 2. Topic không tồn tại

**Lỗi**: `Topic user.created does not exist`

**Giải pháp**:

```bash
# Create topic manually
docker exec -it spm-kafka kafka-topics --create \
  --bootstrap-server localhost:9092 \
  --topic user.created \
  --partitions 3 \
  --replication-factor 1

# List all topics
docker exec -it spm-kafka kafka-topics --list --bootstrap-server localhost:9092
```

### 3. Producer không publish được

**Kiểm tra**:

1. Logs có error không?
2. Kafka broker accessible?
3. Topic tồn tại chưa?
4. Configuration đúng chưa?

**Debug**:

```csharp
_logger.LogInformation("Attempting to publish to topic {Topic} with message {Message}",
    topic, json);
```

### 4. Events không được consume

**Nguyên nhân có thể**:

- Consumer service chưa start
- Consumer chưa subscribe đúng topic
- Consumer có error và đang retry

**Kiểm tra offsets**:

```bash
docker exec -it spm-kafka kafka-consumer-groups --bootstrap-server localhost:9092 \
  --group my-consumer-group --describe
```

---

## Monitoring

### Kafka Topics Management

**List topics**:

```bash
docker exec -it spm-kafka kafka-topics --list --bootstrap-server localhost:9092
```

**Describe topic**:

```bash
docker exec -it spm-kafka kafka-topics --describe \
  --bootstrap-server localhost:9092 \
  --topic user.created
```

**Consume messages** (for debugging):

```bash
docker exec -it spm-kafka kafka-console-consumer \
  --bootstrap-server localhost:9092 \
  --topic user.created \
  --from-beginning
```

### Health Check

Kafka health check trong docker-compose:

```yaml
healthcheck:
  test:
    [
      "CMD-SHELL",
      "kafka-broker-api-versions --bootstrap-server localhost:9092 || exit 1",
    ]
  interval: 30s
  timeout: 10s
  retries: 5
```

---

## Implementation Decision Tree

### Service cần Kafka không?

```
Does your service create/update entities that other services need to know about?
│
├─ YES → Implement Producer (Publish Events)
│   │
│   ├─ User Service: user.created, user.updated
│   ├─ Project Service: project.created, task.created, etc.
│   └─ File Service: file.uploaded
│
└─ NO → Does your service need to react to events from other services?
    │
    ├─ YES → Implement Consumer (Subscribe to Events)
    │   │
    │   ├─ Notification Service: consume để gửi notifications
    │   ├─ AI Service: consume để indexing
    │   └─ Project Service: consume user.created để tạo workspace
    │
    └─ NO → KHÔNG CẦN Kafka
        │
        ├─ API Gateway (chỉ routing)
        └─ Frontend (client-side)
```

### Checklist khi quyết định:

**Cần Producer khi:**

- [ ] Service tạo/update entities quan trọng
- [ ] Services khác cần biết về changes
- [ ] Cần loose coupling với services khác

**Cần Consumer khi:**

- [ ] Service cần react với events từ services khác
- [ ] Cần xử lý events async
- [ ] Cần reliability (không miss events)

**Không cần Kafka khi:**

- [ ] Service chỉ xử lý internal logic
- [ ] Không có cross-service communication
- [ ] Chỉ cần sync HTTP calls

### Ví dụ:

**User Service** ✅ Cần Producer

- Tạo user mới → publish `user.created`
- Update user → publish `user.updated`

**Notification Service** ✅ Cần Consumer

- Subscribe `user.created` → gửi welcome email
- Subscribe `task.assigned` → gửi notification

**API Gateway** ❌ KHÔNG cần Kafka

- Chỉ route requests
- Không có business logic

---

## Future Enhancements

### 1. Kafka Consumer Implementation

Hiện tại chỉ có Producer. Cần thêm Consumer để:

- **Notification service**: consume `user.created`, `task.assigned` events để gửi notifications
- **AI service**: consume `task.created`, `comment.created` events để indexing
- **Project service**: consume `user.created` events để tạo default workspace

### 2. Schema Registry

Sử dụng Confluent Schema Registry để validate event schemas:

```yaml
schema-registry:
  image: confluentinc/cp-schema-registry:7.5.0
  depends_on:
    - kafka
  environment:
    SCHEMA_REGISTRY_KAFKASTORE_BOOTSTRAP_SERVERS: kafka:9092
    SCHEMA_REGISTRY_HOST_NAME: schema-registry
    SCHEMA_REGISTRY_LISTENERS: http://0.0.0.0:8081
  ports:
    - "8081:8081"
```

### 3. Kafka Streams

Sử dụng Kafka Streams cho real-time data processing:

- Aggregate statistics
- Real-time analytics
- Event transformation

### 4. Dead Letter Queue (DLQ)

Thêm DLQ topic cho failed messages:

```
user.created.dlq
```

### 5. Retry Mechanism

Implement retry với exponential backoff:

```csharp
var config = new ProducerConfig
{
    Retries = 3,
    RetryBackoffMs = 100
};
```

---

## Tài liệu tham khảo

- [Apache Kafka Documentation](https://kafka.apache.org/documentation/)
- [Confluent .NET Client](https://github.com/confluentinc/confluent-kafka-dotnet)
- [Event-Driven Architecture Patterns](https://www.oreilly.com/library/view/designing-event-driven-systems/9781492038252/)

---

**Last Updated**: 2025-01-20 (Updated với giải thích chi tiết về các thành phần Kafka, flow hoạt động, và ví dụ triển khai)  
**Maintainer**: SPM Development Team
