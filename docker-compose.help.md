# Docker Compose Commands Guide

## Quick Start

### Lần đầu tiên hoặc sau khi thay đổi Dockerfile:
```bash
# Build tất cả services
docker-compose build

# Hoặc build một service cụ thể
docker-compose build user-service

# Sau đó start
docker-compose up -d
```

### Các lần sau (không thay đổi Dockerfile):
```bash
# Chỉ cần start - đủ rồi!
docker-compose up -d
```

## Các trường hợp cần rebuild:

1. **Thay đổi Dockerfile hoặc Dockerfile.dev**
   ```bash
   docker-compose build user-service
   docker-compose up -d user-service
   ```

2. **Thay đổi dependencies (package.json, .csproj)**
   ```bash
   docker-compose build --no-cache user-service
   docker-compose up -d user-service
   ```

3. **Lần đầu tiên chạy**
   ```bash
   docker-compose build
   docker-compose up -d
   ```

## Useful Commands

### Xem logs
```bash
# Tất cả services
docker-compose logs -f

# Một service cụ thể
docker-compose logs -f user-service
```

### Stop services
```bash
docker-compose down
```

### Restart một service
```bash
docker-compose restart user-service
```

### Rebuild và restart
```bash
docker-compose up -d --build user-service
```

## Development vs Production

### Development (hiện tại - với hot reload):
- Sử dụng `Dockerfile.dev`
- Hot reload enabled
- Source code mounted

### Production:
- Sử dụng `Dockerfile` (không có .dev)
- Optimized build
- No hot reload

