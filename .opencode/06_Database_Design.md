# 06_Database_Design.md

**Document Status**

**Version:** 1.0  
**Status:** Approved for Implementation

---

# 1. Purpose

This document defines the database architecture for the Browser Agent Framework.

The database is responsible for persistent application state.

The database **must never** become tightly coupled to any plugin, AI provider, or browser implementation.

The schema should be designed to support:

- Multi-user architecture
- Plugin architecture
- Workflow persistence
- AI provider configuration
- Resume management
- Browser sessions
- Audit logging
- Future extensibility

All business logic must remain inside the .NET Orchestrator.

---

# 2. Database Technology

Database

```
PostgreSQL 16+
```

ORM

```
Entity Framework Core 9
```

Migration Tool

```
EF Core Migrations
```

Database Naming Convention

- snake_case
- Singular table names
- UUID primary keys
- UTC timestamps
- Soft deletes where appropriate

---

# 3. Design Principles

## Normalize First

Avoid duplicate information.

Reference data through foreign keys.

---

## Soft Delete

Entities that belong to users should be soft deleted.

Fields

```
deleted_at

deleted_by
```

---

## Audit Friendly

Every important table should contain

```
created_at

created_by

updated_at

updated_by
```

---

## UUID Primary Keys

Every entity should use UUID.

Never expose sequential IDs.

---

## Store Metadata, Not Files

Large files should never be stored inside PostgreSQL.

Database stores

- metadata
- file path
- hash
- MIME type

Actual files belong in Storage.

---

# 4. Database Modules

```
Authentication

↓

Users

↓

Profiles

↓

Documents

↓

AI Providers

↓

Plugins

↓

Workflows

↓

Workflow State

↓

Browser Sessions

↓

Audit Logs
```

Each module owns its own tables.

---

# 5. Core Tables

Phase 1 database consists of

```
users

roles

user_roles

profiles

documents

provider_configs

workflow_plugins

workflows

workflow_steps

browser_sessions

audit_logs

application_settings
```

Future modules may introduce additional tables without modifying existing ones.

---

# 6. Users

Purpose

Authentication

Basic account information

Fields

```
id

email

password_hash

email_verified

is_active

created_at

updated_at
```

Password hashing must use modern algorithms such as Argon2 or PBKDF2.

Passwords are never reversible.

---

# 7. Roles

Purpose

Authorization

Initial roles

```
User

Admin
```

Future

```
Moderator

Support

Developer
```

Many-to-many relationship

```
users

↓

user_roles

↓

roles
```

---

# 8. User Profiles

Each user owns one profile.

Fields

```
user_id

first_name

last_name

phone

location

linkedin

github

portfolio

website

summary
```

Future additions

- preferred salary
- notice period
- relocation preference

---

# 9. Resume Metadata

Each uploaded document belongs to one user.

Fields

```
id

user_id

document_type

display_name

storage_path

mime_type

file_size

sha256

created_at
```

Supported document types

```
Resume

CoverLetter
```

Future

```
Portfolio

Certificate

Transcript
```

---

# 10. AI Provider Configuration

Each user owns zero or more providers.

Fields

```
id

user_id

provider_name

model_name

encrypted_api_key

base_url

temperature

max_tokens

is_default

created_at
```

Important

API Keys must always be encrypted before persistence.

The database never stores plaintext keys.

---

# 11. Workflow Plugins

Stores registered plugins.

Fields

```
id

plugin_name

display_name

version

enabled

description
```

Plugins should be discoverable without modifying database schema.

---

# 12. Workflows

Every automation creates one workflow.

Fields

```
id

user_id

plugin_id

status

current_step

started_at

completed_at

current_url
```

Workflow status

```
Created

Running

Paused

Completed

Failed

Cancelled
```

---

# 13. Workflow Steps

Each workflow contains many steps.

Fields

```
workflow_id

step_number

step_name

status

started_at

completed_at

error_message
```

Purpose

Replay

Debugging

Recovery

Analytics

---

# 14. Browser Sessions

Represents active browser execution.

