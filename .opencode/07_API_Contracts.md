# 07_API_Contracts.md

**Document Status**

**Version:** 1.0  
**Status:** Approved for Implementation

---

# 1. Purpose

This document defines the public and internal API contracts of the Browser Agent Framework.

The objective is to establish stable communication between all system components while ensuring loose coupling, versioning, and future extensibility.

There are two categories of APIs:

1. Public APIs (React ↔ .NET)
2. Internal APIs (.NET ↔ Python AI Service)

Every API must be documented through OpenAPI (Swagger).

Every request and response must use strongly typed DTOs.

No endpoint should expose database entities directly.

---

# 2. API Design Principles

## Version First

Every endpoint must be versioned.

Example

```
/api/v1/
```

Future versions

```
/api/v2/
```

must coexist.

---

## Resource Based

Endpoints should represent resources rather than actions.

Good

```
/workflows

/users

/documents
```

Avoid

```
/createWorkflow

/uploadResume
```

---

## DTO Only

Never expose

- Entity Framework entities
- Domain entities
- Internal models

Only DTOs.

---

## Predictable Responses

Every response follows a common envelope.

Example

```json
{
  "success": true,
  "data": {},
  "errors": [],
  "correlationId": ""
}
```

---

## Validation First

Reject invalid requests before business logic executes.

Use FluentValidation.

---

# 3. Authentication APIs

## Register

```
POST

/api/v1/auth/register
```

Purpose

Create a user account.

---

## Login

```
POST

/api/v1/auth/login
```

Returns

- JWT
- Refresh Token
- User Summary

---

## Refresh

```
POST

/api/v1/auth/refresh
```

---

## Logout

```
POST

/api/v1/auth/logout
```

---

## Current User

```
GET

/api/v1/auth/me
```

---

# 4. User APIs

```
GET

/api/v1/users/profile
```

Retrieve profile.

---

```
PUT

/api/v1/users/profile
```

Update profile.

---

Supported fields

- Personal Information
- Contact Details
- Social Links
- Summary

---

# 5. Document APIs

Upload

```
POST

/api/v1/documents
```

---

List

```
GET

/api/v1/documents
```

---

Delete

```
DELETE

/api/v1/documents/{id}
```

---

Download Metadata

```
GET

/api/v1/documents/{id}
```

---

Supported Types

Resume

Cover Letter

Future

Certificates

Portfolio

---

# 6. AI Provider APIs

Create Provider

```
POST

/api/v1/providers
```

---

Update

```
PUT

/api/v1/providers/{id}
```

---

Delete

```
DELETE

/api/v1/providers/{id}
```

---

List

```
GET

/api/v1/providers
```

---

Validate Connection

```
POST

/api/v1/providers/{id}/validate
```

---

Set Default

```
POST

/api/v1/providers/{id}/default
```

---

Provider configuration includes

- Provider Name
- Model
- Base URL
- API Key
- Temperature
- Max Tokens

API Keys must always be encrypted before persistence.

---

# 7. Workflow APIs

Create Workflow

```
POST

/api/v1/workflows
```

Input

Job URL

Returns

Workflow ID

---

Get Workflow

```
GET

/api/v1/workflows/{id}
```

---

Workflow List

```
GET

/api/v1/workflows
```

---

Pause

```
POST

/api/v1/workflows/{id}/pause
```

---

Resume

```
POST

/api/v1/workflows/{id}/resume
```

---

Cancel

```
POST

/api/v1/workflows/{id}/cancel
```

---

Delete

```
DELETE

/api/v1/workflows/{id}
```

---

# 8. Browser APIs

These endpoints communicate with the Browser Engine indirectly.

Users never call Playwright directly.

Examples

Start Browser

Navigate

Current Screenshot

Current State

Future

Live Stream

Browser Recording

---

# 9. Plugin APIs

Installed Plugins

```
GET

/api/v1/plugins
```

---

Plugin Details

```
GET

/api/v1/plugins/{id}
```

---

Future

Install Plugin

