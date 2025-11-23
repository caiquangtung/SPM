# Login Issue Fix - 23/11/2025

## Vấn đề

1. **Backend trả về `ApiResponse<T>` wrapper nhưng frontend chưa unwrap**

   - Backend đã chuẩn hóa response với `ApiResponse<T>` structure
   - Frontend `authService.login()` expect trực tiếp `AuthResponse` thay vì `ApiResponse<AuthResponse>`
   - Kết quả: `accessToken` và `refreshToken` là `undefined` → cookie set `undefined`
   - **Error:** `access_token=undefined` trong cookies

2. **Frontend trong Docker không thể resolve `host.docker.internal`**

   - Frontend chạy trong Docker container
   - Browser chạy trên host machine
   - Khi browser gọi API, nó gọi từ host → cần dùng `localhost:5001` (port đã expose)
   - `host.docker.internal` chỉ hoạt động từ container → container, không phải browser → container
   - **Error:** `ERR_NAME_NOT_RESOLVED` khi gọi `http://host.docker.internal:5001/api/auth/login`

3. **Frontend đang chạy production build (standalone)**
   - Không có hot reload
   - Code mới không được load
   - Console logs bị minify/remove
   - **Error:** Không thấy logs trong DevTools Console

## Giải pháp

### 1. Unwrap ApiResponse<T> trong authService

**File:** `frontend/lib/auth.ts`

- Sử dụng `unwrapResponse()` helper để unwrap `ApiResponse<AuthResponse>`
- Xử lý cả camelCase và PascalCase (defensive coding)
- Validate tokens trước khi set cookies

### 2. Sửa API URL cho Docker

**File:** `docker-compose.yml`

- Đổi `NEXT_PUBLIC_API_URL` từ `http://host.docker.internal:5001` → `http://localhost:5001`
- Lý do: Browser chạy trên host, gọi `localhost:5001` (port đã expose từ container)

### 3. Chuyển frontend sang dev mode

**Files:**

- `frontend/Dockerfile.dev` - Tạo Dockerfile cho dev mode
- `docker-compose.yml` - Cập nhật để dùng `Dockerfile.dev`

**Thay đổi:**

- Chạy `npm run dev` thay vì production build
- Mount source code để hot reload
- Thêm `WATCHPACK_POLLING=true` cho file watching trong Docker

### 4. Thêm camelCase JSON serialization trong backend

**File:** `services/user-service/Program.cs`

- Thêm `PropertyNamingPolicy.CamelCase` cho JSON serialization
- Đảm bảo backend trả về camelCase (match với frontend types)

## Files đã sửa

1. `frontend/lib/auth.ts` - Unwrap ApiResponse<T>, xử lý camelCase/PascalCase
2. `frontend/lib/api-helpers.ts` - Helper functions để unwrap ApiResponse<T>
3. `frontend/lib/axios.ts` - Cập nhật interceptor để handle wrapped responses cho refresh token
4. `frontend/contexts/AuthContext.tsx` - Sửa redirect logic (dùng router.replace thay vì router.push)
5. `frontend/features/auth/components/LoginForm.tsx` - Loại bỏ duplicate redirect
6. `services/user-service/Program.cs` - Thêm camelCase JSON serialization
7. `docker-compose.yml` - Sửa API URL từ `host.docker.internal` → `localhost:5001`, thêm Dockerfile.dev cho frontend
8. `frontend/Dockerfile.dev` - Tạo dev mode Dockerfile với hot reload

## Code Changes Summary

### Frontend - Unwrap ApiResponse<T>

**Before:**

```typescript
const response = await apiClient.post<AuthResponse>("/auth/login", {...});
Cookies.set("access_token", response.data.accessToken, ...); // undefined!
```

**After:**

```typescript
const response = await apiClient.post<ApiResponse<AuthResponse>>("/auth/login", {...});
const authData = unwrapResponse(response); // Unwrap ApiResponse<T>
Cookies.set("access_token", authData.accessToken, ...); // ✅ Works!
```

### Docker - API URL Fix

**Before:**

```yaml
environment:
  - NEXT_PUBLIC_API_URL=http://host.docker.internal:5001 # ❌ ERR_NAME_NOT_RESOLVED
```

**After:**

```yaml
environment:
  - NEXT_PUBLIC_API_URL=http://localhost:5001 # ✅ Browser calls from host
```

### Backend - camelCase JSON

**Before:**

```csharp
builder.Services.AddControllers();  // PascalCase by default
```

**After:**

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
```

## Kết quả

✅ Login thành công
✅ Tokens được set đúng vào cookies (không còn `undefined`)
✅ Redirect đến `/dashboard` hoạt động
✅ Hot reload cho frontend development
✅ Backend logs hiển thị requests
