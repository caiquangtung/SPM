# Healthcheck Configuration Fix

## Issue

The `api-gateway` service was starting before backend services were ready to accept connections, causing race conditions and connection failures.

## Root Cause

1. Backend services (`user-service`, `project-service`, `file-service`) lacked `healthcheck` definitions
2. `api-gateway`'s `depends_on` didn't use `condition: service_healthy`, only checking container "running" state

## Solution

### 1. Added Healthchecks to Backend Services

All three backend services now have healthcheck definitions:

```yaml
healthcheck:
  test:
    ["CMD-SHELL", "curl -f http://localhost:8080/swagger/index.html || exit 1"]
  interval: 10s
  timeout: 5s
  retries: 5
  start_period: 30s
```

**Details:**

- **Test**: Checks if Swagger UI is accessible (indicates service is fully initialized)
- **Interval**: Checks every 10 seconds
- **Timeout**: 5 seconds per check
- **Retries**: 5 attempts before marking unhealthy
- **Start Period**: 30 seconds grace period for initial startup

### 2. Updated API Gateway Dependencies

Changed from:

```yaml
depends_on:
  - user-service
  - project-service
  - file-service
```

To:

```yaml
depends_on:
  user-service:
    condition: service_healthy
  project-service:
    condition: service_healthy
  file-service:
    condition: service_healthy
```

## Benefits

1. **Eliminates Race Conditions**: API Gateway waits for backend services to be fully ready
2. **Prevents Connection Failures**: No more 502/503 errors during startup
3. **Better Startup Order**: Services start in correct dependency order
4. **Reliable Health Monitoring**: Docker Compose can track service health status

## Verification

To verify the fix works:

```bash
# Start services
docker-compose up -d

# Check health status
docker-compose ps

# Watch startup order
docker-compose up --no-deps api-gateway
```

Expected behavior:

- Backend services start first
- Healthchecks pass (status: healthy)
- API Gateway starts only after all dependencies are healthy
- No connection errors in API Gateway logs

## Notes

- Healthcheck uses Swagger endpoint which is available in Development mode
- For production, consider adding dedicated `/health` endpoints
- `start_period: 30s` gives services time to initialize before health checks begin
- `curl` is available in .NET SDK Docker images
