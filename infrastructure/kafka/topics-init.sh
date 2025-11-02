#!/bin/bash

# Wait for Kafka to be ready
echo "Waiting for Kafka to be ready..."
sleep 30

# List of Kafka topics to create
TOPICS=(
  "user.created"
  "user.updated"
  "project.created"
  "project.updated"
  "task.created"
  "task.updated"
  "task.status.changed"
  "task.assigned"
  "comment.created"
  "file.uploaded"
  "notification.send"
)

KAFKA_BOOTSTRAP_SERVER=kafka:9092

for TOPIC in "${TOPICS[@]}"; do
  echo "Creating topic: $TOPIC"
  kafka-topics --create \
    --bootstrap-server $KAFKA_BOOTSTRAP_SERVER \
    --topic $TOPIC \
    --partitions 3 \
    --replication-factor 1 \
    --if-not-exists || echo "Topic $TOPIC already exists or creation failed"
done

echo "Kafka topics setup completed!"

