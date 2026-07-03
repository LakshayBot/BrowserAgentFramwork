````markdown
# 04_Orchestrator.md

**Document Status**

**Version:** 1.0  
**Status:** Approved for Implementation

---

# 1. Purpose

The .NET API is the central brain of the Browser Agent Framework.

It owns every workflow.

It coordinates communication between every subsystem while ensuring that each component remains isolated from implementation details of the others.

Unlike the Browser Engine or the AI Service, the Orchestrator contains the application's business logic.

Every request coming from the Web UI enters the system through the Orchestrator.

Every interaction with the Browser Engine, AI Service, Database, Storage, Plugins, Authentication, and Workflow Engine is coordinated here.

---

# 2. Architectural Philosophy

The Browser Agent Framework follows the Orchestrator Pattern.

```
                React Web

                     │

                     ▼

             .NET API (Brain)

        ┌────────┼─────────┐

        ▼        ▼         ▼

 Browser     AI Service   Database

        ▼

   Workflow Plugins
```

The Browser Engine never directly talks to Python.

The Python AI Service never directly talks to PostgreSQL.

Plugins never communicate with each other.

Everything flows through the Orchestrator.

---

# 3. Responsibilities

The Orchestrator owns:

- Authentication
- Authorization
- User Profiles
- Workflow execution
- Plugin loading
- Browser lifecycle
- AI communication
- Resume management
- Document management
- Provider management
- Database operations
- Encryption
- Event publishing
- Background jobs
- Logging
- Auditing

The Orchestrator never performs AI reasoning itself.

---

# 4. Clean Architecture

The API must follow Clean Architecture.

```
Presentation

↓

Application

↓

Domain

↓

Infrastructure
```

Dependencies always point inward.

Infrastructure must never contain business rules.

---

# 5. Project Structure

```
apps/api/

├── Api/
│
├── Application/
│
├── Domain/
│
├── Infrastructure/
│
├── Browser/
│
├── Plugins/
│
├── Contracts/
│
├── Shared/
│
├── Workers/
│
└── Tests/
```

---

# 6. Layer Responsibilities

## Api

Controllers

Authentication

Swagger

Request validation

Response formatting

Global exception handling

---

## Application

Business logic

Commands

Queries

Workflow coordination

DTO mapping

Validation

Interfaces

---

## Domain

Entities

Enums

Value Objects

Events

Business Rules

Domain Services

---

## Infrastructure

Database

Repositories

Storage

Encryption

External APIs

Caching

AI Client

---

# 7. Dependency Injection

Every service must be injected.

Never instantiate services manually.

Example

```
WorkflowService

↓

IBrowserService

IAIClient

IStorageService

IPluginLoader
```

Everything must depend on interfaces.

---

# 8. Workflow Engine

Every execution is represented as a Workflow.

Workflow lifecycle

```
Created

↓

Running

↓

Paused

↓

Resumed

↓

Completed

↓

Failed

↓

Cancelled
```

The engine should support restoring workflows from persisted state.

---

# 9. Plugin System

Every workflow belongs to a Plugin.

Example

```
JobApplicationPlugin

GovernmentPortalPlugin

UniversityPlugin

CRMPlugin
```

The Orchestrator discovers plugins automatically.

Plugins should register themselves.

No hardcoded plugin registration.

---

# 10. Workflow Context

Every workflow owns a context.

Context contains

Workflow Id

User Id

Plugin

Current Step

Current URL

Current Page

Current Browser State

Current AI Provider

Variables

Documents

Screenshots

Temporary Values

The Context is persisted after every important step.

---

# 11. Workflow State Persistence

After every significant action

Persist state.

Examples

Navigation

Page extraction

Form filling

Upload

Pause

Resume

Error

Completion

This allows workflow recovery.

---

# 12. Browser Integration

The Browser Engine exposes interfaces.

Example

```
IBrowserManager

INavigationService

IFormExtractor

IInteractionService

IScreenshotService
```

The Orchestrator owns them.

The Browser Engine never owns workflow logic.

---

# 13. AI Integration

Communication occurs through HTTP.

Never call Python directly.

Interfaces

```
IResumeParser

IFieldMapper

IQuestionAnswerer

ICompanyAnalyzer
```

Implementation

↓

HTTP Client

↓

FastAPI

The rest of the application should not know Python exists.

---

# 14. Resume Management

Users may upload multiple resumes.

Example

```
Resume

↓

Software Engineer

↓

Backend

↓

Frontend

↓

Data Engineering
```

