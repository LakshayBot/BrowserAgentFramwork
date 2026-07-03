from pydantic import BaseModel, Field
from typing import Optional
from .provider import ProviderConfigRequest


class QuestionAnswerRequest(BaseModel):
    question: str = Field(..., description="Application question text")
    profile: dict = Field(..., description="User profile data")
    resume: dict = Field(..., description="Parsed resume data")
    job_description: str = Field(default="", description="Job description text")
    provider: ProviderConfigRequest


class QuestionAnswerResponse(BaseModel):
    answer: str = ""
    confidence: float = 0.0
    based_on: str = ""
