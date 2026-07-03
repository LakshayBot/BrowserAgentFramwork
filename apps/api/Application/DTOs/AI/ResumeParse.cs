namespace BrowserAgent.Api.Application.DTOs.AI;

public class ResumeParseRequest
{
    public string ResumeText { get; set; } = string.Empty;
    public ProviderConfigDto Provider { get; set; } = null!;
}

public class ResumeParseResponse
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string LinkedIn { get; set; } = string.Empty;
    public string GitHub { get; set; } = string.Empty;
    public string Portfolio { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = new();
    public List<string> Certifications { get; set; } = new();
    public List<string> Languages { get; set; } = new();
    public List<EducationEntry> Education { get; set; } = new();
    public List<ExperienceEntry> Experience { get; set; } = new();
    public List<ProjectEntry> Projects { get; set; } = new();
}

public class EducationEntry
{
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public string Gpa { get; set; } = string.Empty;
}

public class ExperienceEntry
{
    public string Company { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Technologies { get; set; } = new();
}

public class ProjectEntry
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Technologies { get; set; } = new();
    public string Url { get; set; } = string.Empty;
}
