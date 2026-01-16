# API Gateway Testing Guide

## 🎯 Overview

This guide covers comprehensive testing of the API Gateway, including:
- Authentication flow (register → login → token validation)
- Authorization (JWT-protected routes)
- Routing to microservices
- CORS validation
- Error handling

---

## 🚀 Prerequisites

### 1. Start All Services

```bash
# From project root
docker-compose up -d

# Verify services are running
docker ps
```

Expected containers:
- `spm-postgres` (PostgreSQL + pgvector)
- `spm-kafka` (Kafka broker)
- `spm-zookeeper` (Zookeeper)
- `spm-api-gateway` (API Gateway - port 5000)
- `spm-user-service` (User Service - port 5001)
- `spm-project-service` (Project Service - port 5002)
- `spm-file-service` (File Service - port 5003)
- `spm-frontend` (Frontend - port 3000)

### 2. Check Logs (If Issues)

```bash
# API Gateway logs
docker logs spm-api-gateway -f

# User Service logs
docker logs spm-user-service -f

# Project Service logs
docker logs spm-project-service -f
```

---

## 📋 Test Scenarios

### ✅ Test 1: Anonymous Routes (No JWT Required)

**Expected:** `/api/auth/**` routes should work WITHOUT authentication.

```bash
# 1. Register new user
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "password": "Test@1234",
    "fullName": "Test User"
  }'

# Expected Response: 200 OK
# {
#   "success": true,
#   "message": "User registered successfully",
#   "data": { "userId": "...", "verificationToken": "..." }
# }

# 2. Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "password": "Test@1234"
  }'

# Expected Response: 200 OK
# {
#   "success": true,
#   "message": "Login successful",
#   "data": {
#     "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
#     "refreshToken": "...",
#     "user": { "id": "...", "email": "...", "role": "Member" }
#   }
# }
```

**✅ PASS if:** Both requests return 200 OK without Authorization header.

**❌ FAIL if:** Returns 401 Unauthorized.

---

### ✅ Test 2: Protected Routes (JWT Required)

**Expected:** Routes like `/api/projects`, `/api/tasks`, `/api/files` require JWT.

```bash
# Save access token from login
ACCESS_TOKEN="your-access-token-here"

# 1. Get projects (WITH JWT)
curl -X GET http://localhost:5000/api/projects \
  -H "Authorization: Bearer $ACCESS_TOKEN"

# Expected Response: 200 OK
# {
#   "success": true,
#   "message": "Projects retrieved successfully",
#   "data": []
# }

# 2. Get projects (WITHOUT JWT)
curl -X GET http://localhost:5000/api/projects

# Expected Response: 401 Unauthorized
```

**✅ PASS if:** 
- WITH token: 200 OK
- WITHOUT token: 401 Unauthorized

**❌ FAIL if:** Both succeed (authorization not working).

---

### ✅ Test 3: Create Resources Through Gateway

**Expected:** CRUD operations should work through API Gateway.

```bash
ACCESS_TOKEN="your-access-token-here"

# 1. Create Project
curl -X POST http://localhost:5000/api/projects \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Project via Gateway",
    "description": "Testing routing through API Gateway"
  }'

# Expected Response: 201 Created
# {
#   "success": true,
#   "message": "Project created successfully",
#   "data": { "id": "...", "name": "Test Project via Gateway", ... }
# }

# Save project ID
PROJECT_ID="project-id-from-response"

# 2. Create Task
curl -X POST http://localhost:5000/api/tasks \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "projectId": "'$PROJECT_ID'",
    "title": "Test Task",
    "description": "Created through API Gateway",
    "priority": "High",
    "status": "ToDo"
  }'

# Expected Response: 201 Created

# 3. Get Tasks
curl -X GET "http://localhost:5000/api/tasks?projectId=$PROJECT_ID" \
  -H "Authorization: Bearer $ACCESS_TOKEN"

# Expected Response: 200 OK with task list
```

**✅ PASS if:** All operations succeed and return correct data.

**❌ FAIL if:** Any operation fails or returns wrong data.

---

### ✅ Test 4: JWT Token Expiration & Refresh

**Expected:** Expired tokens should fail, refresh should work.

```bash
# 1. Use expired token (wait 15 minutes, or use old token)
curl -X GET http://localhost:5000/api/projects \
  -H "Authorization: Bearer expired-token"

# Expected Response: 401 Unauthorized

# 2. Refresh token
REFRESH_TOKEN="your-refresh-token-here"
curl -X POST http://localhost:5000/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "'$REFRESH_TOKEN'"
  }'

# Expected Response: 200 OK
# {
#   "success": true,
#   "message": "Token refreshed successfully",
#   "data": { "accessToken": "...", "refreshToken": "..." }
# }
```

**✅ PASS if:** Expired token fails, refresh succeeds.

**❌ FAIL if:** Expired token still works.

---

### ✅ Test 5: CORS Validation

**Expected:** CORS should allow requests from `http://localhost:3000`.

