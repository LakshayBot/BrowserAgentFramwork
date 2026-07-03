from pydantic import BaseModel, Field
from typing import Optional
from .provider import ProviderConfigRequest


class FieldMapRequest(BaseModel):
    page_schema: dict = Field(..., description="Extracted page structure")
    form_schema: dict = Field(..., description="Extracted form fields")
    profile: dict = Field(..., description="User profile data")
    resume: dict = Field(..., description="Parsed resume data")
    provider: ProviderConfigRequest


class FieldMapping(BaseModel):
    field_id: str = ""
    value: str = ""
    confidence: float = 0.0
    source: str = ""


class FieldMapResponse(BaseModel):
    mappings: list[FieldMapping] = []
    unknown_fields: list[str] = []
    confidence_overall: float = 0.0
