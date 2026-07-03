# Contributing to Browser Agent Framework

Thanks for your interest in contributing!

## Development Setup

```bash
git clone https://github.com/browser-agent/browser-agent.git
cd browser-agent
cp .env.example .env
docker compose up
```

## Prerequisites

- Docker
- .NET 10 SDK (for local development)
- Node.js 22+
- Python 3.12+

## Project Structure

```
apps/
├── api/        # .NET API (Orchestrator)
├── ai-service/ # Python AI Service
└── web/        # React Frontend
packages/       # Shared libraries
plugins/        # Workflow plugins
docs/           # Documentation
docker/         # Docker configurations
```

## Branch Strategy

- `main` - Production releases
- `develop` - Integration branch
- `feature/*` - New features
- `bugfix/*` - Bug fixes

## Commit Convention

We use Conventional Commits:
- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation
- `refactor:` - Code refactoring
- `test:` - Testing
- `chore:` - Maintenance

## Pull Requests

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Update documentation
6. Submit a PR to `develop`

## Code Standards

- Follow Clean Architecture
- Use Dependency Injection
- Write XML documentation for public APIs
- Include tests for new features
- Never hardcode AI providers or websites