Enable Plugin

Disable Plugin

Marketplace

---

# 10. Settings APIs

Application Settings

```
GET

/api/v1/settings
```

---

Update Settings

```
PUT

/api/v1/settings
```

---

User Settings

Should include

Preferred Provider

Theme

Language

Future Preferences

---

# 11. Health APIs

```
GET

/health
```

Checks

Database

AI Service

Browser Engine

Storage

---

Future

Redis

Queues

Telemetry

---

# 12. Internal AI Contracts

The Browser Agent never calls Python.

Only the Orchestrator communicates with the AI Service.

Communication format

```
HTTP

JSON
```

---

# 13. Resume Parsing Contract

Endpoint

```
POST

/resume/parse
```

Input

Resume File

Provider Configuration

Output

Structured Resume

---

# 14. Field Mapping Contract

Endpoint

```
POST

/field-map
```

Input

Page Schema

Form Schema

Profile

Resume

Provider

Output

Mapped Fields

Confidence

Unknown Fields

---

# 15. Company Analysis Contract

Endpoint

```
POST

/company/analyze
```

Input

Company

Job Description

Provider

Output

Structured Analysis

---

# 16. Question Answering Contract

Endpoint

```
POST

/answer-question
```

Input

Question

Resume

Profile

Job Description

Provider

Output

Structured Answer

Confidence

---

# 17. Error Contracts

Every error should contain

```json
{
  "code": "",
  "message": "",
  "details": [],
  "correlationId": ""
}
```

No stack traces.

No provider exceptions.

---

# 18. Validation Contracts

Validation errors should return

Field

Rule

Message

Example

```
Email

Invalid Format
```

---

# 19. Status Codes

200

Success

201

Created

204

Deleted

400

Validation Error

401

Unauthorized

403

Forbidden

404

Not Found

409

Conflict

422

Business Rule Failure

429

Rate Limited

500

Internal Error

---

# 20. Pagination

Collection endpoints must support

Page

Page Size

Sort

Search

Future

Filtering

Cursor Pagination

---

# 21. File Upload Contract

Multipart Form Data

Supported

PDF

DOC

DOCX

Maximum Size

Application Setting

Future

Image Upload

Portfolio

---

# 22. Security

Authentication

JWT

Authorization

Policy Based

HTTPS

Required

CORS

Configured

Rate Limiting

Future

---

# 23. Correlation IDs

Every request generates

Correlation ID

Flows through

Controller

↓

Application

↓

Infrastructure

↓

Browser

↓

AI

↓

Logs

---

# 24. OpenAPI

Swagger must include

Examples

Descriptions

Validation Rules

Authentication

Error Responses

Tags

Every endpoint should be documented.

---

# 25. API Evolution

Rules

Never break clients.

Additive changes preferred.

Deprecate before removal.

Document version changes.

---

# 26. Testing

Contract Tests

DTO Validation

Authentication

Authorization

Versioning

Integration

Swagger

OpenAPI

AI Contracts

Browser Contracts

---

# 27. Future Compatibility

Future APIs

Notifications

Organizations

Teams

Marketplace

Billing

Usage

Browser Recording

Vision

Memory

Chat

---

# 28. Out of Scope

GraphQL

gRPC

SignalR

WebSockets

Streaming

These may be introduced later.

---

# 29. Acceptance Criteria

The API layer is complete when

- Every public capability is accessible through versioned REST endpoints
- DTOs are used exclusively
- Swagger documentation is complete
- Validation is enforced
- Authentication protects private endpoints
- Internal AI contracts are clearly defined
- Responses are consistent
- Correlation IDs exist for every request
- Future versions can be introduced without breaking existing clients

---

# 30. Final Statement

The API contracts establish the communication boundary between every component of the Browser Agent Framework. By enforcing strict versioning, DTO-based communication, predictable responses, and provider-agnostic internal contracts, the system remains maintainable, extensible, and suitable for long-term open-source development.

---

**End of Document — 07_API_Contracts.md**