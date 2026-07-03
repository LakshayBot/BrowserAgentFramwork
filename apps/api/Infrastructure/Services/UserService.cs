using BrowserAgent.Api.Application.DTOs.Profile;
using BrowserAgent.Api.Application.Interfaces;
using BrowserAgent.Api.Domain.Entities;
using BrowserAgent.Api.Domain.Exceptions;
using BrowserAgent.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BrowserAgent.Api.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await _context.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (profile is null)
        {
            throw new NotFoundException("Profile", userId);
        }

        return MapToDto(profile);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var profile = await _context.Profiles
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (profile is null)
        {
            profile = new Profile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Profiles.Add(profile);
        }

        profile.FirstName = request.FirstName ?? profile.FirstName;
        profile.LastName = request.LastName ?? profile.LastName;
        profile.Phone = request.Phone ?? profile.Phone;
        profile.Location = request.Location ?? profile.Location;
        profile.LinkedIn = request.LinkedIn ?? profile.LinkedIn;
        profile.GitHub = request.GitHub ?? profile.GitHub;
        profile.Portfolio = request.Portfolio ?? profile.Portfolio;
        profile.Website = request.Website ?? profile.Website;
        profile.Summary = request.Summary ?? profile.Summary;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Profile updated for user: {UserId}", userId);

        return MapToDto(profile);
    }

    private static UserProfileDto MapToDto(Profile profile)
    {
        return new UserProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Phone = profile.Phone,
            Location = profile.Location,
            LinkedIn = profile.LinkedIn,
            GitHub = profile.GitHub,
            Portfolio = profile.Portfolio,
            Website = profile.Website,
            Summary = profile.Summary,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }
}
