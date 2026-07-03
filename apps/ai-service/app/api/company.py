import logging
from fastapi import APIRouter, HTTPException
from app.schemas.company import CompanyAnalyzeRequest, CompanyAnalyzeResponse
from app.services.company_analyzer import CompanyAnalyzer

router = APIRouter(prefix="/company", tags=["company"])
logger = logging.getLogger("ai-service.api")


@router.post("/analyze", response_model=CompanyAnalyzeResponse)
async def analyze_company(request: CompanyAnalyzeRequest):
    try:
        analyzer = CompanyAnalyzer()
        result = await analyzer.analyze(request)
        return result
    except ValueError as exc:
        raise HTTPException(status_code=422, detail=str(exc))
    except Exception as exc:
        logger.error("Company analysis error: %s", exc)
        raise HTTPException(status_code=500, detail=str(exc))
