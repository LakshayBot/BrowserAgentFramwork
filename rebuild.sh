#!/usr/bin/env bash
set -e

echo "=== Building all Docker images ==="
docker compose build

echo ""
echo "=== Starting containers ==="
docker compose up -d

echo ""
echo "=== Checking health ==="
sleep 2
curl -sf http://localhost:8080/health | python3 -m json.tool 2>/dev/null || echo "API still starting..."

echo ""
echo "=== Services ==="
echo "  API:       http://localhost:8080/swagger"
echo "  Frontend:  http://localhost:5173"
echo "  AI:        http://localhost:8000/health"
