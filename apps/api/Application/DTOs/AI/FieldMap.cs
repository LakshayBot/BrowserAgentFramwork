namespace BrowserAgent.Api.Application.DTOs.AI;

public class FieldMapRequest
{
    public Dictionary<string, object> PageSchema { get; set; } = new();
    public Dictionary<string, object> FormSchema { get; set; } = new();
    public Dictionary<string, object> Profile { get; set; } = new();
    public Dictionary<string, object> Resume { get; set; } = new();
    public ProviderConfigDto Provider { get; set; } = null!;
}

public class FieldMapResponse
{
    public List<FieldMapping> Mappings { get; set; } = new();
    public List<string> UnknownFields { get; set; } = new();
    public double ConfidenceOverall { get; set; }
}

public class FieldMapping
{
    public string FieldId { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Source { get; set; } = string.Empty;
}
