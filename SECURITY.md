# Security Policy

## Reporting a Vulnerability

If you discover a security vulnerability, please do NOT open a public issue.

Email the maintainers at security@browser-agent.dev with details.

We will respond within 48 hours and work with you on a fix.

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.x     | Yes                |
| < 1.0   | No                 |

## Security Practices

- API keys are encrypted at rest
- JWT authentication for all endpoints
- No secrets in Docker images
- No hardcoded credentials
- Regular dependency updates
