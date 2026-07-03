namespace BrowserAgent.Api.Application.DTOs.AI;

public class QuestionAnswerRequest
{
    public string Question { get; set; } = string.Empty;
    public Dictionary<string, object> Profile { get; set; } = new();
    public Dictionary<string, object> Resume { get; set; } = new();
    public string JobDescription { get; set; } = string.Empty;
    public ProviderConfigDto Provider { get; set; } = null!;
}

public class QuestionAnswerResponse
{
    public string Answer { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string BasedOn { get; set; } = string.Empty;
}
