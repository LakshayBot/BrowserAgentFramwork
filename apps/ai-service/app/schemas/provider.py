from pydantic import BaseModel, Field
from typing import Optional


class ProviderConfigRequest(BaseModel):
    provider_type: str = Field(..., description="Provider name (DeepSeek, Ollama)")
    model_name: str = Field(..., description="Model to use")
    api_key: str = Field(default="", description="API key for hosted providers")
    base_url: str = Field(default="", description="Custom base URL")
    temperature: float = Field(default=0.7, ge=0.0, le=2.0)
    max_tokens: int = Field(default=4096, ge=1, le=100_000)
