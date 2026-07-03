# Browser Agent Framework

[![CI](https://github.com/browser-agent/browser-agent/actions/workflows/ci.yml/badge.svg)](https://github.com/browser-agent/browser-agent/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

An open-source platform for building AI-powered browser automation agents. The framework separates browser automation, AI reasoning, workflow orchestration, and persistence into independent, reusable components.

## Architecture

```
React Web App  →  .NET Orchestrator  →  Browser Engine (Playwright)
                                 →  Python AI Service (FastAPI)
                                 →  PostgreSQL
```

## Quick Start

```bash
git clone https://github.com/browser-agent/browser-agent.git
cd browser-agent
cp .env.example .env
docker compose up
```

## Services

| Service  | Technology           | Port  |
| -------- | -------------------- | ----- |
| Web      | React 19 + Vite      | 5173  |
| API      | .NET 10 Web API      | 8080  |
| AI       | Python + FastAPI     | 8000  |
| Database | PostgreSQL 16        | 5432  |

## Features (Phase 1)

- Browser automation engine (Playwright)
- AI-powered form understanding and field mapping
- Bring Your Own Key (BYOK) model — no hardcoded AI providers
- Multi-step workflow engine with pause/resume
- Plugin architecture for extensible workflows
- Resume parsing and job application automation

## Documentation

- [Architecture Overview](docs/architecture.md)
- [API Reference](http://localhost:8080/swagger)
- [Contributing Guide](CONTRIBUTING.md)

## License

MIT
