namespace BrowserAgent.Api.Application.DTOs.AI;

public class CompanyAnalyzeRequest
{
    public string Company { get; set; } = string.Empty;
    public string JobDescription { get; set; } = string.Empty;
    public ProviderConfigDto Provider { get; set; } = null!;
}

public class CompanyAnalyzeResponse
{
    public string Industry { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Seniority { get; set; } = string.Empty;
    public List<string> RequiredSkills { get; set; } = new();
    public List<string> PreferredSkills { get; set; } = new();
    public List<string> Responsibilities { get; set; } = new();
    public List<string> Technologies { get; set; } = new();
    public string CompanyOverview { get; set; } = string.Empty;
}
