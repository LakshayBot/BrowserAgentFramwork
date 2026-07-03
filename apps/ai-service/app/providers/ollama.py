import json
import httpx
from pydantic import BaseModel
from .base import BaseProvider, ProviderConfig


class OllamaProvider(BaseProvider):
    DEFAULT_BASE_URL = "http://localhost:11434"

    async def generate(self, prompt: str, config: ProviderConfig) -> str:
        messages = [{"role": "user", "content": prompt}]
        return await self._chat_completion(messages, config)

    async def chat(self, messages: list[dict], config: ProviderConfig) -> str:
        return await self._chat_completion(messages, config)

    async def structured(
        self, prompt: str, response_model: type[BaseModel], config: ProviderConfig
    ) -> BaseModel:
        schema = response_model.model_json_schema()
        messages = [
            {
                "role": "system",
                "content": "You are a structured data extraction assistant. Respond only with valid JSON.",
            },
            {
                "role": "user",
                "content": f"{prompt}\n\nReturn valid JSON conforming to: {json.dumps(schema, indent=2)}",
            },
        ]

        content = await self._chat_completion(messages, config)

        try:
            content = self._clean_json(content)
            data = json.loads(content)
            return response_model.model_validate(data)
        except (json.JSONDecodeError, ValueError) as exc:
            raise ValueError(
                f"Failed to parse structured response: {exc}\nContent: {content[:500]}"
            )

    async def _chat_completion(self, messages: list[dict], config: ProviderConfig) -> str:
        base_url = (config.base_url or self.DEFAULT_BASE_URL).rstrip("/")
        url = f"{base_url}/api/chat"

        payload = {
            "model": config.model_name,
            "messages": messages,
            "options": {
                "temperature": config.temperature,
                "num_predict": config.max_tokens,
            },
            "stream": False,
        }

        async with httpx.AsyncClient(timeout=120.0) as client:
            response = await client.post(url, json=payload)
            response.raise_for_status()
            data = response.json()

        return data["message"]["content"]

    @staticmethod
    def _clean_json(content: str) -> str:
        if content.startswith("```"):
            lines = content.splitlines()
            if lines[0].startswith("```"):
                lines = lines[1:]
            if lines and lines[-1].strip() == "```":
                lines = lines[:-1]
            content = "\n".join(lines)
        return content.strip()
