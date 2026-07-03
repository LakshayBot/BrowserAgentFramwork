from .base import BaseProvider, ProviderConfig
from .deepseek import DeepSeekProvider
from .ollama import OllamaProvider


class ProviderFactory:
    _providers: dict[str, type[BaseProvider]] = {
        "DeepSeek": DeepSeekProvider,
        "Ollama": OllamaProvider,
    }

    @classmethod
    def register(cls, name: str, provider: type[BaseProvider]) -> None:
        cls._providers[name] = provider

    @classmethod
    def get_provider(cls, config: ProviderConfig) -> BaseProvider:
        provider_cls = cls._providers.get(config.provider_type)
        if provider_cls is None:
            raise ValueError(
                f"Unsupported provider: {config.provider_type}. "
                f"Supported: {', '.join(cls._providers.keys())}"
            )
        return provider_cls()

    @classmethod
    def list_providers(cls) -> list[str]:
        return list(cls._providers.keys())
