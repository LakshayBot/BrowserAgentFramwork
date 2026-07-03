namespace BrowserAgent.Api.Application.DTOs.Workflows;

public class VerificationStatusResponse
{
    public bool IsAwaitingVerification { get; set; }
    public string? VerificationType { get; set; }
    public string? WorkflowStatus { get; set; }
    public string? CurrentStep { get; set; }
}
