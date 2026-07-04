using BrowserAgent.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrowserAgent.Api.Infrastructure.Data.Configurations;

public class WorkflowLogConfiguration : IEntityTypeConfiguration<WorkflowLog>
{
    public void Configure(EntityTypeBuilder<WorkflowLog> builder)
    {
        builder.ToTable("workflow_logs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Level).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Message).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.Data).HasColumnType("jsonb");
        builder.Property(x => x.ScreenshotPath).HasMaxLength(500);

        builder.HasOne(x => x.Workflow).WithMany().HasForeignKey(x => x.WorkflowId);
        builder.HasIndex(x => x.WorkflowId);
        builder.HasIndex(x => x.Timestamp);
    }
}
