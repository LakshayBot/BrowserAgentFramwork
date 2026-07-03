using BrowserAgent.Api.Infrastructure.Data;
using BrowserAgent.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrowserAgent.Api.Infrastructure.Services;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Roles.AnyAsync())
        {
            context.Roles.AddRange(
                new Role { Id = Guid.NewGuid(), Name = "User" },
                new Role { Id = Guid.NewGuid(), Name = "Admin" }
            );
            await context.SaveChangesAsync();
        }
    }
}
