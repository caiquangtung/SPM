# API Gateway (YARP)

API Gateway sử dụng YARP (Yet Another Reverse Proxy) để route requests đến các microservices.

## Cấu hình

### Routes

- `/api/auth/**` → `user-service` (Anonymous - không cần JWT)
- `/api/projects/**` → `project-service` (Requires JWT)
- `/api/tasks/**` → `project-service` (Requires JWT)
- `/api/tasks/{taskId}/comments/**` → `project-service` (Requires JWT)
- `/api/files/**` → `file-service` (Requires JWT)
- `/api/tasks/{taskId}/attachments/**` → `file-service` (Requires JWT)

### JWT Authentication

API Gateway validate JWT token cho tất cả routes ngoại trừ `/api/auth/**`.

JWT configuration:
- **SecretKey**: Từ environment variable `JWT__SecretKey`
- **Issuer**: `spm-api-gateway`
- **Audience**: `spm-services`

### CORS

Cho phép requests từ `http://localhost:3000` (frontend).

## Development

```bash
# Run với hot reload
docker-compose up api-gateway

# Hoặc local
cd services/api-gateway
dotnet watch run
```

## Port

- **Development**: `5000:8080` (host:container)
- **Internal**: `8080` (Docker network)

