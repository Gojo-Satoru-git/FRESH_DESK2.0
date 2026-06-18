using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Adrenalin.Infrastructure.Authentication
{
    public class JwtProvider: IJwtProvider
    {
        private readonly JwtOptions _options;

    public JwtProvider(
        IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateToken(
        Guid userId,
        string email, IEnumerable<string> roles,
        IEnumerable<string> permissions,
        Guid sessionId,
        string? firstName = null,
        string? lastName = null
    )
    {
        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                userId.ToString()),

            new(
                JwtRegisteredClaimNames.Email,
                email),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
            
            
        };
        claims.Add(
    new Claim(
        "session_id",
        sessionId.ToString()));

        if (!string.IsNullOrWhiteSpace(firstName))
        {
            claims.Add(new(JwtRegisteredClaimNames.GivenName, firstName));
        }
        if (!string.IsNullOrWhiteSpace(lastName))
        {
            claims.Add(new(JwtRegisteredClaimNames.FamilyName, lastName));
        }
        var fullName = $"{firstName} {lastName}".Trim();
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            claims.Add(new("name", fullName));
        }

        foreach (var role in roles)
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
        }
        foreach (var permission in permissions)
        {
            claims.Add(
                new Claim(
                    "permission",
                    permission));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _options.SecretKey));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);
        Console.WriteLine($"Roles Count: {roles.Count()}");
Console.WriteLine($"Permissions Count: {permissions.Count()}");
        var token =
            new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    _options.ExpiryMinutes),
                signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
    }
}