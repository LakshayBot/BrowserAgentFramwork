import json
import logging
from app.providers.base import ProviderConfig
from app.schemas.question import QuestionAnswerRequest, QuestionAnswerResponse
from .base import BaseAIService

logger = logging.getLogger("ai-service.question")


class QuestionAnswerer(BaseAIService):
    async def answer(self, request: QuestionAnswerRequest) -> QuestionAnswerResponse:
        provider_config = ProviderConfig(
            provider_type=request.provider.provider_type,
            model_name=request.provider.model_name,
            api_key=request.provider.api_key,
            base_url=request.provider.base_url,
            temperature=request.provider.temperature,
            max_tokens=request.provider.max_tokens,
        )

        provider = self._get_provider(provider_config)
        system_prompt = self._load_prompt("question")

        user_prompt = (
            f"{system_prompt}\n\n"
            f"Question: {request.question}\n\n"
            f"User Profile:\n{json.dumps(request.profile, indent=2)}\n\n"
            f"Resume:\n{json.dumps(request.resume, indent=2)}\n\n"
            f"Job Description:\n{request.job_description}"
        )

        try:
            result = await provider.structured(user_prompt, QuestionAnswerResponse, provider_config)
            return result
        except Exception as exc:
            logger.error("Question answering failed: %s", exc)
            raise
