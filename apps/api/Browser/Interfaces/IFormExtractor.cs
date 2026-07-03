namespace BrowserAgent.Api.Browser.Interfaces;

public interface IFormExtractor
{
    Task<ExtractedPage> ExtractPageAsync(string sessionId, CancellationToken ct = default);
    Task<ExtractedForm?> ExtractFormAsync(string sessionId, CancellationToken ct = default);
}

public class ExtractedPage
{
    public List<string> Sections { get; set; } = new();
    public List<ExtractedForm> Forms { get; set; } = new();
    public List<ExtractedButton> Buttons { get; set; } = new();
    public List<string> Links { get; set; } = new();
    public List<string> Headings { get; set; } = new();
    public List<string> Navigation { get; set; } = new();
}

public class ExtractedForm
{
    public string? FormId { get; set; }
    public string? FormName { get; set; }
    public List<ExtractedField> Fields { get; set; } = new();
}

public class ExtractedField
{
    public string Id { get; set; } = string.Empty;
    public string? Label { get; set; }
    public string? Placeholder { get; set; }
    public string FieldType { get; set; } = "text";
    public bool Required { get; set; }
    public bool Disabled { get; set; }
    public bool Readonly { get; set; }
    public List<string>? AvailableOptions { get; set; }
    public string? CurrentValue { get; set; }
}

public class ExtractedButton
{
    public string? Text { get; set; }
    public string? Type { get; set; }
    public bool Visible { get; set; }
    public bool Enabled { get; set; }
}
