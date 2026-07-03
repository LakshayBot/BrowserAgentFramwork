namespace BrowserAgent.Api.Infrastructure.Services;

public class VerificationDetectionResult
{
    public bool Detected { get; set; }
    public string? VerificationType { get; set; }
    public string? Description { get; set; }
    public double Confidence { get; set; }
}

public class HumanVerificationDetector
{
    private static readonly List<VerificationPattern> Patterns = new()
    {
        new("captcha", "CAPTCHA", "CAPTCHA verification required", 0.95),
        new("i am not a robot", "CAPTCHA", "reCAPTCHA verification detected", 0.95),
        new("recaptcha", "CAPTCHA", "reCAPTCHA widget detected", 0.98),
        new("verify your email", "EmailVerification", "Email verification required", 0.90),
        new("email verification", "EmailVerification", "Email verification code sent", 0.90),
        new("verification code", "EmailVerification", "Verification code input detected", 0.85),
        new("sms code", "SMSVerification", "SMS verification required", 0.90),
        new("phone verification", "SMSVerification", "Phone verification code required", 0.85),
        new("enter the code", "Generic", "Code entry field detected", 0.70),
        new("security check", "SecurityChallenge", "Security challenge page", 0.85),
        new("two-factor", "TwoFactor", "Two-factor authentication required", 0.95),
        new("2fa", "TwoFactor", "Two-factor authentication required", 0.90),
        new("authenticator", "TwoFactor", "Authenticator app code required", 0.90),
        new("identity verification", "IdentityVerification", "Identity verification required", 0.85),
        new("confirm your identity", "IdentityVerification", "Identity confirmation required", 0.85),
    };

    public VerificationDetectionResult Analyze(string pageText, string pageUrl)
    {
        if (string.IsNullOrWhiteSpace(pageText))
            return new VerificationDetectionResult { Detected = false };

        var text = pageText.ToLowerInvariant();
        var url = pageUrl.ToLowerInvariant();

        var matches = Patterns
            .Select(p => new
            {
                p.Type,
                p.Description,
                p.Confidence,
                Match = text.Contains(p.Keyword) || url.Contains(p.Keyword)
            })
            .Where(x => x.Match)
            .ToList();

        if (!matches.Any())
            return new VerificationDetectionResult { Detected = false };

        var best = matches.OrderByDescending(x => x.Confidence).First();

        return new VerificationDetectionResult
        {
            Detected = true,
            VerificationType = best.Type,
            Description = best.Description,
            Confidence = best.Confidence
        };
    }

    private record VerificationPattern(string Keyword, string Type, string Description, double Confidence);
}
