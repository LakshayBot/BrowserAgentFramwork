import logging
from contextlib import asynccontextmanager
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from app.api import health, resume, field_map, company, question
from app.schemas.health import HealthResponse

logger = logging.getLogger("ai-service")


@asynccontextmanager
async def lifespan(app: FastAPI):
    logger.info("AI Service starting")
    yield
    logger.info("AI Service shutting down")


app = FastAPI(
    title="Browser Agent AI Service",
    description="Stateless AI reasoning service for the Browser Agent Framework. Supports provider-agnostic resume parsing, field mapping, company analysis, and question answering.",
    version="1.0.0",
    lifespan=lifespan,
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(health.router)
app.include_router(resume.router)
app.include_router(field_map.router)
app.include_router(company.router)
app.include_router(question.router)


@app.get("/", response_model=HealthResponse)
async def root():
    return HealthResponse(
        status="Healthy",
        version="1.0.0",
        service="browser-agent-ai",
    )
