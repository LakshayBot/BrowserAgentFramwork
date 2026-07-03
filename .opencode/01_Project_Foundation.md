# 01_Project_Foundation.md

Document Status

Version 1.0
Approved for Implementation

### 1. Executive Summary

This document defines the foundation of the Browser Agent Framework, an open-source platform for building AI-powered browser automation agents.

The framework is intentionally designed to be generic. Job applications are the first plugin, not the core product.

The architecture separates browser automation, orchestration, AI reasoning, persistence, and user management into independent components that can evolve separately.

### 2. Project Vision

### Vision

Build an extensible open-source browser agent platform capable of understanding web pages, reasoning about user goals, and executing workflows across arbitrary websites.

### Initial Plugin

### Job Application Plugin

Phase 1

The first production plugin automates job applications up to the final review step.

Target workflow

Paste Job URL

Open Browser

Find Apply Button

Understand Form

Map Resume

Fill Application

Upload Documents

Handle Multi-Step Flow

Pause for Human Verification

Resume

Stop Before Final Submit

### 3. Architectural Principles

Modular

Every major capability is isolated behind interfaces.

Plugin-first

New workflows can be added without changing the core engine.

Provider-agnostic AI

AI providers are interchangeable.

BYOK

Users own their AI credentials.

Stateless AI Service

The Python service never persists user data.

Docker-first

The entire stack runs through Docker Compose.

Open-source friendly

Clear structure, documentation, and contribution paths.

Extensible

Designed for future agents and plugins.

### 4. High-Level Architecture

### System Overview

React Web App

.NET 9 API

Orchestrator

Browser Engine

Playwright

Python AI

FastAPI

PostgreSQL

Persistence

### Responsibilities

| Component         | Responsibility                                           |
| ----------------- | -------------------------------------------------------- |
| React Web         | UI, authentication, settings, workflow monitoring        |
| .NET API          | Orchestration, plugins, workflows, persistence, security |
| Browser Engine    | Navigation, extraction, typing, clicking, uploads        |
| Python AI Service | Reasoning, field mapping, resume parsing                 |
| PostgreSQL        | Users, profiles, workflows, encrypted provider settings  |

### 5. Repository Structure

Monorepo Layout

browser-agent/

├── apps/

│ ├── api/

│ ├── ai-service/

│ └── web/

├── packages/

│ ├── contracts/

│ ├── sdk/

│ └── shared/

├── plugins/

│ ├── job-application/

│ └── template/

├── docs/

├── docker/

├── scripts/

├── tests/

└── .github/

### Key Rules

* apps/ contains runnable applications.

* packages/ contains shared libraries and contracts.

* plugins/ contains workflow plugins.

* docs/ contains architecture and specifications.

* tests/ contains integration and E2E tests.

### 6. Technology Stack

### Frontend

| Technology  | Version |
| ----------- | ------- |
| React       | 19      |
| Vite        | Latest  |
| TypeScript  | Latest  |
| TailwindCSS | Latest  |
| ShadCN UI   | Latest  |

### Backend

| Technology            | Version |
| --------------------- | ------- |
| .NET                  | 9       |
| ASP.NET Core          | 9       |
| Entity Framework Core | 9       |
| Serilog               | Latest  |
| Playwright            | Latest  |

### AI Service

| Technology | Version |
| ---------- | ------- |
| Python     | 3.12    |
| FastAPI    | Latest  |
| Pydantic   | Latest  |
| Uvicorn    | Latest  |

### Infrastructure

| Technology     | Version |
| -------------- | ------- |
| PostgreSQL     | 16+     |
| Docker         | Latest  |
| Docker Compose | Latest  |

### 7. Authentication Strategy

### Authentication

Use JWT-based authentication.

Requirements

* Register

* Login

* Refresh tokens

* Logout

* Password reset

* Email verification (future)

* Role support (future)

Initial Roles

| Role  | Purpose               |
| ----- | --------------------- |
| User  | Normal usage          |
| Admin | System administration |

### 8. BYOK (Bring Your Own Key)

### Critical Design Decision

The application never owns AI provider keys.

Users can configure

* DeepSeek

* OpenAI (future)

* Anthropic (future)

* Gemini (future)

* Groq (future)

* OpenRouter (future)

* Ollama (local)

* LM Studio (future)

Storage Rules

* Keys are encrypted at rest.

* Keys are stored per user.

* Python service never stores keys.

* Every AI request is self-contained.

### 9. Plugin Architecture

### Goal

Allow new automation workflows to be added independently.

Plugin Interface

IWorkflowPlugin

• Name

• Description

• CanHandle(url)

• StartWorkflow()

• ResumeWorkflow()

First Plugin

Job Application Plugin

Implements IWorkflowPlugin

### 10. Non-Goals (Phase 1)

### Explicitly Out of Scope

* Automatic CAPTCHA solving

* Final application submission

* Autonomous job searching

* Interview scheduling

* Browser extension

* Mobile application

* Distributed worker scaling

* Self-healing AI agents

### 11. Success Criteria

### Phase 1 is successful when

* A user can paste a job URL.

* The browser opens automatically.

* The framework reaches the application form.

* Fields are extracted as structured JSON.

* AI maps user profile data to fields.

* Forms are filled automatically.

* Documents are uploaded.

* Multi-step flows are handled.

* Human verification pauses the workflow.

* The workflow resumes after verification.

* The system stops on the final review page.

* The application is not auto-submitted.

### 12. Acceptance Criteria

### Foundation Acceptance Criteria

* Repository structure exists.

* All services build successfully.

* `docker compose up` starts the stack.

* Health endpoints respond.

* Swagger is available.

* Authentication works.

* BYOK storage works.

* Plugin discovery works.

* Logging is enabled.

* Database migrations run automatically.

### 13. Future Compatibility

The architecture must support future additions without major refactoring.

### Planned Future Features

* Autonomous agents

* Vision models

* Browser recordings

* Workflow templates

* Team collaboration

* Cloud execution

* Distributed workers

* Marketplace for plugins

* MCP integration

* RAG knowledge base

* Fine-tuned models

### 14. Final Statement

This document establishes the architectural foundation for the Browser Agent Framework. All subsequent documents must conform to the principles, boundaries, and design decisions defined here.

End of Document — 01_Project_Foundation.md
