import logging
from fastapi import APIRouter, HTTPException
from app.schemas.question import QuestionAnswerRequest, QuestionAnswerResponse
from app.services.question_answering import QuestionAnswerer

router = APIRouter(prefix="/answer-question", tags=["question"])
logger = logging.getLogger("ai-service.api")


@router.post("", response_model=QuestionAnswerResponse)
async def answer_question(request: QuestionAnswerRequest):
    try:
        answerer = QuestionAnswerer()
        result = await answerer.answer(request)
        return result
    except ValueError as exc:
        raise HTTPException(status_code=422, detail=str(exc))
    except Exception as exc:
        logger.error("Question answering error: %s", exc)
        raise HTTPException(status_code=500, detail=str(exc))
