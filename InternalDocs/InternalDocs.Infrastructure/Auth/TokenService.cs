using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InternalDocs.Application.Abstractions.Services;
using InternalDocs.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace InternalDocs.Infrastructure.Auth;

public sealed class TokenService(IConfiguration configuration) : ITokenService
{
    public string GenerateJwt(User user)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var secret = jwtSettings["Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
        var issuer = jwtSettings["Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
        var audience = jwtSettings["Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
        var expiryMinutes = int.TryParse(jwtSettings["ExpiryMinutes"], out var parsed) ? parsed : 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
