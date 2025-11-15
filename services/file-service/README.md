# File Service

File upload, storage, and management service for the SPM system.

## Overview

The File Service handles:

- File uploads with validation
- File storage on disk (Docker volume)
- File metadata management in PostgreSQL
- Task attachments (linking files to tasks)
- File download and deletion
- Kafka event publishing for file operations

## Architecture

- **Database Schema**: `spm_file`
- **Storage**: Files stored in Docker volume at `/app/storage`
- **Authentication**: JWT Bearer Token required for all endpoints
- **Events**: Publishes `file.uploaded` events to Kafka

## API Endpoints

### Files

- `POST /api/files/upload` - Upload a file
- `GET /api/files/{id}` - Get file metadata
- `GET /api/files/{id}/download` - Download a file
- `DELETE /api/files/{id}` - Delete a file (soft delete)
- `GET /api/files/my-files` - Get all files uploaded by current user

### Task Attachments

- `POST /api/tasks/{taskId}/attachments` - Attach a file to a task
- `GET /api/tasks/{taskId}/attachments` - Get all attachments for a task
- `DELETE /api/tasks/{taskId}/attachments/{attachmentId}` - Detach a file from a task

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=spm_db;Username=spm_user;Password=spm_pass"
  },
  "JWT": {
    "SecretKey": "your-secret-key-must-be-at-least-32-characters-long",
    "Issuer": "spm-api-gateway",
    "Audience": "spm-services"
  },
  "Kafka": {
    "BootstrapServers": "kafka:9092"
  },
  "FileStorage": {
    "Path": "/app/storage"
  }
}
```

## File Storage

- Files are stored in a Docker volume mounted at `/app/storage`
- Each file is given a unique GUID-based filename to prevent conflicts
- Original filenames are preserved in the database
- Maximum file size: 100 MB (configurable)

## Database Schema

### Files Table

- `id` (UUID) - Primary key
- `original_name` (VARCHAR) - Original filename
- `stored_name` (VARCHAR) - Unique stored filename
- `mime_type` (VARCHAR) - MIME type
- `size` (BIGINT) - File size in bytes
- `storage_path` (VARCHAR) - Full path to file on disk
- `uploaded_by` (UUID) - User ID who uploaded
- `uploaded_at` (TIMESTAMP) - Upload timestamp
- `is_deleted` (BOOLEAN) - Soft delete flag

### Task Attachments Table

- `id` (UUID) - Primary key
- `task_id` (UUID) - Task ID (reference to project-service)
- `file_id` (UUID) - Foreign key to files table
- `uploaded_by` (UUID) - User ID who attached
- `uploaded_at` (TIMESTAMP) - Attachment timestamp

## Development

### Prerequisites

- .NET 8 SDK
- PostgreSQL 16+
- Docker & Docker Compose (for full stack)

### Running Locally

1. Copy `appsettings.example.json` to `appsettings.json` and configure
2. Ensure PostgreSQL is running with `spm_file` schema
3. Run migrations: `dotnet ef database update`
4. Start the service: `dotnet run`

### Running with Docker

```bash
docker-compose up file-service
```

## Kafka Events

### file.uploaded

Published when a file is successfully uploaded.

```json
{
  "FileId": "uuid",
  "UserId": "uuid",
  "FileName": "example.pdf",
  "FileSize": 1024,
  "Timestamp": "2025-01-01T00:00:00Z"
}
```

## Security

- All endpoints require JWT authentication
- File ownership is enforced (users can only delete their own files)
- File size limits prevent DoS attacks
- Files are stored outside the web root
- Soft delete prevents accidental data loss

## Error Handling

- `400 Bad Request` - Invalid file or request
- `401 Unauthorized` - Missing or invalid JWT token
- `404 Not Found` - File not found
- `500 Internal Server Error` - Server error

All errors return standardized `ApiResponse` format.
