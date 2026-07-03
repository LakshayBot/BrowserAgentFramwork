using BrowserAgent.Api.Application.DTOs.Auth;
using BrowserAgent.Api.Application.Interfaces;
using BrowserAgent.Api.Domain.Entities;
using BrowserAgent.Api.Domain.Exceptions;
using BrowserAgent.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BrowserAgent.Api.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext context,
        IJwtService jwtService,
        IPasswordHasher passwordHasher,
        ILogger<AuthService> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<TokenResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existingUser = await _context.Users
            .AsNoTracking()
            .AnyAsync(x => x.Email == request.Email.ToLower().Trim(), ct);

        if (existingUser)
        {
            throw new DomainException("EMAIL_EXISTS", "An account with this email already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLower().Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            IsActive = true,
            EmailVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        var userRole = await _context.Roles
            .AsNoTracking()
            .FirstAsync(x => x.Name == "User", ct);

        _context.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = userRole.Id
        });

        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Profiles.Add(profile);

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("User registered: {UserId} {Email}", user.Id, user.Email);

        return await GenerateTokenResponseAsync(user, ct);
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == request.Email.ToLower().Trim(), ct);

        if (user is null)
        {
            throw new UnauthorizedException("Invalid email or password");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException("Account is deactivated");
        }

        if (!_passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            throw new UnauthorizedException("Invalid email or password");
        }

        _logger.LogInformation("User logged in: {UserId} {Email}", user.Id, user.Email);

        return await GenerateTokenResponseAsync(user, ct);
    }

    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var storedToken = await _context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == refreshToken, ct);

        if (storedToken is null)
        {
            throw new UnauthorizedException("Invalid refresh token");
        }

        if (!storedToken.IsActive)
        {
            storedToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            throw new UnauthorizedException("Refresh token has expired or been revoked");
        }

        storedToken.RevokedAt = DateTime.UtcNow;

        var user = storedToken.User;

        if (!user.IsActive)
        {
            throw new UnauthorizedException("Account is deactivated");
        }

        _logger.LogInformation("Token refreshed for user: {UserId}", user.Id);

        return await GenerateTokenResponseAsync(user, ct);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == refreshToken, ct);

        if (storedToken is not null)
        {
            storedToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(x => x.Profile)
            .FirstOrDefaultAsync(x => x.Id == userId, ct);

        if (user is null)
        {
            throw new NotFoundException("User", userId);
        }

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            EmailVerified = user.EmailVerified,
            FirstName = user.Profile?.FirstName,
            LastName = user.Profile?.LastName,
            CreatedAt = user.CreatedAt
        };
    }

    private async Task<TokenResponse> GenerateTokenResponseAsync(User user, CancellationToken ct)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        _context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = _jwtService.GetRefreshTokenExpiration(),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(ct);

        var profile = await _context.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == user.Id, ct);

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                EmailVerified = user.EmailVerified,
                FirstName = profile?.FirstName,
                LastName = profile?.LastName,
                CreatedAt = user.CreatedAt
            }
        };
    }
}
