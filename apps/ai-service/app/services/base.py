from app.providers.base import BaseProvider, ProviderConfig
from app.providers.factory import ProviderFactory
from app.prompts import load_prompt


class BaseAIService:
    def _get_provider(self, config: ProviderConfig) -> BaseProvider:
        return ProviderFactory.get_provider(config)

    def _load_prompt(self, category: str) -> str:
        return load_prompt(category)
