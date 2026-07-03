import json
import logging
from app.providers.base import ProviderConfig
from app.schemas.company import CompanyAnalyzeRequest, CompanyAnalyzeResponse
from .base import BaseAIService

logger = logging.getLogger("ai-service.company")


class CompanyAnalyzer(BaseAIService):
    async def analyze(self, request: CompanyAnalyzeRequest) -> CompanyAnalyzeResponse:
        provider_config = ProviderConfig(
            provider_type=request.provider.provider_type,
            model_name=request.provider.model_name,
            api_key=request.provider.api_key,
            base_url=request.provider.base_url,
            temperature=request.provider.temperature,
            max_tokens=request.provider.max_tokens,
        )

        provider = self._get_provider(provider_config)
        system_prompt = self._load_prompt("company")

        user_prompt = (
            f"{system_prompt}\n\n"
            f"Company: {request.company}\n\n"
            f"Job Description:\n{request.job_description}"
        )

        try:
            result = await provider.structured(user_prompt, CompanyAnalyzeResponse, provider_config)
            return result
        except Exception as exc:
            logger.error("Company analysis failed: %s", exc)
            raise
