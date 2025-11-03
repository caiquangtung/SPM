# Load Balancing trong SPM Project

## 📋 Tổng Quan

SPM sử dụng **multi-layer load balancing** tùy theo môi trường:

### **Development (Hiện tại):**

- **YARP** - API Gateway với built-in load balancing

### **Production (Kế hoạch):**

- **Nginx hoặc Traefik** - External load balancer
- **YARP** - Internal routing và load balancing giữa service instances
- **Docker Swarm/Kubernetes** - Orchestration layer với service discovery

---

## 🔧 Load Balancing Hiện Tại

### **YARP (Yet Another Reverse Proxy)**

**YARP** là API Gateway được chọn cho SPM, có **built-in load balancing**:

```csharp
// YARP Configuration (khi implement)
{
  "ReverseProxy": {
    "Routes": {
      "user-service-route": {
        "ClusterId": "user-service-cluster",
        "Match": {
          "Path": "/api/auth/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "user-service-cluster": {
        "Destinations": {
          "user-service-1": {
            "Address": "http://user-service:8080"
          },
          "user-service-2": {
            "Address": "http://user-service:8080"
          }
        },
        "LoadBalancingPolicy": "RoundRobin" // hoặc LeastRequests, Random
      }
    }
  }
}
```

**YARP Load Balancing Policies:**

| Policy                | Mô tả                                                | Khi nào dùng             |
| --------------------- | ---------------------------------------------------- | ------------------------ |
| **RoundRobin**        | Phân phối requests theo vòng tròn                    | Workload đều nhau        |
| **LeastRequests**     | Chọn server có ít requests nhất                      | Workload không đều       |
| **Random**            | Chọn ngẫu nhiên                                      | Testing hoặc đơn giản    |
| **PowerOfTwoChoices** | Chọn giữa 2 servers ngẫu nhiên, chọn ít requests hơn | Production (recommended) |

---

## 🏗️ Architecture Layers

### **Layer 1: External Load Balancer (Production)**

```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │
       ▼
┌─────────────────┐
│  Nginx/Traefik  │  ← External Load Balancer
│  (Port 80/443)  │
└──────┬──────────┘
       │
       ▼
┌─────────────────┐
│  YARP Gateway   │  ← Multiple instances (scaled)
│   Instances     │
└─────────────────┘
```

**Nginx Example:**

```nginx
upstream yarp_gateway {
    least_conn;
    server yarp-gateway-1:8080;
    server yarp-gateway-2:8080;
    server yarp-gateway-3:8080;
}

server {
    listen 80;
    server_name api.spm.com;

    location / {
        proxy_pass http://yarp_gateway;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

**Traefik Example:**

```yaml
services:
  traefik:
    image: traefik:v2.10
    command:
      - "--api.insecure=true"
      - "--providers.docker=true"
      - "--entrypoints.web.address=:80"
    ports:
      - "80:80"
      - "8080:8080"
    labels:
      - "traefik.http.services.yarp.loadbalancer.server.port=8080"
```

### **Layer 2: YARP Internal Load Balancing**

```
┌─────────────────┐
│  YARP Gateway   │
└──────┬───────────┘
       │
       ├─────────────────┬─────────────────┐
       ▼                 ▼                 ▼
┌─────────────┐  ┌─────────────┐  ┌─────────────┐
│ User Service│  │User Service │  │User Service │
│ Instance 1  │  │ Instance 2  │  │ Instance 3  │
└─────────────┘  └─────────────┘  └─────────────┘
```

**YARP Configuration:**

```json
{
  "Clusters": {
    "user-service-cluster": {
      "Destinations": {
        "user-service-1": {
          "Address": "http://user-service-1:8080"
        },
        "user-service-2": {
          "Address": "http://user-service-2:8080"
        },
        "user-service-3": {
          "Address": "http://user-service-3:8080"
        }
      },
      "LoadBalancingPolicy": "PowerOfTwoChoices",
      "HealthCheck": {
        "Active": {
          "Enabled": true,
          "Interval": "00:00:10",
          "Timeout": "00:00:02",
          "Path": "/health"
        }
      }
    }
  }
}
```

### **Layer 3: Service Discovery (Docker Swarm/Kubernetes)**

**Docker Swarm:**

```yaml
# docker-compose.yml cho production
services:
  user-service:
    image: spm/user-service:latest
    deploy:
      replicas: 3 # Tự động load balance
      update_config:
        parallelism: 1
        delay: 10s
    networks:
      - spm-network
