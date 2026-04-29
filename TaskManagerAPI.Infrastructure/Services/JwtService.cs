using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskManagerAPI.Core.Entities;
using TaskManagerAPI.Core.Interfaces;

namespace TaskManagerAPI.Infrastructure.Services;

/// <summary>
/// Generates signed JWT tokens. The secret, issuer, audience, and expiry
/// all come from IConfiguration so they can be changed per-environment via
/// environment variables without redeploying.
/// </summary>
public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config) => _config = config;

    public string GenerateToken(ApplicationUser user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name,               user.UserName),
            new Claim(ClaimTypes.Role,               user.Role),
            // Custom claim so controllers can read userId without re-querying
            new Claim("uid", user.Id)
        };

        var token = new JwtSecurityToken(
            issuer:             jwtSettings["Issuer"],
            audience:           jwtSettings["Audience"],
            claims:             claims,
            expires:            GetExpiry(),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public DateTime GetExpiry()
    {
        var minutes = int.Parse(_config["JwtSettings:ExpiryMinutes"] ?? "60");
        return DateTime.UtcNow.AddMinutes(minutes);
    }
}
