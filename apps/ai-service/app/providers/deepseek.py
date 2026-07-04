import json
from typing import Any
import httpx
from pydantic import BaseModel
from .base import BaseProvider, ProviderConfig


class DeepSeekProvider(BaseProvider):
    DEFAULT_BASE_URL = "https://api.deepseek.com"

    async def generate(self, prompt: str, config: ProviderConfig) -> str:
        messages = [{"role": "user", "content": prompt}]
        return await self._chat_completion(messages, config)

    async def chat(self, messages: list[dict], config: ProviderConfig) -> str:
        return await self._chat_completion(messages, config)

    async def structured(
        self, prompt: str, response_model: type[BaseModel], config: ProviderConfig
    ) -> BaseModel:
        schema = response_model.model_json_schema()
        system_prompt = (
            "You are a structured data extraction assistant. "
            "Respond only with valid JSON matching the provided schema. "
            "Do not include markdown, explanations, or code blocks."
        )

        messages = [
            {"role": "system", "content": system_prompt},
            {
                "role": "user",
                "content": f"{prompt}\n\nRespond with JSON matching this schema:\n{json.dumps(schema, indent=2)}",
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
        url = f"{base_url}/v1/chat/completions"

        headers = {
            "Authorization": f"Bearer {config.api_key}",
            "Content-Type": "application/json",
        }

        payload = {
            "model": config.model_name,
            "messages": messages,
            "temperature": config.temperature,
            "max_tokens": config.max_tokens,
        }

        async with httpx.AsyncClient(timeout=120.0) as client:
            response = await client.post(url, headers=headers, json=payload)
            if not response.is_success:
                raise ValueError(f"DeepSeek API {response.status_code}: {response.text[:500]}")
            data = response.json()

        choice = data["choices"][0]
        return choice["message"]["content"]

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
