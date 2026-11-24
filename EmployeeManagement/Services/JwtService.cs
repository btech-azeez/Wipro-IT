using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EmployeeManagement.Models;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagement.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal? ValidateToken(string token);
    }

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        public JwtService(IConfiguration configuration) => _configuration = configuration;

        public string GenerateToken(User user)
        {
            var key         = GetKey();
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name,            user.Username),
                new Claim(ClaimTypes.Email,           user.Email),
                new Claim(ClaimTypes.Role,            user.Role),
                new Claim("FullName",                 user.FullName),
                new Claim("Department",               user.Department ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer:             _configuration["Jwt:Issuer"] ?? "WiproEMS",
                audience:           _configuration["Jwt:Audience"] ?? "WiproEMS",
                claims:             claims,
                expires:            DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                return new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = GetKey(),
                    ValidateIssuer           = true,
                    ValidIssuer              = _configuration["Jwt:Issuer"] ?? "WiproEMS",
                    ValidateAudience         = true,
                    ValidAudience            = _configuration["Jwt:Audience"] ?? "WiproEMS",
                    ValidateLifetime         = true,
                    ClockSkew                = TimeSpan.Zero
                }, out _);
            }
            catch { return null; }
        }

        private SymmetricSecurityKey GetKey() =>
            new(Encoding.UTF8.GetBytes(
                _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.")));
    }
}
