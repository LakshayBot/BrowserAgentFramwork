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
        "field_mapping_html": (
            "You are a browser automation agent. Given raw HTML of a job application page, "
            "identify all form input fields, select dropdowns, and textareas. "
            "For each field, determine a unique CSS selector (prefer #id, then [name=\"...\"], then label text). "
            "Then map the user's profile and resume data to the matching fields based on field labels, "
            "placeholder text, and surrounding context.\n\n"
            "Return a FieldMapResponse with:\n"
            "- mappings: one FieldMapping per field with field_id (label), selector (CSS), value (mapped data), confidence (0-1)\n"
            "- unknown_fields: any field IDs you couldn't map\n"
            "- confidence_overall: 0-1 average confidence\n\n"
            "Only map fields you are confident about (0.3+). Use id selectors when available (e.g., #firstName). "
            "Fall back to [name=\"...\"] selectors. For selects/dropdowns, pick the best matching option."
        ),
        "company": "Analyze the company and job description.",
        "question": "Answer the application question based on the user's profile and resume.",
    }
    return defaults.get(category, "Process the following request.")
