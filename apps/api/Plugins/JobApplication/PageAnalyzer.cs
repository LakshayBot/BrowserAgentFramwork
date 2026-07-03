using BrowserAgent.Api.Browser.Interfaces;

namespace BrowserAgent.Api.Plugins.JobApplication;

public enum PageType
{
    Unknown,
    JobListing,
    ApplicationForm,
    LoginRequired,
    AccountRegistration,
    MultiStepForm,
    ReviewPage,
    HumanVerification,
    Submitted
}

public class PageAnalysis
{
    public PageType Type { get; set; } = PageType.Unknown;
    public string? Description { get; set; }
    public bool HasApplyButton { get; set; }
    public bool HasSubmitButton { get; set; }
    public bool HasLoginForm { get; set; }
    public bool HasReviewIndicators { get; set; }
    public bool RequiresHumanVerification { get; set; }
    public double Confidence { get; set; }
}

public class PageAnalyzer
{
    public PageAnalysis Analyze(ExtractedPage page, string url)
    {
        if (page is null)
            return new PageAnalysis { Type = PageType.Unknown, Description = "No page data" };

        var analysis = new PageAnalysis();

        var buttonsText = string.Join(" ", page.Buttons.Select(b => (b.Text ?? "").ToLowerInvariant()));
        var allText = (string.Join(" ", page.Headings) + " " + buttonsText + " " + string.Join(" ", page.Links)).ToLowerInvariant();

        analysis.HasApplyButton = HasAny(buttonsText, "apply", "apply now", "submit application", "start application", "i'm interested");
        analysis.HasSubmitButton = HasAny(buttonsText, "submit", "submit application", "send application");
        analysis.HasLoginForm = HasAny(allText, "sign in", "login", "log in");

        analysis.HasReviewIndicators = CountMatches(allText, "review", "confirm", "verify your", "summary", "preview your") >= 2;
        analysis.RequiresHumanVerification = HasAny(allText, "captcha", "verification code", "verify your email", "sms code", "security check", "are you a robot", "two-factor");

        analysis.Type = DeterminePageType(analysis, page, url);
        analysis.Description = analysis.Type switch
        {
            PageType.JobListing => "Job listing with apply button",
            PageType.ApplicationForm => "Application form detected",
            PageType.LoginRequired => "Login required to continue",
            PageType.AccountRegistration => "Account registration required",
            PageType.MultiStepForm => "Multi-step application form",
            PageType.ReviewPage => "Application review page",
            PageType.HumanVerification => "Human verification required",
            PageType.Submitted => "Application already submitted",
            _ => "Unknown page type"
        };
        analysis.Confidence = analysis.Type != PageType.Unknown ? 0.85 : 0.3;

        return analysis;
    }

    private static PageType DeterminePageType(PageAnalysis analysis, ExtractedPage page, string url)
    {
        if (analysis.RequiresHumanVerification)
            return PageType.HumanVerification;

        if (analysis.HasReviewIndicators && analysis.HasSubmitButton)
            return PageType.ReviewPage;

        var urlLower = url.ToLowerInvariant();
        if (urlLower.Contains("review") || urlLower.Contains("confirm"))
            return PageType.ReviewPage;

        if (analysis.HasApplyButton && HasForms(page))
            return PageType.JobListing;

        if (analysis.HasLoginForm && !analysis.HasApplyButton)
            return PageType.LoginRequired;

        if (urlLower.Contains("register") || urlLower.Contains("signup") || urlLower.Contains("create-account"))
            return PageType.AccountRegistration;

        if (HasForms(page))
            return PageType.ApplicationForm;

        if (urlLower.Contains("submit") || urlLower.Contains("success") || urlLower.Contains("thank"))
            return PageType.Submitted;

        return PageType.Unknown;
    }

    private static bool HasForms(ExtractedPage page) =>
        page.Forms is not null && page.Forms.Any(f => f.Fields.Count > 0);

    private static bool HasAny(string text, params string[] keywords) =>
        keywords.Any(k => text.Contains(k));

    private static int CountMatches(string text, params string[] keywords) =>
        keywords.Count(k => text.Contains(k));
}
