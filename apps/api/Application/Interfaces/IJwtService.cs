using System.Security.Claims;
using BrowserAgent.Api.Domain.Entities;

namespace BrowserAgent.Api.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    DateTime GetRefreshTokenExpiration();
}
