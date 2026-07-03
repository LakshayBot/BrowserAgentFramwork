using BrowserAgent.Api.Domain.Entities;
using BrowserAgent.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrowserAgent.Api.Infrastructure.Data.Configurations;

public class ProviderConfigConfiguration : IEntityTypeConfiguration<ProviderConfig>
{
    public void Configure(EntityTypeBuilder<ProviderConfig> builder)
    {
        builder.ToTable("provider_configs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.ModelName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.EncryptedApiKey).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.BaseUrl).HasMaxLength(500);
        builder.Property(x => x.Temperature).HasDefaultValue(0.0);
        builder.Property(x => x.MaxTokens).HasDefaultValue(4096);
        builder.Property(x => x.IsDefault).HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
        builder.Property(x => x.ProviderType).HasConversion<int>();

        builder.HasOne(x => x.User).WithMany(x => x.ProviderConfigs).HasForeignKey(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.IsDefault });
        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
