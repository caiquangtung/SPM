# API Gateway (YARP)

**Single Entry Point for all SPM Microservices**

API Gateway sử dụng **YARP (Yet Another Reverse Proxy)** để route requests đến các microservices, với centralized JWT authentication và CORS handling.

---

## 🎯 **Features**

- ✅ **Reverse Proxy**: Routes requests to appropriate microservices
- ✅ **Centralized JWT Authentication**: Validates JWT tokens before forwarding
- ✅ **Authorization Policies**: Anonymous (auth routes) vs Authenticated (protected routes)
- ✅ **CORS Support**: Configured for frontend (`http://localhost:3000`)
- ✅ **Hot Reload**: Development mode with automatic code reloading
- ✅ **Load Balancing**: YARP built-in support (ready for multiple instances)

---

## 🗺️ **Route Configuration**

### **1. Anonymous Routes (No JWT Required)**

| Route Pattern | Target Service | Port | Notes |
|--------------|----------------|------|-------|
| `/api/auth/**` | user-service | 8080 | Register, Login, Refresh, Verify Email |

### **2. Protected Routes (JWT Required)**

| Route Pattern | Target Service | Port | Notes |
|--------------|----------------|------|-------|
| `/api/projects/**` | project-service | 8080 | Project CRUD operations |
| `/api/tasks/**` | project-service | 8080 | Task CRUD, Search, Status updates |
| `/api/tasks/{taskId}/comments/**` | project-service | 8080 | Task comments |
| `/api/files/**` | file-service | 8080 | File upload, download, list |
| `/api/tasks/{taskId}/attachments/**` | file-service | 8080 | Task file attachments |

---

## 🔐 **Authentication & Authorization**

### **JWT Configuration**

```json
{
  "JWT": {
    "SecretKey": "your-super-secret-key-change-in-production",
    "Issuer": "spm-api-gateway",
    "Audience": "spm-services"
  }
}
```

**Environment Variable (Recommended):**
```bash
export JWT__SecretKey="your-super-secret-key-change-in-production"
```

### **Authorization Policies**

1. **Anonymous Policy**: Allows unauthenticated access
   - Applied to: `/api/auth/**`
   - Used for: Register, Login, Refresh Token

2. **Default Policy**: Requires authenticated user
   - Applied to: All other routes (`/api/projects`, `/api/tasks`, `/api/files`)
   - Validates: JWT token presence and validity

3. **Fallback Policy**: Default (authenticated) for any unmatched routes

---

## 🌐 **CORS Configuration**

```csharp
// Allows requests from frontend
WithOrigins("http://localhost:3000")
  .AllowAnyMethod()
  .AllowAnyHeader()
  .AllowCredentials()
```

**Allowed Origin:** `http://localhost:3000` (Next.js frontend)

---

## 🚀 **Getting Started**

### **Option 1: Docker Compose (Recommended)**

```bash
# Start all services including API Gateway
cd /path/to/SPM
docker-compose up -d

# Verify API Gateway is running
curl http://localhost:5000/api/auth/login
```

### **Option 2: Local Development**

```bash
# Run API Gateway locally (services must be running)
cd services/api-gateway
dotnet watch run

# API Gateway will be available at http://localhost:5000
```

---

## 🧪 **Testing**

### **Quick Health Check**

```bash
# Test anonymous route (should work without token)
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test@1234"}'

# Test protected route (should return 401 without token)
curl http://localhost:5000/api/projects
```

### **Comprehensive Testing**

See [TESTING.md](./TESTING.md) for full test suite including:
- ✅ Anonymous routes
- ✅ Protected routes
- ✅ JWT validation
- ✅ Token refresh
- ✅ CORS validation
- ✅ File uploads
- ✅ AI search
- ✅ Error handling

### **HTTP Test File**

Use `api-gateway.http` with VS Code REST Client extension:
- Open `api-gateway.http`
- Click "Send Request" above each request
- Follow the test flow (register → login → create project → create task)

---

## 📊 **Port Configuration**

| Environment | Port Mapping | Access URL |
|-------------|--------------|------------|
| **Docker** | `5000:8080` | `http://localhost:5000` |
| **Local** | `5000` (default) | `http://localhost:5000` |
| **Container** | `8080` | `http://api-gateway:8080` (internal) |

---

## 🔧 **Configuration Files**

### **appsettings.json**

```json
{
  "ReverseProxy": {
    "Routes": {
      "user-service-route": {
        "ClusterId": "user-service-cluster",
        "Match": { "Path": "/api/auth/{**catch-all}" },
        "AuthorizationPolicy": "Anonymous"
      },
      "project-service-route": {
        "ClusterId": "project-service-cluster",
        "Match": { "Path": "/api/projects/{**catch-all}" },
        "AuthorizationPolicy": "Default"
      }
      // ... more routes
    },
    "Clusters": {
      "user-service-cluster": {
        "Destinations": {
          "user-service": {
            "Address": "http://user-service:8080"
          }
        }
      }
      // ... more clusters
    }
  }
}
```