```

**Kubernetes:**

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: user-service
spec:
  replicas: 3
  selector:
    matchLabels:
      app: user-service
  template:
    metadata:
      labels:
        app: user-service
    spec:
      containers:
        - name: user-service
          image: spm/user-service:latest
---
apiVersion: v1
kind: Service
metadata:
  name: user-service
spec:
  selector:
    app: user-service
  ports:
    - port: 8080
  type: ClusterIP # Kubernetes tự động load balance
```

---

## 🎯 Load Balancing Strategies

### **1. Round Robin (Vòng tròn)**

**Cách hoạt động:**

- Request 1 → Server 1
- Request 2 → Server 2
- Request 3 → Server 3
- Request 4 → Server 1 (quay vòng)

**Ưu điểm:**

- Đơn giản, dễ implement
- Phân phối đều requests

**Nhược điểm:**

- Không tính đến server capacity
- Có thể overload server yếu hơn

**Khi dùng:**

- Servers có cùng capacity
- Workload đều nhau

### **2. Least Connections (Ít kết nối nhất)**

**Cách hoạt động:**

- Luôn chọn server có ít active connections nhất

**Ưu điểm:**

- Tự động điều chỉnh theo workload
- Phù hợp với long-running connections

**Nhược điểm:**

- Cần track connections count
- Phức tạp hơn Round Robin

**Khi dùng:**

- Servers có capacity khác nhau
- Long-running connections (SignalR, WebSocket)

### **3. Power of Two Choices (Recommended)**

**Cách hoạt động:**

- Chọn ngẫu nhiên 2 servers
- Chọn server có ít requests nhất trong 2

**Ưu điểm:**

- Cân bằng tốt
- Performance cao (không cần scan tất cả servers)

**Nhược điểm:**

- Phức tạp hơn Round Robin

**Khi dùng:**

- **Production (recommended)**
- Large number of servers

### **4. IP Hash (Session Affinity)**

**Cách hoạt động:**

- Hash client IP → Server cố định

**Ưu điểm:**

- Session affinity (sticky sessions)
- Client luôn đi tới cùng server

**Nhược điểm:**

- Có thể không cân bằng nếu IP không đều

**Khi dùng:**

- Cần session affinity
- Stateful applications

---

## 📊 Load Balancing trong SPM Services

### **Hiện Tại (Development)**

```
Client → YARP Gateway → User Service (single instance)
```

**Docker Compose hiện tại:**

```yaml
user-service:
  build: ./services/user-service
  container_name: spm-user-service # Single instance
  ports:
    - "5001:8080"
```

### **Production (Kế hoạch)**

```
Client → Nginx/Traefik → YARP Gateway (3 instances) → User Service (3 instances)
```

**Docker Swarm:**

```yaml
services:
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    deploy:
      replicas: 2

  api-gateway:
    image: spm/api-gateway:latest
    deploy:
      replicas: 3 # YARP instances

  user-service:
    image: spm/user-service:latest
    deploy:
      replicas: 3 # Service instances
```

**Kubernetes:**

```yaml
# HorizontalPodAutoscaler
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: user-service-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: user-service
  minReplicas: 3
  maxReplicas: 10
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          type: Utilization
          averageUtilization: 70
```

---

## 🔍 Health Checks & Failover

### **YARP Health Checks**

```json
{
  "Clusters": {
    "user-service-cluster": {
      "HealthCheck": {
        "Active": {
          "Enabled": true,
          "Interval": "00:00:10", // Check mỗi 10 giây
          "Timeout": "00:00:02", // Timeout 2 giây
          "Path": "/health", // Health check endpoint
          "Policy": "ConsecutiveFailures" // Remove sau N failures
        },
        "Passive": {
          "Enabled": true,
          "ReactivationPeriod": "00:01:00" // Thử lại sau 1 phút
        }
      }
    }
  }
}
```

