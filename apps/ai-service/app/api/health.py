from fastapi import APIRouter

router = APIRouter(tags=["health"])

@router.get("/health")
async def health_check():
    return {
        "status": "Healthy",
        "timestamp": __import__("datetime").datetime.utcnow().isoformat(),
        "service": "browser-agent-ai",
        "version": "1.0.0",
    }