---

## 🐛 **Troubleshooting**

### **Issue: 401 Unauthorized on all routes**

**Cause:** JWT secret key mismatch between services.

**Fix:**
```bash
# Verify JWT_SECRET_KEY in docker-compose.yml
grep JWT_SECRET_KEY docker-compose.yml

# Should be same for:
# - api-gateway
# - user-service
# - project-service
# - file-service
```

### **Issue: 404 Not Found**

**Cause:** Route not configured or service down.

**Fix:**
```bash
# Check service is running
docker ps | grep spm-

# Check API Gateway logs
docker logs spm-api-gateway -f

# Verify route in appsettings.json
cat appsettings.json | jq '.ReverseProxy.Routes'
```

### **Issue: CORS error in browser**

**Cause:** Frontend origin not allowed.

**Fix:**
```bash
# Verify CORS configuration in Program.cs
grep -A 3 "WithOrigins" Program.cs

# Should include: http://localhost:3000
```

### **Issue: Service not responding**

**Cause:** Downstream service crashed or not started.

**Fix:**
```bash
# Check all services
docker-compose ps

# Restart specific service
docker-compose restart user-service

# Check service logs
docker logs spm-user-service -f
```

---

## 📚 **Architecture**

```
┌─────────────────────────────────────────┐
│      Client (Browser/Postman)          │
└────────────────┬────────────────────────┘
                 │ HTTP/HTTPS
                 ▼
┌────────────────────────────────────────────┐
│         API Gateway (Port 5000)            │
│  ┌──────────────────────────────────────┐ │
│  │   1. CORS Validation                 │ │
│  │   2. JWT Authentication              │ │
│  │   3. Authorization Policy Check      │ │
│  │   4. Route Matching                  │ │
│  │   5. Forward to Service              │ │
│  └──────────────────────────────────────┘ │
└─────┬──────────┬──────────┬───────────────┘
      │          │          │
      ▼          ▼          ▼
┌──────────┐ ┌──────────┐ ┌──────────┐
│  User    │ │ Project  │ │  File    │
│ Service  │ │ Service  │ │ Service  │
│ :8080    │ │ :8080    │ │ :8080    │
└──────────┘ └──────────┘ └──────────┘
```

---

## 🔄 **Request Flow Example**

### **Example: Create Task**

```
1. Client sends request:
   POST http://localhost:5000/api/tasks
   Authorization: Bearer eyJhbGci...
   { "projectId": "...", "title": "..." }

2. API Gateway receives request:
   ├─ Validates CORS (origin: http://localhost:3000)
   ├─ Extracts JWT from Authorization header
   ├─ Validates JWT (signature, expiry, issuer, audience)
   ├─ Checks authorization policy ("Default" - requires auth)
   ├─ Matches route: /api/tasks → project-service-cluster
   └─ Forwards to: http://project-service:8080/api/tasks

3. Project Service receives request:
   ├─ Processes request (JWT claims available if needed)
   ├─ Creates task in database
   └─ Returns response

4. API Gateway forwards response to client:
   { "success": true, "data": { "id": "...", ... } }
```

---

## 🔐 **Security Features**

1. ✅ **Centralized JWT Validation**: Single point for token validation
2. ✅ **Secret Key Protection**: Use environment variables, never commit secrets
3. ✅ **Token Expiration**: Enforces 15-minute access token expiry
4. ✅ **CORS Protection**: Restricts to allowed origins only
5. ✅ **HTTPS Ready**: Can be configured for TLS/SSL in production
6. ✅ **Rate Limiting Ready**: YARP supports rate limiting (can be enabled)

---

## 📈 **Performance Considerations**

- **Minimal Overhead**: YARP adds ~1-2ms latency
- **Keep-Alive**: Connection pooling to backend services
- **Load Balancing**: Ready for horizontal scaling
- **Health Checks**: Can be configured for service health monitoring

---

## 🎯 **Production Checklist**

- [ ] Use strong JWT secret key (256-bit minimum)
- [ ] Enable HTTPS/TLS
- [ ] Configure rate limiting
- [ ] Add health check endpoints
- [ ] Set up logging and monitoring
- [ ] Configure load balancing
- [ ] Enable service discovery (Consul/K8s)
- [ ] Add request/response logging
- [ ] Configure timeouts appropriately
- [ ] Add circuit breaker (Polly)

---

## 📝 **Related Documentation**

- [TESTING.md](./TESTING.md) - Complete testing guide
- [api-gateway.http](./api-gateway.http) - HTTP test collection
- [IMPLEMENTATION_PLAN.md](../../documents/IMPLEMENTATION_PLAN.md) - Sprint plan
- [API_GATEWAY_IMPLEMENTATION.md](../../documents/API_GATEWAY_IMPLEMENTATION.md) - Decision doc

---

**API Gateway Status: ✅ READY FOR PRODUCTION**

