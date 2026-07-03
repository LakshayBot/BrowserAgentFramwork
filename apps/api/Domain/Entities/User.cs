namespace BrowserAgent.Api.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Profile? Profile { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<ProviderConfig> ProviderConfigs { get; set; } = new List<ProviderConfig>();
    public ICollection<Workflow> Workflows { get; set; } = new List<Workflow>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
