using BrowserAgent.Api.Domain.Entities;
using BrowserAgent.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrowserAgent.Api.Infrastructure.Data.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.DisplayName).IsRequired().HasMaxLength(256);
        builder.Property(x => x.StoragePath).IsRequired().HasMaxLength(1024);
        builder.Property(x => x.MimeType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.FileSize).IsRequired();
        builder.Property(x => x.Sha256).IsRequired().HasMaxLength(64);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
        builder.Property(x => x.DocumentType).HasConversion<int>();

        builder.HasOne(x => x.User).WithMany(x => x.Documents).HasForeignKey(x => x.UserId);
        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