```bash
# Test CORS preflight
curl -X OPTIONS http://localhost:5000/api/projects \
  -H "Origin: http://localhost:3000" \
  -H "Access-Control-Request-Method: GET" \
  -H "Access-Control-Request-Headers: Authorization" \
  -v

# Expected Headers in Response:
# Access-Control-Allow-Origin: http://localhost:3000
# Access-Control-Allow-Methods: GET, POST, PUT, DELETE, ...
# Access-Control-Allow-Headers: Authorization, Content-Type, ...
# Access-Control-Allow-Credentials: true
```

**✅ PASS if:** CORS headers are present.

**❌ FAIL if:** CORS headers missing or wrong origin.

---

### ✅ Test 6: AI Features Through Gateway

**Expected:** Semantic search should work through API Gateway.

```bash
ACCESS_TOKEN="your-access-token-here"

# Vector similarity search
curl -X POST http://localhost:5000/api/tasks/search \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "authentication and login implementation",
    "topK": 5
  }'

# Expected Response: 200 OK
# {
#   "success": true,
#   "message": "Search completed successfully",
#   "data": [
#     { "taskId": "...", "title": "...", "similarity": 0.85 },
#     ...
#   ]
# }
```

**Note:** Requires `GEMINI_API_KEY` to be set in environment.

**✅ PASS if:** Search returns results with similarity scores.

**❌ FAIL if:** Returns error or empty results (when tasks exist).

---

### ✅ Test 7: File Upload Through Gateway

**Expected:** Multipart file uploads should be proxied correctly.

```bash
ACCESS_TOKEN="your-access-token-here"

# Upload file
curl -X POST http://localhost:5000/api/files/upload \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -F "file=@/path/to/test-file.txt"

# Expected Response: 200 OK
# {
#   "success": true,
#   "message": "File uploaded successfully",
#   "data": {
#     "id": "...",
#     "fileName": "test-file.txt",
#     "contentType": "text/plain",
#     "sizeInBytes": 1024
#   }
# }
```

**✅ PASS if:** File uploads successfully through gateway.

**❌ FAIL if:** Upload fails or file is corrupted.

---

## 🔍 Troubleshooting

### Issue: 401 Unauthorized on Protected Routes

**Cause:** JWT validation failing.

**Check:**
1. JWT secret key matches across all services
2. Token is valid and not expired
3. Authorization header format: `Bearer <token>`

```bash
# Verify JWT secret in docker-compose.yml
grep JWT_SECRET_KEY docker-compose.yml

# Should be same across:
# - api-gateway
# - user-service
# - project-service
# - file-service
```

### Issue: 404 Not Found

**Cause:** Route not configured in YARP.

**Check:**
```bash
# View API Gateway routes configuration
cat services/api-gateway/appsettings.json | grep -A 5 "Routes"

# Verify route pattern matches your request
```

### Issue: CORS Error in Browser

**Cause:** CORS not configured properly.

**Check:**
```bash
# Verify CORS origin in Program.cs
grep -A 3 "WithOrigins" services/api-gateway/Program.cs

# Should include: http://localhost:3000
```

### Issue: Service Not Responding

**Cause:** Downstream service is down.

**Check:**
```bash
# Check service health
docker ps | grep spm-

# Check service logs
docker logs spm-user-service -f
docker logs spm-project-service -f
docker logs spm-file-service -f

# Restart specific service
docker-compose restart user-service
```

---

## 📊 Test Results Checklist

| Test | Expected Result | Status |
|------|----------------|--------|
| Register (Anonymous) | 200 OK without JWT | ⬜ |
| Login (Anonymous) | 200 OK, returns JWT | ⬜ |
| Get Projects (No JWT) | 401 Unauthorized | ⬜ |
| Get Projects (With JWT) | 200 OK | ⬜ |
| Create Project | 201 Created | ⬜ |
| Create Task | 201 Created | ⬜ |
| Update Task Status | 200 OK | ⬜ |
| Add Comment | 201 Created | ⬜ |
| Search Tasks (AI) | 200 OK | ⬜ |
| Upload File | 200 OK | ⬜ |
| Attach File to Task | 200 OK | ⬜ |
| CORS Preflight | Headers present | ⬜ |
| Token Refresh | 200 OK, new token | ⬜ |
| Expired Token | 401 Unauthorized | ⬜ |

---

## 🎯 Success Criteria

✅ **API Gateway is READY** if:

1. ✅ All anonymous routes work without JWT
2. ✅ All protected routes require valid JWT
3. ✅ JWT validation works correctly
4. ✅ Requests route to correct services
5. ✅ CORS allows frontend access
6. ✅ Create/Read operations work through gateway
7. ✅ File uploads work through gateway
8. ✅ Token refresh works
9. ✅ Proper error responses (401, 404, 500)

---

## 📝 Reporting Issues

If tests fail, provide:
1. Test that failed
2. Expected vs actual response
3. API Gateway logs
4. Service logs (user/project/file)
5. Request curl command used

```bash
# Collect logs
docker logs spm-api-gateway > gateway.log 2>&1
docker logs spm-user-service > user.log 2>&1
docker logs spm-project-service > project.log 2>&1
```

---

**Good luck testing! 🚀**

