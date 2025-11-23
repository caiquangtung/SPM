# Docker Development Guide - Frontend

## Volume Mount Configuration

Trong `docker-compose.yml`, frontend có cấu hình volumes:

```yaml
volumes:
  - ./frontend:/app # Mount source code từ host
  - /app/node_modules # Anonymous volume - giữ node_modules trong container
  - /app/.next # Anonymous volume - giữ build cache trong container
```

## Cách hoạt động:

1. **`./frontend:/app`** - Mount source code từ host vào container

   - Mọi thay đổi code trên host → tự động sync vào container
   - Hot reload hoạt động

2. **`/app/node_modules`** - Anonymous volume

   - `node_modules` trong container **KHÔNG** được mount từ host
   - `node_modules` trong container là riêng biệt
   - Mục đích: Tránh conflict giữa node_modules của host (có thể khác OS) và container (Linux)

3. **`/app/.next`** - Anonymous volume
   - Build cache được giữ trong container
   - Tránh conflict với build cache trên host

## Khi nào cài đặt package?

### ✅ Cài đặt TRONG Container (Khi frontend chạy trong Docker):

```bash
# Cài package mới
docker exec spm-frontend npm install <package-name>

# Hoặc vào trong container
docker exec -it spm-frontend sh
npm install <package-name>
```

**Lý do:** Vì có volume mount `/app/node_modules`, nên:

- `node_modules` trong container là riêng biệt
- Install trên host không ảnh hưởng đến container
- Cần install trong container để package có sẵn cho Next.js

### ❌ KHÔNG cài đặt trên host (khi frontend chạy trong Docker)

Nếu bạn chạy `npm install` trên host:

- Package sẽ được cài vào `frontend/node_modules` trên host
- Nhưng container không thấy vì có anonymous volume `/app/node_modules`
- Kết quả: Module not found error

## Workflow Development

### Khi thêm package mới:

1. **Cài trong container:**

   ```bash
   docker exec spm-frontend npm install <package-name>
   ```

2. **Package sẽ được thêm vào `package.json`** (vì source code được mount)

3. **Commit `package.json` và `package-lock.json`** vào git

4. **Khi rebuild image**, Dockerfile sẽ chạy `npm ci` và cài lại packages

### Khi chạy frontend local (không Docker):

```bash
cd frontend
npm install  # Cài trên host
npm run dev
```

## Best Practice

1. **Development với Docker:**

   - Luôn cài packages trong container
   - Sử dụng `docker exec spm-frontend npm install <package>`

2. **Production build:**

   - Packages được cài trong Dockerfile build stage
   - Không cần cài thủ công

3. **Đồng bộ package.json:**
   - Sau khi cài package trong container, `package.json` trên host sẽ được update (vì volume mount)
   - Commit cả `package.json` và `package-lock.json`

## Troubleshooting

### Module not found sau khi cài package:

1. Kiểm tra package đã được cài trong container:

   ```bash
   docker exec spm-frontend npm list <package-name>
   ```

2. Nếu chưa có, cài lại:

   ```bash
   docker exec spm-frontend npm install <package-name>
   ```

3. Restart frontend nếu cần:
   ```bash
   docker-compose restart frontend
   ```

### Package.json không sync:

- Đảm bảo volume mount `./frontend:/app` hoạt động
- Kiểm tra: `docker exec spm-frontend cat /app/package.json`
