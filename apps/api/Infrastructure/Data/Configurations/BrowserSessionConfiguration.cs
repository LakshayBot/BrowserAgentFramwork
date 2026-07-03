using BrowserAgent.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrowserAgent.Api.Infrastructure.Data.Configurations;

public class BrowserSessionConfiguration : IEntityTypeConfiguration<BrowserSession>
{
    public void Configure(EntityTypeBuilder<BrowserSession> builder)
    {
        builder.ToTable("browser_sessions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.BrowserType).IsRequired().HasMaxLength(50);
        builder.Property(x => x.SessionIdentifier).HasMaxLength(256);
        builder.Property(x => x.CurrentUrl).HasMaxLength(2048);
        builder.Property(x => x.CurrentTitle).HasMaxLength(500);
        builder.Property(x => x.LastScreenshot).HasMaxLength(1024);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");

        builder.HasOne(x => x.Workflow).WithOne(x => x.BrowserSession)
            .HasForeignKey<BrowserSession>(x => x.WorkflowId);
    }
}
