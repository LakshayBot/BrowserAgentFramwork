from pydantic import BaseModel, Field
from typing import Optional
from .provider import ProviderConfigRequest


class CompanyAnalyzeRequest(BaseModel):
    company: str = Field(..., description="Company name")
    job_description: str = Field(..., description="Full job description text")
    provider: ProviderConfigRequest


class CompanyAnalyzeResponse(BaseModel):
    industry: str = ""
    role: str = ""
    seniority: str = ""
    required_skills: list[str] = []
    preferred_skills: list[str] = []
    responsibilities: list[str] = []
    technologies: list[str] = []
    company_overview: str = ""
