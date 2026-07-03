using BrowserAgent.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrowserAgent.Api.Infrastructure.Data.Configurations;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("profiles");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.FirstName).HasMaxLength(100);
        builder.Property(x => x.LastName).HasMaxLength(100);
        builder.Property(x => x.Phone).HasMaxLength(30);
        builder.Property(x => x.Location).HasMaxLength(200);
        builder.Property(x => x.LinkedIn).HasMaxLength(500);
        builder.Property(x => x.GitHub).HasMaxLength(500);
        builder.Property(x => x.Portfolio).HasMaxLength(500);
        builder.Property(x => x.Website).HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now() at time zone 'utc'");

        builder.HasOne(x => x.User).WithOne(x => x.Profile).HasForeignKey<Profile>(x => x.UserId);
        builder.HasIndex(x => x.UserId).IsUnique();
    }
}
