import os
from pathlib import Path


_PROMPT_DIR = Path(__file__).parent


def load_prompt(*subdirs: str, filename: str = "system.md") -> str:
    path = _PROMPT_DIR.joinpath(*subdirs, filename)
    if not path.exists():
        return _default_prompt(subdirs[-1] if subdirs else "general")
    return path.read_text(encoding="utf-8").strip()


def _default_prompt(category: str) -> str:
    defaults = {
        "resume": "Extract structured information from the following resume text.",
        "field_mapping": "Map the user profile data to the form fields shown on the page.",
        "company": "Analyze the company and job description.",
        "question": "Answer the application question based on the user's profile and resume.",
    }
    return defaults.get(category, "Process the following request.")
