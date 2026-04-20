# Notification Service

Sprint 3 notification microservice for SPM.

## Responsibilities

- Consume Kafka events from project-service
- Persist notifications in PostgreSQL
- Push real-time updates with SignalR
- Expose notification APIs for the frontend

## Endpoints

- `GET /api/notifications`
- `GET /api/notifications/unread-count`
- `PUT /api/notifications/{id}/read`
- `PUT /api/notifications/read-all`
- `GET /hubs/notifications`

## Development

The service uses:

- PostgreSQL schema: `spm_notification`
- Kafka group: `notification-service`
- JWT auth shared with the API Gateway
