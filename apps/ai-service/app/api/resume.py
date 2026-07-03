import logging
from fastapi import APIRouter, HTTPException
from app.schemas.resume import ResumeParseRequest, ResumeParseResponse
from app.schemas.error import ErrorResponse
from app.services.resume_parser import ResumeParser

router = APIRouter(prefix="/resume", tags=["resume"])
logger = logging.getLogger("ai-service.api")


@router.post("/parse", response_model=ResumeParseResponse)
async def parse_resume(request: ResumeParseRequest):
    try:
        parser = ResumeParser()
        result = await parser.parse(request)
        return result
    except ValueError as exc:
        raise HTTPException(status_code=422, detail=str(exc))
    except Exception as exc:
        logger.error("Resume parse error: %s", exc)
        raise HTTPException(status_code=500, detail=str(exc))