The AI decides which resume best matches a job.

The Browser Engine only uploads files.

---

# 15. Document Storage

Supported

Resume

Cover Letter

Certificates

Portfolio

Future

Transcript

Recommendation Letters

Visa Documents

Files should never be stored in the database.

Only metadata.

---

# 16. User Profiles

Each user owns

Personal Information

Contact Details

Skills

Experience

Education

Projects

Social Links

Documents

AI Providers

Application Preferences

The Profile is the canonical source of truth.

---

# 17. Bring Your Own Key (BYOK)

Users configure providers through the application.

Supported

DeepSeek

Ollama

Future

OpenAI

Anthropic

Gemini

Groq

OpenRouter

Provider settings include

Provider

Model

API Key

Base URL

Temperature

Max Tokens

Default Provider

The Orchestrator encrypts API Keys before storage.

---

# 18. Encryption

Encrypt

Provider Keys

Sensitive Tokens

Future Secrets

Never encrypt

Public Resume

Configuration

Job URLs

Use industry standard encryption.

Encryption implementation should be replaceable.

---

# 19. Background Jobs

Background workers should execute

Cleanup

Screenshot compression

Workflow recovery

Expired session cleanup

Future

Notifications

Analytics

Email

Do not execute long-running tasks inside controllers.

---

# 20. Event Bus

Every important action publishes an event.

Examples

```
WorkflowCreated

WorkflowStarted

WorkflowPaused

WorkflowResumed

WorkflowCompleted

WorkflowFailed

ProviderChanged

ResumeUploaded

DocumentDeleted
```

Future plugins may subscribe.

---

# 21. Logging

Log

Workflow ID

User ID

Plugin

Execution Time

Browser Events

AI Calls

Errors

State Changes

Never log

Passwords

API Keys

Resume Contents

Personal Answers

Sensitive Documents

---

# 22. Error Handling

Use centralized exception handling.

Every API returns

```
Success

Data

Errors

Correlation ID
```

Never expose stack traces.

---

# 23. Validation

Use FluentValidation.

Validate

DTOs

Commands

Queries

Settings

Workflow Requests

Never trust client input.

---

# 24. Storage

Abstract storage behind interfaces.

Future implementations

Local Disk

Azure Blob

AWS S3

MinIO

The rest of the application should not care.

---

# 25. Configuration

Infrastructure configuration belongs in

appsettings

Examples

Database

JWT

Logging

Storage

Never store

User API Keys

Provider Configurations

User Preferences

Those belong in PostgreSQL.

---

# 26. Health Checks

Expose

```
/health
```

Verify

Database

Python AI

Storage

Browser

Future

Redis

Queue

Telemetry

---

# 27. API Versioning

Every endpoint should be versioned.

Example

```
/api/v1/
```

Future versions should coexist.

---

# 28. Authentication

JWT

Refresh Tokens

Password Hashing

Secure Cookies (future)

OAuth (future)

Multi-Factor Authentication (future)

Authorization must be policy based.

---

# 29. Observability

Future-ready architecture for

OpenTelemetry

Prometheus

Grafana

Distributed Tracing

Structured Logging

Correlation IDs

Every request should generate a Correlation ID.

---

# 30. Testing

Unit Tests

Application Services

Validators

Workflow Engine

Plugin Loader

Integration Tests

Database

Browser

Python Service

Authentication

End-to-End

Complete workflow

Target Coverage

Minimum 80%

---

# 31. Out of Scope

The Orchestrator must never implement

AI reasoning

Prompt engineering

DOM extraction

Browser interaction

Playwright logic

CAPTCHA solving

Provider SDK logic

Those belong to specialized components.

---

# 32. Acceptance Criteria

The Orchestrator is complete when it can

- Authenticate users
- Load plugins dynamically
- Create workflows
- Persist workflow state
- Launch browser sessions
- Communicate with the AI Service
- Manage user profiles
- Manage encrypted provider configurations
- Publish events
- Resume paused workflows
- Recover interrupted executions
- Expose versioned APIs
- Operate independently of any specific plugin

---

# 33. Final Statement

The .NET Orchestrator is the control plane of the Browser Agent Framework. Every subsystem communicates through it, but no subsystem depends on another directly. This architecture ensures extensibility, testability, maintainability, and long-term scalability while allowing new plugins, AI providers, storage backends, and browser capabilities to be added with minimal changes.

---

**End of Document — 04_Orchestrator.md**
````
