using BrowserAgent.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrowserAgent.Api.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.EventType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Timestamp).HasDefaultValueSql("now() at time zone 'utc'");

        builder.HasOne(x => x.Workflow).WithMany(x => x.AuditLogs).HasForeignKey(x => x.WorkflowId);
        builder.HasOne(x => x.User).WithMany(x => x.AuditLogs).HasForeignKey(x => x.UserId);
        builder.HasIndex(x => x.WorkflowId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.EventType);
    }
}
