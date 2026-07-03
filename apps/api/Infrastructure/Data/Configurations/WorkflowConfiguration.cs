using BrowserAgent.Api.Domain.Entities;
using BrowserAgent.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrowserAgent.Api.Infrastructure.Data.Configurations;

public class WorkflowConfiguration : IEntityTypeConfiguration<Workflow>
{
    public void Configure(EntityTypeBuilder<Workflow> builder)
    {
        builder.ToTable("workflows");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.CurrentStep).HasMaxLength(200);
        builder.Property(x => x.CurrentUrl).HasMaxLength(2048);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
        builder.Property(x => x.Status).HasConversion<int>();

        builder.HasOne(x => x.User).WithMany(x => x.Workflows).HasForeignKey(x => x.UserId);
        builder.HasOne(x => x.Plugin).WithMany().HasForeignKey(x => x.PluginId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Status);
    }
}
