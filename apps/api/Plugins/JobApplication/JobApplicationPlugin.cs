using BrowserAgent.Api.Plugins;

namespace BrowserAgent.Api.Plugins.JobApplication;

public class JobApplicationPlugin : IWorkflowPlugin
{
    public string Name => "job-application";
    public string DisplayName => "Job Application";
    public string Description => "Automates job applications on supported platforms. Navigates to job URLs, fills forms, uploads resumes, and pauses before final submission.";
    public string Version => "1.0.0";

    public IReadOnlyList<PluginStep> Steps { get; } = new List<PluginStep>
    {
        new() { Number = 1, Name = "Browser Start", Description = "Launch headless Chromium browser" },
        new() { Number = 2, Name = "Navigation", Description = "Navigate to the job URL" },
        new() { Number = 3, Name = "Page Analysis", Description = "Analyze page to determine context" },
        new() { Number = 4, Name = "Application Entry", Description = "Find and click the Apply button" },
        new() { Number = 5, Name = "Authentication", Description = "Handle login or registration if required" },
        new() { Number = 6, Name = "Form Extraction", Description = "Extract form fields from the page" },
        new() { Number = 7, Name = "AI Field Mapping", Description = "Map profile data to form fields" },
        new() { Number = 8, Name = "Form Filling", Description = "Fill form fields with mapped values" },
        new() { Number = 9, Name = "Document Upload", Description = "Upload resume and cover letter" },
        new() { Number = 10, Name = "Validation", Description = "Check for validation errors" },
        new() { Number = 11, Name = "Multi-Step", Description = "Continue to next step if multi-step form" },
        new() { Number = 12, Name = "Review Detection", Description = "Detect review page and pause before submit" }
    };

    public bool CanHandle(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        return uri.Scheme is "http" or "https";
    }
}
