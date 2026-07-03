from abc import ABC, abstractmethod
from typing import Any
from pydantic import BaseModel


class ProviderConfig(BaseModel):
    provider_type: str
    model_name: str
    api_key: str = ""
    base_url: str = ""
    temperature: float = 0.7
    max_tokens: int = 4096


class BaseProvider(ABC):
    @abstractmethod
    async def generate(self, prompt: str, config: ProviderConfig) -> str:
        ...

    @abstractmethod
    async def chat(self, messages: list[dict], config: ProviderConfig) -> str:
        ...

    @abstractmethod
    async def structured(
        self, prompt: str, response_model: type[BaseModel], config: ProviderConfig
    ) -> BaseModel:
        ...
