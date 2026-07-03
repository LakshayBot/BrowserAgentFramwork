import logging
from fastapi import APIRouter, HTTPException
from app.schemas.field_map import FieldMapRequest, FieldMapResponse
from app.services.field_mapper import FieldMapper

router = APIRouter(prefix="/field-map", tags=["field-map"])
logger = logging.getLogger("ai-service.api")


@router.post("", response_model=FieldMapResponse)
async def map_fields(request: FieldMapRequest):
    try:
        mapper = FieldMapper()
        result = await mapper.map_fields(request)
        return result
    except ValueError as exc:
        raise HTTPException(status_code=422, detail=str(exc))
    except Exception as exc:
        logger.error("Field mapping error: %s", exc)
        raise HTTPException(status_code=500, detail=str(exc))
