# Frontend Environment Variables Setup

## 📋 Overview

Frontend sử dụng `NEXT_PUBLIC_API_URL` để kết nối đến User Service API.

## 🔧 Setup cho Local Development (npm run dev)

### 1. Tạo file `.env.local` trong thư mục `frontend/`

```bash
# Frontend Environment Variables (Local Development)
NEXT_PUBLIC_API_URL=http://localhost:5001
```

### 2. File `.env` (optional)

File `.env` trong thư mục `frontend/` sẽ được sử dụng nếu `.env.local` không tồn tại:

```bash
# Frontend Environment Variables
NEXT_PUBLIC_API_URL=http://localhost:5001
```

### 3. Restart dev server

```bash
npm run dev
```

## 🐳 Setup cho Docker

### 1. Environment variables trong docker-compose.yml

Docker-compose đã được cấu hình để:

- Pass build args vào Dockerfile tại build time
- Set environment variables tại runtime

```yaml
frontend:
  build:
    context: ./frontend
    args:
      - NEXT_PUBLIC_API_URL=http://localhost:5001
  environment:
    - NEXT_PUBLIC_API_URL=http://localhost:5001
```

### 2. Rebuild container khi thay đổi

```bash
docker-compose build frontend
docker-compose up -d frontend
```

## ⚠️ Lưu ý quan trọng

### Next.js Environment Variables

- **`NEXT_PUBLIC_*` variables**: Được embed vào JavaScript bundle tại **build time**, không phải runtime
- **Build time**: Khi chạy `npm run build` hoặc build Docker image
- **Runtime**: Khi ứng dụng đang chạy

### Khi nào cần rebuild?

- ✅ Thay đổi `NEXT_PUBLIC_API_URL` trong `.env` → Cần restart dev server (`npm run dev`)
- ✅ Thay đổi `NEXT_PUBLIC_API_URL` trong Docker → Cần rebuild container (`docker-compose build frontend`)

### URL Configuration

#### Local Development

- Frontend: `http://localhost:3000` (npm run dev)
- User Service: `http://localhost:5001` (Docker hoặc dotnet run)
- **API URL**: `http://localhost:5001` ✅

#### Docker Setup

- Frontend: `http://localhost:3000` (browser truy cập)
- User Service: `http://localhost:5001` (browser truy cập)
- **API URL**: `http://localhost:5001` ✅

**Lưu ý**: Khi frontend chạy trong Docker nhưng được truy cập từ browser, browser sẽ gọi API đến `http://localhost:5001` từ phía client, không phải từ container. Vậy nên `http://localhost:5001` là đúng.

## 🔍 Troubleshooting

### Frontend vẫn sử dụng port cũ (5000)

1. **Clear browser cache**: Hard refresh (Cmd+Shift+R / Ctrl+Shift+R)
2. **Clear Next.js cache**:
   ```bash
   rm -rf .next
   npm run build
   ```
3. **Rebuild Docker container**:
   ```bash
   docker-compose build --no-cache frontend
   docker-compose up -d frontend
   ```

### CORS Error

Đảm bảo User Service CORS config cho phép `http://localhost:3000`:

```json
{
  "CORS": {
    "AllowedOrigins": ["http://localhost:3000", "https://localhost:3000"]
  }
}
```

### Environment variable không được nhận

1. **Kiểm tra file `.env.local`** có tồn tại và có giá trị đúng không
2. **Kiểm tra tên biến** phải bắt đầu với `NEXT_PUBLIC_`
3. **Restart dev server** sau khi thay đổi
4. **Rebuild container** nếu chạy Docker

## 📚 Tham khảo

- [Next.js Environment Variables](https://nextjs.org/docs/basic-features/environment-variables)
- [Docker Build Args](https://docs.docker.com/engine/reference/builder/#arg)
