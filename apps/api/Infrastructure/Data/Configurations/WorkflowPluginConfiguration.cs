using BrowserAgent.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrowserAgent.Api.Infrastructure.Data.Configurations;

public class WorkflowPluginConfiguration : IEntityTypeConfiguration<WorkflowPlugin>
{
    public void Configure(EntityTypeBuilder<WorkflowPlugin> builder)
    {
        builder.ToTable("workflow_plugins");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.PluginName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.DisplayName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Version).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Enabled).HasDefaultValue(true);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");

        builder.HasIndex(x => x.PluginName).IsUnique();
    }
}
