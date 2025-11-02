using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventManagement.Application.Configurations;
using EventManagement.Application.Interfaces;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EventManagement.Infrastructure.Services;

public class JwtTokenHelper : IJwtTokenHelper
{
    private readonly SecurityConfiguration _securityConfiguration;

    public JwtTokenHelper(IConfiguration configuration)
    {
        _securityConfiguration = configuration.GetSection("Security").Get<SecurityConfiguration>()!;
    }

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityConfiguration.JwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var token = new JwtSecurityToken(
            issuer: _securityConfiguration.JwtIssuer,
            audience: _securityConfiguration.JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_securityConfiguration.JwtExpiryInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
