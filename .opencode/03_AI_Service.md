````markdown
# 03_AI_Service.md

**Document Status**

**Version:** 1.0  
**Status:** Approved for Implementation

---

# 1. Purpose

The AI Service is responsible for all reasoning within the Browser Agent Framework.

It is **not** responsible for browser automation, business logic, authentication, workflow execution, persistence, or user management.

The AI Service should behave like a pure reasoning engine.

Input:

```text
Structured JSON
```

↓

Reason

↓

Output:

```text
Structured JSON
```

The service must never return responses intended for humans unless explicitly requested.

Every response should follow a predefined schema.

---

# 2. Core Philosophy

The Browser Agent Framework should never become dependent on a specific LLM.

Instead, the AI Service becomes a translation layer between business logic and any language model.

Business Logic

↓

AI Service

↓

Provider

↓

Model

Changing the underlying model should never require changes in the .NET application.

---

# 3. Design Principles

## Stateless

The AI Service stores no user data.

No conversations.

No history.

No resumes.

No provider settings.

No authentication information.

Every request is self-contained.

---

## Provider Agnostic

Supported providers should all implement the same interface.

The AI Service should never contain conditional business logic such as

```python
if provider == "deepseek":
```

outside the provider factory.

---

## Schema Driven

Every endpoint must define:

- Request Schema
- Response Schema

using Pydantic.

No endpoint should return arbitrary JSON.

---

## Predictable

Temperature should default to deterministic values.

Responses should be structured.

The .NET Orchestrator should never need to "guess" the AI output.

---

# 4. Responsibilities

The AI Service owns:

- Resume parsing
- Field mapping
- Company understanding
- Job description analysis
- Question answering
- Prompt management
- Provider abstraction
- Structured output validation

It does NOT own:

- Browser automation
- User authentication
- Workflow state
- Database persistence
- File storage
- Logging into websites

---

# 5. Folder Structure

```text
apps/
└── ai-service/
    ├── app/
    │
    ├── api/
    │
    ├── providers/
    │
    ├── prompts/
    │
    ├── models/
    │
    ├── services/
    │
    ├── parsers/
    │
    ├── validators/
    │
    ├── schemas/
    │
    ├── cache/
    │
    ├── middleware/
    │
    ├── utils/
    │
    └── tests/
    │
    ├── main.py
    └── requirements
```

No business logic should exist inside API controllers.

---

# 6. Technology Stack

Language

Python 3.12

Framework

FastAPI

Validation

Pydantic

HTTP Client

httpx

Server

Uvicorn

Testing

pytest

Dependency Management

uv

---

# 7. AI Provider Architecture

Every provider must implement a common interface.

Example responsibilities:

- Generate
- Chat
- Structured Output
- Health Check

Future capabilities

- Vision
- Embeddings
- Function Calling
- Audio
- Image Understanding

The Orchestrator should never know which provider is used internally.

---

# 8. Supported Providers (Phase 1)

Hosted

- DeepSeek

Local

- Ollama

Future

- OpenAI
- Anthropic
- Gemini
- Groq
- OpenRouter
- LM Studio
- vLLM

The architecture must make future providers plug-and-play.

---

# 9. Bring Your Own Key (BYOK)

The AI Service never owns API keys.

Provider configuration is supplied with every request.

Example request metadata

- Provider
- Model
- API Key
- Base URL
- Temperature
- Max Tokens

The AI Service must never:

- Save API keys
- Cache API keys
- Write API keys to logs

---

# 10. Provider Factory

Provider selection should happen through a factory.

Responsibilities

- Validate provider
- Create provider instance
- Validate capabilities
- Return implementation

No other module should instantiate providers directly.

---

# 11. Prompt Management

Prompts must never be hardcoded.

Instead

```text
prompts/

resume/

field_mapping/

company/

question/

system/
```

Prompt files should be versioned.

Future support:

- Localization
- Prompt versioning
- A/B testing

---

# 12. Resume Parser

Input

Resume file

Output

Structured Resume

The parser should extract

Personal Information

Education

Experience

Projects

Skills

Certifications

Languages

Links

Achievements

The parser should normalize data.

Example

"Dot Net"

↓

".NET"

The parser should never hallucinate missing information.

---

# 13. Field Mapper

Purpose

Match webpage fields with profile data.

Input

Page schema

User profile

Output

Mapped values

Example

"Given Name"

↓

First Name

"Mobile"

↓

Phone

"CV"

↓

Resume

No browser interaction occurs here.

---

# 14. Company Analyzer

Input

Company name

Job description

Output

Structured analysis

Include

Industry

Role

Required skills

Preferred skills

Responsibilities

Technologies

Seniority

This information may be used later for resume selection.

---

# 15. Question Answering

Input

Question

User profile

Resume

Job Description

Output

Answer

The response should be concise.

Avoid unnecessary creativity.

Future support

Custom writing styles.

---

# 16. Structured Outputs

Every endpoint must validate output.

If validation fails

Retry once.

If still invalid

Return structured error.

Never return malformed JSON.

---

# 17. Caching

The AI Service may cache

Prompt templates

Provider metadata

Model capabilities

Never cache

Resumes

Questions

Personal data

API keys

---

# 18. Error Handling

Errors should be categorized.

Validation

Provider

Network

Timeout

Rate Limit

Internal

Return consistent error objects.

Never expose provider stack traces.

---

# 19. Logging

Log

Endpoint

Provider

Model

Execution Time

Token Usage

Success

Failure

Never log

Prompt contents

Resume

API Keys

Personal Information

---

# 20. Security

The AI Service should trust only authenticated requests from the .NET API.

Future

mTLS

API Gateway

JWT between services

Rate limiting

---

# 21. API Endpoints

Phase 1

POST

```text
/resume/parse
```

POST

```text
/field-map
```

POST

```text
/company/analyze
```

POST

```text
/answer-question
```

GET

```text
/health
```

Future

```text
/chat

/vision

/embedding

/summarize
```

---

# 22. Performance Goals

Resume Parsing

< 5 sec

Field Mapping

< 2 sec

Question Answering

< 5 sec

Health Check

< 200 ms

---

# 23. Testing Requirements

Unit Tests

Resume Parser

Field Mapper

Provider Factory

Prompt Loader

Validators

Integration Tests

DeepSeek

Ollama

Schema Validation

Failure Recovery

Target Coverage

Minimum 80%

---

# 24. Future Compatibility

Future capabilities

Vision Models

OCR

Tool Calling

Function Calling

Image Understanding

Browser Screenshot Analysis

Agent Memory

Embeddings

Vector Search

Fine-Tuned Models

None of these should require architectural changes.

---

# 25. Out of Scope

The AI Service must never implement

Browser Automation

Playwright

Authentication

Database Access

Plugin Discovery

Workflow Engine

User Profiles

Browser State

CAPTCHA handling

These belong elsewhere.

---

# 26. Acceptance Criteria

The AI Service is complete when it can:

- Accept structured JSON requests
- Route requests to the configured provider
- Parse resumes into normalized schemas
- Map webpage fields to profile data
- Analyze company/job descriptions
- Generate structured answers to application questions
- Validate every response against Pydantic schemas
- Return deterministic JSON
- Operate without storing any user or provider data
- Support both DeepSeek and Ollama through the same abstraction layer

---

# 27. Final Statement

The AI Service is the reasoning engine of the Browser Agent Framework. It must remain stateless, provider-agnostic, schema-driven, and completely independent from browser automation and business workflows. Future AI capabilities should be introduced by extending provider implementations and services without changing the core architecture.

---

**End of Document — 03_AI_Service.md**
````