Fields

```
workflow_id

browser_type

session_identifier

current_url

current_title

last_screenshot

created_at
```

Sessions are temporary.

Future

Persistent sessions.

---

# 15. Audit Logs

Every major event becomes an audit record.

Fields

```
id

workflow_id

user_id

event_type

description

timestamp
```

Audit logs are immutable.

They should never be updated.

---

# 16. Application Settings

Global configuration.

Examples

```
allow_registration

maintenance_mode

max_resume_size

supported_file_types
```

User-specific settings belong elsewhere.

---

# 17. Relationships

```
User

│

├── Profile

├── Documents

├── Provider Configurations

├── Workflows

└── Audit Logs

Workflow

│

├── Workflow Steps

├── Browser Session

└── Plugin
```

---

# 18. Indexing Strategy

Index

```
email

workflow_status

plugin_name

user_id

workflow_id

provider_name
```

Composite indexes

```
user_id + is_default

workflow_id + step_number
```

Avoid unnecessary indexes.

---

# 19. Transactions

Use transactions for

Workflow Creation

Workflow Completion

Provider Updates

Resume Upload

Never wrap long-running browser operations inside database transactions.

---

# 20. Concurrency

Use optimistic concurrency.

Entities should include

```
row_version
```

or equivalent concurrency tokens.

Avoid table locking.

---

# 21. Encryption

Encrypt

API Keys

OAuth Tokens (future)

Secrets

Never encrypt

Resume metadata

Workflow state

Plugin metadata

Encryption implementation must be replaceable.

---

# 22. Storage Integration

Database stores only metadata.

Storage providers

Local Disk

Azure Blob (future)

AWS S3 (future)

MinIO (future)

The Storage Provider returns a reference.

Database persists that reference.

---

# 23. Migration Strategy

Every schema modification must be created through EF Core Migrations.

Never manually modify production databases.

Migration naming should be descriptive.

Example

```
AddWorkflowTable

AddProviderConfiguration

AddResumeMetadata
```

---

# 24. Backup Strategy

Future support

Nightly backups

Point-in-time recovery

Disaster recovery

Read replicas

Not required for Phase 1.

---

# 25. Performance Expectations

Authentication lookup

< 50 ms

Profile lookup

< 50 ms

Workflow creation

< 100 ms

Workflow update

< 100 ms

Provider lookup

< 50 ms

---

# 26. Data Retention

Workflow history should remain indefinitely.

Audit logs remain immutable.

Temporary browser sessions may be cleaned periodically.

Future

Retention policies.

---

# 27. Testing Requirements

Unit Tests

Entity mappings

Validation

Encryption

Repository behavior

Integration Tests

PostgreSQL

Migrations

Relationships

Cascade rules

Performance Tests

Large workflow histories

Multiple concurrent users

---

# 28. Future Compatibility

Database should support

- Multiple browser engines
- Multiple AI providers
- Plugin marketplace
- Teams
- Organizations
- Shared workflows
- Browser recordings
- Agent memory
- Notifications
- Billing
- Usage analytics

No schema redesign should be required.

---

# 29. Out of Scope

Phase 1 excludes

Redis

Message Queues

Distributed locks

Sharding

Event sourcing

CQRS read models

Time-series databases

These may be introduced later if needed.

---

# 30. Acceptance Criteria

The database design is considered complete when it supports:

- Multi-user authentication
- User profiles
- Resume metadata
- Multiple encrypted AI provider configurations per user
- Workflow persistence
- Workflow recovery
- Plugin registration
- Browser session tracking
- Immutable audit logging
- EF Core migrations
- Future extensibility without major schema changes

---

# 31. Final Statement

The PostgreSQL database serves as the persistent foundation of the Browser Agent Framework. Its schema is intentionally generic, normalized, and plugin-aware, enabling future expansion without tightly coupling storage to browser automation, AI providers, or workflow implementations. All business logic remains in the .NET Orchestrator, ensuring the database remains a reliable persistence layer rather than an execution engine.

---

**End of Document — 06_Database_Design.md**