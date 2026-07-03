# 08_Docker_Deployment.md

**Document Status**

**Version:** 1.0  
**Status:** Approved for Implementation

---

# 1. Purpose

This document defines the containerization strategy for the Browser Agent Framework.

Docker is not an optional deployment target—it is the **primary development, testing, and deployment environment**.

Every component of the Browser Agent Framework must be capable of running inside containers without requiring modifications to the application code.

A contributor should be able to clone the repository, execute a single command, and have the complete development environment running.

---

# 2. Goals

The Docker architecture must provide:

- Zero manual setup
- Cross-platform compatibility
- Local development environment
- Production-ready images
- Service isolation
- Simple scaling
- Easy onboarding for contributors
- Consistent runtime environments

---

# 3. Container Philosophy

Each application owns exactly one container.

Containers should have a single responsibility.

Example

```
React

↓

Container

.NET API

↓

Container

Python AI

↓

Container

PostgreSQL

↓

Container
```

Future services should receive their own containers.

---

# 4. Phase 1 Containers

The initial Docker Compose environment consists of

```
browser-agent-web

browser-agent-api

browser-agent-ai

browser-agent-postgres
```

Future additions

```
redis

minio

grafana

prometheus

nginx

rabbitmq

ollama
```

---

# 5. Repository Layout

```
docker/

├── api/

├── ai/

├── web/

├── postgres/

├── scripts/

└── compose/
```

Docker-related files should remain isolated from application code.

---

# 6. Docker Compose

The project root contains

```
docker-compose.yml
```

Future

```
docker-compose.dev.yml

docker-compose.prod.yml

docker-compose.override.yml
```

Compose files should remain environment-specific.

---

# 7. Networking

Docker Compose should create an isolated network.

Example

```
browser-agent-network
```

All internal services communicate using container names.

Never use localhost inside containers.

Example

Correct

```
http://browser-agent-ai
```

Incorrect

```
http://localhost
```

---

# 8. Environment Variables

Infrastructure configuration should use environment variables.

Examples

Database

JWT

Logging

Storage

Ports

Never store

User API Keys

Provider Credentials

Workflow Data

Those belong inside PostgreSQL.

---

# 9. .env Files

Repository should include

```
.env.example
```

Never commit

```
.env
```

Contributors create

```
.env
```

locally.

---

# 10. Backend Container

The .NET container should include

.NET Runtime

Playwright

Required browser dependencies

Health endpoint

Graceful shutdown

The container should expose

```
8080
```

internally.

---

# 11. Browser Dependencies

Since Playwright executes browsers, required system packages must exist inside the container.

Image should include

Chromium runtime

Fonts

Certificates

Required Linux libraries

No manual installation after startup.

---

# 12. Python AI Container

Contains

Python 3.12

FastAPI

Uvicorn

uv

Required AI libraries

No browser dependencies should exist here.

The AI container should remain lightweight.

---

# 13. React Container

Contains

Node

Vite

Production build

Development hot reload

Future

Static Nginx deployment

---

# 14. PostgreSQL Container

Responsibilities

Persistent storage

Automatic initialization

Volume mounting

Database health checks

Future

Replication

Backups

---

# 15. Persistent Volumes

Persist

Database

Uploaded Documents

Logs

Screenshots

Future

Browser recordings

Model cache

Containers should remain disposable.

---

# 16. Startup Order

Preferred startup sequence

```
PostgreSQL

↓

AI Service

↓

API

↓

React
```

Health checks should determine readiness.

Never rely on startup timing.

---

# 17. Health Checks

Every service exposes

Health endpoint

Compose waits until

Healthy

before dependent services start.

Checks

API

AI

Database

Future

Redis

Storage

---

# 18. Image Naming

Images should follow

```
browser-agent-api

browser-agent-web

browser-agent-ai

browser-agent-postgres
```

Future

```
browser-agent-worker

browser-agent-redis
```

---

# 19. Build Strategy

Use multi-stage builds.

Example

Restore

↓

Build

↓

Publish

↓

Runtime

Final runtime images should contain only required artifacts.

---

# 20. Image Size

Optimize

Remove unnecessary packages.

Avoid development tools in production images.

Keep runtime images as small as practical without sacrificing maintainability.

---

# 21. Logging

Containers should write logs to

stdout

stderr

Docker captures logs.

Avoid writing application logs exclusively to files.

Future

Centralized logging.

---

# 22. Configuration Management

Configuration priority

Environment Variables

↓

Application Configuration

↓

Defaults

Infrastructure configuration belongs to Docker.

User configuration belongs to PostgreSQL.

---

# 23. Secrets

Never bake secrets into Docker images.

Never copy

API Keys

JWT Secrets

Passwords

Certificates

into images.

Production should use secret management.

---

# 24. Local Development

Developer workflow

```
git clone

↓

Create .env

↓

docker compose up

↓

Everything Works
```

No manual dependency installation should be required.

---

# 25. Production Deployment

Future deployment should support

Docker Swarm

Kubernetes

Azure Container Apps

AWS ECS

Google Cloud Run

Architecture should not require changes.

---

# 26. Resource Limits

Future Compose files should define

CPU

Memory

Restart Policy

Health Retry

Phase 1 may use defaults.

---

# 27. CI/CD Compatibility

Docker images should be buildable through GitHub Actions.

Every pull request should verify

Application builds

Docker builds

Tests pass

No runtime failures

---

# 28. Browser Considerations

The Browser Engine runs inside the API container.

Requirements

Headless Chromium

Sandbox configuration

Proper permissions

Graceful cleanup

Future

Remote browser execution

Browser pool

Dedicated browser workers

---

# 29. AI Provider Considerations

Hosted providers

No additional containers.

Local providers

Future

```
ollama
```

can run as an optional container.

The architecture should support both hosted and local models without code changes.

---

# 30. File Storage

Phase 1

Local Volume

Future

Azure Blob

S3

MinIO

Storage implementation should be abstracted.

Docker should mount upload directories.

---

# 31. Backups

Future support

Database backups

Document backups

Workflow snapshots

Not required for Phase 1.

---

# 32. Monitoring

Future containers

Prometheus

Grafana

Jaeger

OpenTelemetry Collector

Architecture should remain compatible.

---

# 33. Security

Containers should

Run as non-root whenever practical.

Expose only required ports.

Use minimal runtime images.

Avoid unnecessary Linux packages.

Images should be rebuilt regularly to receive security updates.

---

# 34. Testing

Docker should support

Development

Integration Tests

End-to-End Tests

GitHub Actions

Every environment should use the same Compose definitions whenever possible.

---

# 35. Acceptance Criteria

Docker deployment is considered complete when

- Every application runs inside its own container
- A single `docker compose up` starts the complete environment
- No manual dependency installation is required
- Health checks function correctly
- Volumes persist application data
- Internal networking functions without localhost dependencies
- Images build successfully in CI
- Playwright operates correctly inside the API container
- The AI Service communicates with the API through Docker networking
- PostgreSQL persists data across container restarts

---

# 36. Future Compatibility

The Docker architecture must support future additions including

- Redis
- MinIO
- Ollama
- Background Workers
- Browser Pools
- Monitoring Stack
- Distributed Execution
- Kubernetes
- Cloud Deployments

without requiring redesign of existing containers.

---

# 37. Final Statement

Docker is the canonical runtime environment for the Browser Agent Framework. Every contributor, CI pipeline, and production deployment should rely on the same containerized architecture, ensuring consistency, reproducibility, and simplified onboarding while keeping infrastructure concerns isolated from application code.

---

**End of Document — 08_Docker_Deployment.md**