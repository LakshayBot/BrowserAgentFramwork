from pydantic import BaseModel, Field
from typing import Optional
from .provider import ProviderConfigRequest


class ResumeParseRequest(BaseModel):
    resume_text: str = Field(..., description="Raw text extracted from the resume file")
    provider: ProviderConfigRequest


class EducationEntry(BaseModel):
    institution: str = ""
    degree: str = ""
    field: str = ""
    start_date: str = ""
    end_date: str = ""
    gpa: str = ""


class ExperienceEntry(BaseModel):
    company: str = ""
    title: str = ""
    start_date: str = ""
    end_date: str = ""
    description: str = ""
    technologies: list[str] = []


class ProjectEntry(BaseModel):
    name: str = ""
    description: str = ""
    technologies: list[str] = []
    url: str = ""


class ResumeParseResponse(BaseModel):
    full_name: str = ""
    email: str = ""
    phone: str = ""
    location: str = ""
    linkedin: str = ""
    github: str = ""
    portfolio: str = ""
    summary: str = ""
    skills: list[str] = []
    certifications: list[str] = []
    languages: list[str] = []
    education: list[EducationEntry] = []
    experience: list[ExperienceEntry] = []
    projects: list[ProjectEntry] = []
