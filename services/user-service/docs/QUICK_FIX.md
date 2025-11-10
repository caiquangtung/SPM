# Quick Fix Guide - User Service

Hướng dẫn nhanh để fix các lỗi thường gặp. Xem [TROUBLESHOOTING.md](./TROUBLESHOOTING.md) để biết chi tiết.

---

## 🚨 Lỗi DNS "Name or service not known"

### Fix nhanh

1. **Cập nhật docker-compose.yml**: Thêm `networks: - spm-network` vào `postgres`, `zookeeper`, `kafka`

```yaml
postgres:
  # ... existing config
  networks:
    - spm-network

zookeeper:
  # ... existing config
  networks:
    - spm-network

kafka:
  # ... existing config
  networks:
    - spm-network
```

2. **Restart services**:

```bash
docker-compose down
docker-compose up -d
```

3. **Verify**:

```bash
docker exec spm-user-service sh -c "getent hosts postgres"
# Kết quả: 172.18.0.X      postgres
```

---

## 🚨 Thiếu tables trong schema spm_user

### Fix nhanh

1. **Tạo migration**:

```bash
cd services/user-service
dotnet ef migrations add InitialCreate --context UserDbContext
```

2. **Apply migration**:

```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet ef database update --context UserDbContext
```

3. **Verify**:

```bash
docker exec spm-postgres psql -U spm_user -d spm_db -c \
  "SELECT table_name FROM information_schema.tables WHERE table_schema = 'spm_user';"
# Kết quả: users, email_verifications, refresh_tokens
```

4. **Rebuild service** (nếu cần):

```bash
docker-compose build user-service
docker-compose up -d user-service
```

---

## 🔍 Kiểm tra nhanh

### Kiểm tra network

```bash
docker inspect spm-postgres --format='{{range $net,$v := .NetworkSettings.Networks}}{{$net}} {{end}}'
docker inspect spm-user-service --format='{{range $net,$v := .NetworkSettings.Networks}}{{$net}} {{end}}'
# Cả hai phải cùng network: spm_spm-network
```

### Kiểm tra tables

```bash
docker exec spm-postgres psql -U spm_user -d spm_db -c \
  "SELECT table_name FROM information_schema.tables WHERE table_schema = 'spm_user';"
# Phải có 3 tables: users, email_verifications, refresh_tokens
```

### Kiểm tra API

```bash
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Password123!","fullName":"Test"}'
# Kết quả: {"success":true,...}
```

### Kiểm tra logs

```bash
docker logs spm-user-service --tail 50
# Không có lỗi DNS, service listening trên port 8080
```

---

## 📚 Xem thêm

- [TROUBLESHOOTING.md](./TROUBLESHOOTING.md) - Hướng dẫn chi tiết
- [README.md](../README.md) - Documentation đầy đủ
- [POSTMAN_GUIDE.md](../POSTMAN_GUIDE.md) - Hướng dẫn test API

---

**Last Updated**: 2025-11-10