### **Health Check Endpoint (Service)**

```csharp
// Program.cs
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

### **Failover Behavior**

```
Server 1: Healthy ✅
Server 2: Healthy ✅
Server 3: Unhealthy ❌ → Tự động remove khỏi pool

Request → Chỉ route tới Server 1 và Server 2

Server 3: Recovered ✅ → Tự động add lại sau health check pass
```

---

## 📈 Monitoring & Metrics

### **Load Balancing Metrics**

**Nginx:**

```nginx
http {
    upstream yarp_gateway {
        least_conn;
        server yarp-gateway-1:8080;
        server yarp-gateway-2:8080;
    }

    # Track metrics
    log_format upstream_log '$remote_addr - $remote_user [$time_local] '
                           '"$request" $status $body_bytes_sent '
                           '"$http_referer" "$http_user_agent" '
                           'upstream_addr: $upstream_addr '
                           'upstream_response_time: $upstream_response_time';
}
```

**YARP Metrics:**

```csharp
// YARP exposes metrics via Prometheus
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapMetrics(); // Prometheus metrics
});
```

### **Key Metrics to Monitor**

| Metric                    | Mô tả                       | Alert Threshold |
| ------------------------- | --------------------------- | --------------- |
| **Request Rate**          | Requests/second per server  | > 1000 req/s    |
| **Response Time**         | Average response time       | > 500ms         |
| **Error Rate**            | 5xx errors / total requests | > 1%            |
| **Active Connections**    | Current connections         | > 1000          |
| **Health Check Failures** | Failed health checks        | > 3 consecutive |

---

## 🎓 Best Practices

### **1. Health Checks**

- ✅ Enable active health checks
- ✅ Enable passive health checks (circuit breaker)
- ✅ Set appropriate intervals (10-30s)
- ✅ Use dedicated `/health` endpoint

### **2. Load Balancing Policy**

- ✅ Use **PowerOfTwoChoices** for production
- ✅ Use **LeastConnections** for long-running connections (SignalR)
- ✅ Use **RoundRobin** for simple cases

### **3. Scaling**

- ✅ Start with 2-3 instances per service
- ✅ Use auto-scaling based on CPU/Memory
- ✅ Scale horizontally, not vertically

### **4. Monitoring**

- ✅ Monitor all layers (External LB, YARP, Services)
- ✅ Set up alerts for failover events
- ✅ Track latency per service instance

### **5. Session Affinity**

- ✅ Use IP Hash only when necessary (stateful)
- ✅ Prefer stateless design (no session affinity needed)

---

## 🚀 Implementation Roadmap

### **Phase 1: Development (Hiện tại)**

- ✅ YARP với single service instances
- ✅ Manual routing configuration

### **Phase 2: Staging**

- ⏳ YARP với multiple instances (3 instances)
- ⏳ Basic health checks
- ⏳ RoundRobin load balancing

### **Phase 3: Production**

- ⏳ Nginx/Traefik external load balancer
- ⏳ YARP PowerOfTwoChoices policy
- ⏳ Health checks với circuit breaker
- ⏳ Auto-scaling (Docker Swarm/Kubernetes)
- ⏳ Monitoring và alerting

---

## 📚 References

- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)
- [YARP Load Balancing](https://microsoft.github.io/reverse-proxy/articles/basics/load-balancing/)
- [Nginx Load Balancing](https://nginx.org/en/docs/http/load_balancing.html)
- [Traefik Documentation](https://doc.traefik.io/traefik/)

---

**Tóm tắt:** SPM sử dụng **YARP** làm load balancer chính, với kế hoạch thêm **Nginx/Traefik** ở production layer. YARP cung cấp built-in load balancing với nhiều policies (RoundRobin, LeastRequests, PowerOfTwoChoices).
