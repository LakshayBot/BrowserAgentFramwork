using BrowserAgent.Api.Domain.Entities;
using BrowserAgent.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrowserAgent.Api.Infrastructure.Data.Configurations;

public class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
{
    public void Configure(EntityTypeBuilder<WorkflowStep> builder)
    {
        builder.ToTable("workflow_steps");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.StepName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
        builder.Property(x => x.Status).HasConversion<int>();

        builder.HasOne(x => x.Workflow).WithMany(x => x.Steps).HasForeignKey(x => x.WorkflowId);
        builder.HasIndex(x => new { x.WorkflowId, x.StepNumber }).IsUnique();
    }
}
