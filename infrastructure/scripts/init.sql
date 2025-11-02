-- Enable pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Create schemas
CREATE SCHEMA IF NOT EXISTS spm_user;
CREATE SCHEMA IF NOT EXISTS spm_project;
CREATE SCHEMA IF NOT EXISTS spm_file;
CREATE SCHEMA IF NOT EXISTS spm_notification;
CREATE SCHEMA IF NOT EXISTS spm_ai;

-- Set default search_path
ALTER DATABASE spm_db SET search_path TO public, spm_user, spm_project, spm_file, spm_notification, spm_ai;

