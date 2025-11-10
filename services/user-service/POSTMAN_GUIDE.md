# Postman Collection Guide - User Service

Hướng dẫn sử dụng Postman collection để test User Service API.

## 📦 Files

1. **SPM-User-Service.postman_collection.json** - Collection chứa tất cả các API endpoints
2. **SPM-User-Service.postman_environment.json** - Environment variables cho local development

## 🚀 Cách Import vào Postman

### Bước 1: Import Collection

1. Mở Postman
2. Click **Import** (góc trên bên trái)
3. Chọn file `SPM-User-Service.postman_collection.json`
4. Click **Import**

### Bước 2: Import Environment

1. Click **Import** lại
2. Chọn file `SPM-User-Service.postman_environment.json`
3. Click **Import**
4. Chọn environment **"SPM User Service - Local"** từ dropdown ở góc trên bên phải

## 🔧 Cấu hình Environment

Environment variables mặc định:

| Variable            | Giá trị mặc định        | Mô tả                              |
| ------------------- | ----------------------- | ---------------------------------- |
| `baseUrl`           | `http://localhost:5001` | Base URL của User Service          |
| `accessToken`       | (tự động lưu)           | Access token từ login response     |
| `refreshToken`      | (tự động lưu)           | Refresh token từ login response    |
| `userId`            | (tự động lưu)           | User ID từ register/login response |
| `userEmail`         | `test@example.com`      | Email để test                      |
| `userRole`          | (tự động lưu)           | Role của user                      |
| `expiresAt`         | (tự động lưu)           | Thời gian hết hạn của access token |
| `verificationToken` | (thủ công)              | Token để verify email              |

### Thay đổi Base URL

Nếu service chạy ở port khác hoặc URL khác:

1. Click vào environment **"SPM User Service - Local"**
2. Sửa giá trị của `baseUrl`
3. Click **Save**

## 📋 API Endpoints

### 1. Register - Đăng ký tài khoản mới

**Endpoint:** `POST /api/auth/register`

**Request Body:**

```json
{
  "email": "test@example.com",
  "password": "Password123!",
  "fullName": "Test User"
}
```

**Response:**

```json
{
  "success": true,
  "message": "User registered successfully. Please check your email to verify your account.",
  "data": {
    "userId": "uuid"
  },
  "timestamp": "2025-01-28T12:00:00Z"
}
```

**Lưu ý:**

- `userId` được tự động lưu vào environment variable
- Sau khi register, user sẽ nhận email verification token (cần lấy từ email hoặc database)

### 2. Verify Email - Xác thực email

**Endpoint:** `POST /api/auth/verify-email`

**Request Body:**

```json
{
  "token": "verification-token-from-email"
}
```

**Lưu ý:**

- Token được gửi qua email sau khi register
- Hoặc có thể lấy từ database table `email_verifications`
- Cần set giá trị `verificationToken` trong environment trước khi gọi API này

### 3. Login - Đăng nhập

**Endpoint:** `POST /api/auth/login`

**Request Body:**

```json
{
  "email": "test@example.com",
  "password": "Password123!"
}
```

**Response:**

```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGc...",
    "refreshToken": "base64token...",
    "expiresAt": "2025-01-28T12:15:00Z",
    "user": {
      "id": "uuid",
      "email": "test@example.com",
      "emailConfirmed": true,
      "fullName": "Test User",
      "avatarUrl": null,
      "role": "Member"
    }
  },
  "timestamp": "2025-01-28T12:00:00Z"
}
```

**Lưu ý:**

- `accessToken`, `refreshToken`, `userId`, `userEmail`, `userRole`, `expiresAt` được tự động lưu vào environment
- Các request sau có thể sử dụng `{{accessToken}}` trong header Authorization

### 4. Refresh Token - Làm mới access token

**Endpoint:** `POST /api/auth/refresh`

**Request Body:**

```json
{
  "refreshToken": "refresh-token-from-login"
}
```

**Response:**

```json
{
  "success": true,
  "message": "Token refreshed successfully",
  "data": {
    "accessToken": "new-access-token",
    "refreshToken": "new-refresh-token",
    "expiresAt": "2025-01-28T12:30:00Z",
    "user": {
      "id": "uuid",
      "email": "test@example.com",
      "emailConfirmed": true,
      "fullName": "Test User",
      "avatarUrl": null,
      "role": "Member"
    }
  },
  "timestamp": "2025-01-28T12:15:00Z"
}
```

**Lưu ý:**

- Sử dụng `{{refreshToken}}` từ environment (tự động lấy từ login)
- Token mới được tự động cập nhật vào environment

## 🧪 Automated Tests

Mỗi request đều có automated tests:

- **Status code validation** - Kiểm tra status code là 200
- **Response structure validation** - Kiểm tra cấu trúc response
- **Auto-save tokens** - Tự động lưu tokens vào environment variables

### Xem kết quả test:

1. Gửi request
2. Click tab **Test Results** ở phần response
3. Xem các test cases đã pass/fail

## 🔄 Workflow Test

### Flow 1: Đăng ký → Verify Email → Login

1. **Register** - Đăng ký tài khoản mới

   - Sửa email trong request body nếu cần
   - Gửi request
   - Lưu `userId` từ response

2. **Verify Email** - Xác thực email

   - Lấy verification token từ email hoặc database
   - Set `verificationToken` trong environment
   - Gửi request với `{{verificationToken}}`

3. **Login** - Đăng nhập
   - Sử dụng email/password đã register
   - Tokens được tự động lưu vào environment

### Flow 2: Login → Refresh Token

1. **Login** - Đăng nhập để lấy tokens
2. **Refresh Token** - Làm mới token (tự động sử dụng `{{refreshToken}}`)

## 🛠️ Troubleshooting

### Lỗi: "Cannot connect to server"

- Kiểm tra User Service có đang chạy không: `http://localhost:5001`
- Kiểm tra `baseUrl` trong environment có đúng không
- Nếu chạy bằng Docker: `docker-compose up -d user-service`

### Lỗi: "Email already exists"

- Thử đổi email trong request body
- Hoặc xóa user cũ từ database

### Lỗi: "Invalid verification token"

- Kiểm tra token có đúng không
- Token có thể đã hết hạn (mặc định 24 giờ)
- Lấy token mới từ database: `SELECT token FROM spm_user.email_verifications ORDER BY created_at DESC LIMIT 1;`

### Tokens không tự động lưu

- Kiểm tra environment đã được chọn chưa
- Kiểm tra Tests tab có chạy thành công không
- Xem Console log trong Postman để debug

## 📝 Ghi chú

- Tất cả các endpoint đều trả về format `ApiResponse<T>`
- Access token có thời hạn 15 phút (mặc định)
- Refresh token có thời hạn 7 ngày (mặc định)
- Password phải có ít nhất 6 ký tự (theo validation)
- Email phải đúng format và chưa tồn tại trong hệ thống

## 🔗 Liên kết

- [User Service README](./README.md)
- [API Documentation](./README.md#api-endpoints)
