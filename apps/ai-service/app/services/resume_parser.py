import logging
from app.providers.base import ProviderConfig
from app.schemas.resume import (
    ResumeParseRequest,
    ResumeParseResponse,
)
from .base import BaseAIService

logger = logging.getLogger("ai-service.resume")


class ResumeParser(BaseAIService):
    async def parse(self, request: ResumeParseRequest) -> ResumeParseResponse:
        provider_config = ProviderConfig(
            provider_type=request.provider.provider_type,
            model_name=request.provider.model_name,
            api_key=request.provider.api_key,
            base_url=request.provider.base_url,
            temperature=request.provider.temperature,
            max_tokens=request.provider.max_tokens,
        )

        provider = self._get_provider(provider_config)
        system_prompt = self._load_prompt("resume")
        user_prompt = f"{system_prompt}\n\nResume text:\n{request.resume_text}"

        try:
            result = await provider.structured(user_prompt, ResumeParseResponse, provider_config)
            return result
        except Exception as exc:
            logger.error("Resume parsing failed: %s", exc)
            raise
