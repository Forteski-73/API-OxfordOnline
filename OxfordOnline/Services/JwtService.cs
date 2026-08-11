using Microsoft.IdentityModel.Tokens;
using OxfordOnline.Models.Dto;
using OxfordOnline.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OxfordOnline.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly TimeSpan _tokenLifetime;

        public JwtService(IConfiguration config)
        {
            _config = config;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            _issuer = _config["Jwt:Issuer"]!;
            _audience = _config["Jwt:Audience"]!;
            _tokenLifetime = TimeSpan.FromHours(24); // mesmo padrão usado hoje no UserController
        }

        public string GenerateToken(string username, int? profileId = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username)
            };

            if (profileId.HasValue)
            {
                claims.Add(new Claim("profileId", profileId.Value.ToString()));
            }

            return GenerateToken(claims);
        }

        public string GenerateToken(SeniorEmployeeData employee)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, employee.Name),
                new Claim("registration", employee.Registration.ToString()),
                new Claim("position", employee.Position ?? string.Empty),
                new Claim("authProvider", "senior")
            };

            return GenerateToken(claims);
        }

        public string GenerateToken(IEnumerable<Claim> claims)
        {
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.Add(_tokenLifetime),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = _issuer,
                    ValidAudience = _audience,

                    IssuerSigningKey = _key,

                    ClockSkew = TimeSpan.Zero
                },
                out _
            );

            return principal;
        }
    }
}