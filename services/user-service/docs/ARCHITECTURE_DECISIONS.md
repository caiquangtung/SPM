# Architecture Decisions - User Service

Tài liệu này giải thích các quyết định kiến trúc quan trọng trong User Service, bao gồm lý do tại sao chúng ta chọn các giải pháp cụ thể.

---

## 📋 Mục lục

- [1. Tại sao không sử dụng ASP.NET Core Identity Framework?](#1-tại-sao-không-sử-dụng-aspnet-core-identity-framework)
- [2. Tại sao không sử dụng MassTransit cho Kafka?](#2-tại-sao-không-sử-dụng-masstransit-cho-kafka)
- [3. So sánh các giải pháp](#3-so-sánh-các-giải-pháp)
- [4. Khi nào nên xem xét thay đổi?](#4-khi-nào-nên-xem-xét-thay-đổi)

---

## 1. Tại sao không sử dụng ASP.NET Core Identity Framework?

### 🎯 Quyết định: Tự implement authentication thay vì dùng Identity Framework

### ✅ Lý do chọn giải pháp hiện tại

#### 1. **Microservices Architecture**

**Vấn đề với Identity Framework**:

- Identity Framework được thiết kế cho monolithic applications
- Có nhiều dependencies và features không cần thiết cho microservices
- Khó tách biệt và scale độc lập

**Giải pháp hiện tại**:

- Lightweight, chỉ implement những gì cần thiết
- Dễ dàng scale và deploy độc lập
- Phù hợp với microservices architecture

#### 2. **JWT-based Authentication**

**Vấn đề với Identity Framework**:

- Identity Framework mặc định sử dụng cookie-based authentication
- JWT support có sẵn nhưng cần cấu hình thêm
- Phức tạp hơn khi cần stateless authentication

**Giải pháp hiện tại**:

- JWT-based authentication từ đầu
- Stateless, phù hợp với microservices
- Dễ dàng validate token ở các services khác
- Refresh token mechanism đơn giản và rõ ràng

#### 3. **Custom Requirements**

**Yêu cầu của SPM**:

- Email verification với custom flow
- Custom role system (Admin/PM/Member)
- Refresh token với httpOnly cookies
- Custom password policies
- Event publishing (Kafka) khi user created/updated

**Vấn đề với Identity Framework**:

- Có nhiều features không cần (2FA, external login, etc.)
- Khó customize flow theo yêu cầu cụ thể
- Event system không tích hợp sẵn với Kafka

**Giải pháp hiện tại**:

- Full control over authentication flow
- Dễ dàng customize theo yêu cầu
- Tích hợp Kafka events một cách tự nhiên
- Code rõ ràng, dễ maintain

#### 4. **Database Schema Control**

**Vấn đề với Identity Framework**:

- Identity Framework tạo nhiều tables mặc định (AspNetUsers, AspNetRoles, etc.)
- Schema phức tạp, khó customize
- Migration files lớn và phức tạp

**Giải pháp hiện tại**:

- Database schema đơn giản, chỉ có những gì cần
- Full control over database design
- Migration files nhỏ gọn, dễ hiểu
- Dễ dàng optimize queries

#### 5. **Learning Curve & Maintenance**

**Vấn đề với Identity Framework**:

- Có learning curve cho team
- Nhiều abstractions và conventions cần nhớ
- Khó debug khi có vấn đề
- Updates có thể breaking changes

**Giải pháp hiện tại**:

- Code đơn giản, dễ hiểu
- Dễ debug và troubleshoot
- Full control, không phụ thuộc vào framework updates
- Team có thể customize theo nhu cầu

### ❌ Nhược điểm của giải pháp hiện tại

1. **Phải tự implement các features**:

   - Password reset (chưa có)
   - Account lockout (chưa có)
   - 2FA (nếu cần trong tương lai)

2. **Phải tự maintain security**:

   - Phải đảm bảo password hashing đúng (đang dùng BCrypt)
   - Phải đảm bảo JWT implementation secure
   - Phải tự handle các edge cases

3. **Có thể thiếu một số best practices**:
   - Identity Framework có nhiều best practices built-in
   - Phải tự research và implement

### 📊 So sánh

| Tiêu chí              | Identity Framework   | Custom Implementation (Hiện tại) |
| --------------------- | -------------------- | -------------------------------- |
| **Complexity**        | Cao (nhiều features) | Thấp (chỉ những gì cần)          |
| **Flexibility**       | Thấp (khó customize) | Cao (full control)               |
| **Microservices**     | Không phù hợp        | Phù hợp                          |
| **JWT Support**       | Có nhưng cần config  | Native support                   |
| **Learning Curve**    | Cao                  | Thấp                             |
| **Maintenance**       | Microsoft maintain   | Team tự maintain                 |
| **Database Schema**   | Phức tạp             | Đơn giản                         |
| **Custom Features**   | Khó implement        | Dễ implement                     |
| **Event Integration** | Không có sẵn         | Dễ tích hợp                      |

### 🎯 Kết luận

**Giải pháp hiện tại phù hợp vì**:

- ✅ Microservices architecture
- ✅ JWT-based authentication
- ✅ Custom requirements
- ✅ Lightweight và đơn giản
- ✅ Full control

**Identity Framework phù hợp khi**:

- ❌ Monolithic application
- ❌ Cần nhiều features (2FA, external login, etc.)
- ❌ Team đã quen với Identity Framework
- ❌ Không cần custom nhiều

---

## 2. Tại sao không sử dụng MassTransit cho Kafka?

### 🎯 Quyết định: Sử dụng Kafka client trực tiếp thay vì MassTransit

### ✅ Lý do chọn giải pháp hiện tại

#### 1. **Simplicity**

**Vấn đề với MassTransit**:

- MassTransit là abstraction layer, thêm một layer phức tạp
- Cần học MassTransit concepts (consumers, sagas, etc.)
- Configuration phức tạp hơn
- Nhiều dependencies

**Giải pháp hiện tại**:

- Kafka client trực tiếp, đơn giản và rõ ràng
- Ít dependencies
- Dễ hiểu và maintain
- Code trực tiếp, không có abstraction layer

#### 2. **Single Message Broker**

**Vấn đề với MassTransit**:

- MassTransit hỗ trợ nhiều message brokers (RabbitMQ, Azure Service Bus, Redis, etc.)
- Abstraction layer cho phép switch giữa các brokers
- Nhưng chúng ta chỉ dùng Kafka, không cần switch

**Giải pháp hiện tại**:

- Chỉ dùng Kafka, không cần abstraction
- Không có overhead của abstraction layer
- Code tối ưu cho Kafka

#### 3. **Kafka Client Maturity**

**Confluent.Kafka**:

- Official Kafka client cho .NET
- Mature và stable
- Đầy đủ features
- Good performance
- Active maintenance

**MassTransit với Kafka**:

- MassTransit hỗ trợ Kafka nhưng không phải primary focus
- Có thể có limitations
- Updates có thể chậm hơn

#### 4. **Control & Flexibility**

**Giải pháp hiện tại**:

- Full control over Kafka configuration
- Dễ dàng customize producer/consumer
- Dễ dàng implement advanced features (exactly-once, transactions, etc.)
- Không bị giới hạn bởi MassTransit abstractions

**MassTransit**:

- Abstract away nhiều details
- Khó customize sâu
- Phải follow MassTransit patterns

#### 5. **Learning Curve**

**Giải pháp hiện tại**:

- Chỉ cần học Kafka concepts
- Code trực tiếp với Kafka client
- Dễ debug và troubleshoot

**MassTransit**:

- Cần học cả Kafka và MassTransit
- Abstraction layer có thể che giấu issues
- Khó debug khi có vấn đề

### ❌ Nhược điểm của giải pháp hiện tại

1. **Phải tự implement các patterns**:

   - Retry logic (đang dùng try-catch đơn giản)
   - Dead letter queue (chưa có)
   - Circuit breaker (chưa có)
   - Saga pattern (nếu cần)

2. **Phải tự maintain code**:

   - Phải tự handle error cases
   - Phải tự implement retry logic
   - Phải tự manage connections

3. **Có thể thiếu một số best practices**:
   - MassTransit có nhiều best practices built-in
   - Phải tự research và implement

### 📊 So sánh

| Tiêu chí              | MassTransit             | Kafka Client (Hiện tại) |
| --------------------- | ----------------------- | ----------------------- |
| **Complexity**        | Cao (abstraction layer) | Thấp (trực tiếp)        |
| **Flexibility**       | Thấp (bị giới hạn)      | Cao (full control)      |
| **Learning Curve**    | Cao (cần học cả 2)      | Thấp (chỉ Kafka)        |
| **Dependencies**      | Nhiều                   | Ít                      |
| **Performance**       | Có overhead             | Tối ưu                  |
| **Multi-broker**      | Hỗ trợ                  | Không (chỉ Kafka)       |
| **Advanced Features** | Built-in                | Phải tự implement       |
| **Debugging**         | Khó (abstraction)       | Dễ (trực tiếp)          |
| **Maintenance**       | Community maintain      | Team tự maintain        |

### 🎯 Kết luận

**Giải pháp hiện tại phù hợp vì**:

- ✅ Chỉ dùng Kafka, không cần multi-broker
- ✅ Đơn giản và dễ hiểu
- ✅ Full control
- ✅ Kafka client đủ mạnh

**MassTransit phù hợp khi**:

- ❌ Cần hỗ trợ nhiều message brokers
- ❌ Cần advanced features (sagas, state machines, etc.)
- ❌ Team đã quen với MassTransit
- ❌ Cần retry, circuit breaker built-in

---

## 3. So sánh các giải pháp

### Authentication: Identity Framework vs Custom Implementation

```csharp
// Identity Framework
services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Custom Implementation (Hiện tại)
services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options => { /* JWT config */ });
```

**Kết quả**:

- Identity Framework: ~50+ tables, nhiều dependencies
- Custom Implementation: 3 tables (users, email_verifications, refresh_tokens), ít dependencies

### Message Broker: MassTransit vs Kafka Client

```csharp
// MassTransit
services.AddMassTransit(x =>
{
    x.UsingKafka((context, cfg) =>
    {
        cfg.Host("localhost:9092");
        cfg.ConfigureEndpoints(context);
    });
});

// Kafka Client (Hiện tại)
var producer = new ProducerBuilder<Null, string>(config).Build();
await producer.ProduceAsync("user.created", message);
```

**Kết quả**:

- MassTransit: ~10+ dependencies, abstraction layer
- Kafka Client: 1 dependency (Confluent.Kafka), trực tiếp

---

## 4. Khi nào nên xem xét thay đổi?

### 🔄 Xem xét Identity Framework khi:

1. **Cần nhiều features**:

   - 2FA (Two-Factor Authentication)
   - External login (Google, Facebook, etc.)
   - Account lockout
   - Password complexity policies phức tạp

2. **Monolithic architecture**:

   - Chuyển từ microservices sang monolithic
   - Cần tích hợp với ASP.NET Core MVC

3. **Team expertise**:
   - Team đã quen với Identity Framework
   - Có nhiều experience với Identity Framework

### 🔄 Xem xét MassTransit khi:

1. **Cần multi-broker support**:

   - Cần hỗ trợ RabbitMQ, Azure Service Bus, etc.
   - Có thể switch broker trong tương lai

2. **Cần advanced features**:

   - Saga pattern (distributed transactions)
   - State machines
   - Request/response pattern
   - Routing slips

3. **Cần built-in patterns**:

   - Retry policies
   - Circuit breaker
   - Dead letter queue
   - Outbox pattern

4. **Complex event orchestration**:
   - Nhiều services cần coordinate
   - Complex workflows
   - Event sourcing

### 📝 Migration Path (nếu cần)

#### Migration to Identity Framework:

```csharp
// 1. Install package
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore

// 2. Update DbContext
public class UserDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>

// 3. Update models
public class ApplicationUser : IdentityUser { }

// 4. Update services
services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<UserDbContext>();
```

**Ước tính effort**: 2-3 weeks (migration + testing)

#### Migration to MassTransit:

```csharp
// 1. Install package
dotnet add package MassTransit
dotnet add package MassTransit.Kafka

// 2. Update Program.cs
services.AddMassTransit(x =>
{
    x.UsingKafka((context, cfg) =>
    {
        cfg.Host("localhost:9092");
        cfg.ConfigureEndpoints(context);
    });
});

// 3. Create consumers
public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
{
    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        // Handle event
    }
}
```

**Ước tính effort**: 1-2 weeks (migration + testing)

---

## 🎯 Recommendation

### Hiện tại: Giữ nguyên giải pháp

**Lý do**:

- ✅ Phù hợp với microservices architecture
- ✅ Đơn giản và dễ maintain
- ✅ Đáp ứng đủ requirements hiện tại
- ✅ Team có full control

### Tương lai: Xem xét thay đổi nếu:

1. **Identity Framework**:

   - Cần 2FA hoặc external login
   - Chuyển sang monolithic architecture
   - Team có expertise với Identity Framework

2. **MassTransit**:
   - Cần saga pattern hoặc state machines
   - Cần multi-broker support
   - Cần advanced event orchestration
   - Complexity tăng lên đáng kể

### Best Practice: YAGNI (You Aren't Gonna Need It)

- ✅ Implement những gì cần thiết hiện tại
- ✅ Tránh over-engineering
- ✅ Xem xét thay đổi khi thực sự cần
- ✅ Refactor khi requirements thay đổi

---

## 📚 Tham khảo

- [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [MassTransit Documentation](https://masstransit.io/)
- [Confluent Kafka .NET Client](https://github.com/confluentinc/confluent-kafka-dotnet)
- [JWT Authentication](https://jwt.io/)
- [Microservices Patterns](https://microservices.io/patterns/index.html)

---

**Last Updated**: 2025-11-10
