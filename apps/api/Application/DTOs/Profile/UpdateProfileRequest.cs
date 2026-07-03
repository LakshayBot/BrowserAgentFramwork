namespace BrowserAgent.Api.Application.DTOs.Profile;

public class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? LinkedIn { get; set; }
    public string? GitHub { get; set; }
    public string? Portfolio { get; set; }
    public string? Website { get; set; }
    public string? Summary { get; set; }
}
