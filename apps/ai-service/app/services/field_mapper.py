import json
import logging
from app.providers.base import ProviderConfig
from app.schemas.field_map import FieldMapRequest, FieldMapResponse, FieldMapping
from .base import BaseAIService

logger = logging.getLogger("ai-service.field-map")


class FieldMapper(BaseAIService):
    async def map_fields(self, request: FieldMapRequest) -> FieldMapResponse:
        provider_config = ProviderConfig(
            provider_type=request.provider.provider_type,
            model_name=request.provider.model_name,
            api_key=request.provider.api_key,
            base_url=request.provider.base_url,
            temperature=request.provider.temperature,
            max_tokens=request.provider.max_tokens,
        )

        provider = self._get_provider(provider_config)
        system_prompt = self._load_prompt("field_mapping")

        user_prompt = (
            f"{system_prompt}\n\n"
            f"Page Structure:\n{json.dumps(request.page_schema, indent=2)}\n\n"
            f"Form Fields:\n{json.dumps(request.form_schema, indent=2)}\n\n"
            f"User Profile:\n{json.dumps(request.profile, indent=2)}\n\n"
            f"Resume Data:\n{json.dumps(request.resume, indent=2)}"
        )

        try:
            result = await provider.structured(user_prompt, FieldMapResponse, provider_config)
            return result
        except Exception as exc:
            logger.error("Field mapping failed: %s", exc)
            raise
