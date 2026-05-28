using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OxfordOnline.Data;
using OxfordOnline.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using OxfordOnline.Resources;
using Microsoft.Extensions.Logging;
using System.Text;

namespace OxfordOnline.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<UserController> _logger;

        public UserController(AppDbContext context, IConfiguration config, ILogger<UserController> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromHeader(Name = "Authorization")] string authHeader,
            [FromBody] ApiUser user)
        {
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized(new { message = EndPointsMessages.TokenMissingOrInvalid });

            string token = authHeader.Substring("Bearer ".Length).Trim();

            if (token != _config["AuthToken"])
                return Unauthorized(new { message = EndPointsMessages.TokenInvalid });

            if (string.IsNullOrWhiteSpace(user.User) || string.IsNullOrWhiteSpace(user.Password))
                return BadRequest(new { message = EndPointsMessages.UserAndPasswordRequired });

            if (string.IsNullOrWhiteSpace(user.Account))
            {
                return BadRequest(new { message = EndPointsMessages.EmailRequired });
            }

            var existingUser = await _context.ApiUser.FirstOrDefaultAsync(u => u.Account == user.Account);

            try
            {
                string hash = BCrypt.Net.BCrypt.HashPassword(user.Password);

                if (existingUser != null)
                {
                    existingUser.User = user.User;
                    existingUser.Password = hash;
                }
                else
                {
                    _context.ApiUser.Add(new ApiUser
                    {
                        User = user.User,
                        Password = hash,
                        Account = user.Account
                    });
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = EndPointsMessages.UserRegisteredSuccess });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, EndPointsMessages.LogErrorRegisterUser);
                return StatusCode(500, new
                {
                    message = EndPointsMessages.LogErrorRegisterUser,
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ApiUser user)
        {
            try
            {
                var dbUser = await _context.ApiUser.FirstOrDefaultAsync(u => u.User == user.User);
                if (dbUser == null || !BCrypt.Net.BCrypt.Verify(user.Password, dbUser.Password))
                    return Unauthorized(new { message = EndPointsMessages.InvalidUserOrPassword });

                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, dbUser.User)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(24), // TOKEN expira em 24 horas
                    signingCredentials: creds
                );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, EndPointsMessages.LogErrorLoginUser);
                return StatusCode(500, new
                {
                    message = EndPointsMessages.LogErrorLoginUser,
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpPost("LoginUser")]
        public async Task<IActionResult> LoginUser([FromBody] ApiUser user)
        {
            try
            {
                var dbUser = await _context.ApiUser
                    .FirstOrDefaultAsync(u => u.User == user.User);

                // Valida usuário e senha
                if (dbUser == null || !BCrypt.Net.BCrypt.Verify(user.Password, dbUser.Password))
                {
                    return Unauthorized(new { message = EndPointsMessages.InvalidUserOrPassword });
                }

                // ============================================
                // ATUALIZA PROFILE ID SE ENVIADO E VÁLIDO
                // ============================================
                if (user.ProfileId.HasValue && user.ProfileId.Value > 0)
                {
                    bool profileExists = await _context.Profile
                        .AnyAsync(p => p.Id == user.ProfileId.Value);

                    if (!profileExists)
                    {
                        return BadRequest(new { message = $"O perfil de ID {user.ProfileId.Value} não existe na tabela de perfis." });
                    }
                    if (dbUser.ProfileId != user.ProfileId.Value)
                    {
                        dbUser.ProfileId = user.ProfileId.Value;
                        await _context.SaveChangesAsync();
                    }
                }

                // ============================================
                // BUSCA MENUS LIBERADOS (Baseado no ProfileId atualizado)
                // ============================================
                object allowedMenus = Array.Empty<object>();

                if (dbUser.ProfileId.HasValue)
                {
                    allowedMenus = await (
                        from pm in _context.ProfileMenu
                        join m in _context.Menu on pm.MenuId equals m.Id
                        where pm.ProfileId == dbUser.ProfileId.Value && m.IsActive
                        select new
                        {
                            title = m.Title,
                            routeName = m.RouteName,
                            imagePath = m.ImagePath
                        }
                    ).ToListAsync();
                }

                // ============================================
                // CONFIGURA CLAIMS JWT
                // ============================================
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, dbUser.User)
                };

                if (dbUser.ProfileId.HasValue)
                {
                    claims.Add(new Claim("profileId", dbUser.ProfileId.Value.ToString()));
                }

                // ============================================
                // GERA TOKEN JWT
                // ============================================
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(24),
                    signingCredentials: creds
                );

                // ============================================
                // RETORNO
                // ============================================
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    profileId = dbUser.ProfileId,
                    menus = allowedMenus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, EndPointsMessages.LogErrorLoginUser);
                return StatusCode(500, new
                {
                    message = EndPointsMessages.LogErrorLoginUser,
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpGet("Users")]
        public async Task<IActionResult> GetUsers([FromHeader(Name = "Authorization")] string authHeader)
        {
            try
            {
                // Valida Bearer Token
                if (string.IsNullOrWhiteSpace(authHeader) ||
                    !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(new
                    {
                        message = EndPointsMessages.TokenMissingOrInvalid
                    });
                }

                string token = authHeader.Substring("Bearer ".Length).Trim();

                // Valida JWT
                var tokenHandler = new JwtSecurityTokenHandler();

                var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

                tokenHandler.ValidateToken(
                    token,
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = _config["Jwt:Issuer"],
                        ValidAudience = _config["Jwt:Audience"],

                        IssuerSigningKey = new SymmetricSecurityKey(key),

                        ClockSkew = TimeSpan.Zero
                    },
                    out SecurityToken validatedToken
                );

                // Busca usuários
                var users = await (
                    from u in _context.ApiUser

                    join p in _context.Profile
                        on u.ProfileId equals p.Id into profileJoin

                    from p in profileJoin.DefaultIfEmpty()

                    select new
                    {
                        id = u.Id,
                        user = u.User,
                        account = u.Account,
                        profileName = p != null
                            ? p.Name
                            : null
                    }
                ).ToListAsync();

                return Ok(users);
            }
            catch (SecurityTokenExpiredException)
            {
                return Unauthorized(new
                {
                    message = "Token expirado."
                });
            }
            catch (SecurityTokenException)
            {
                return Unauthorized(new
                {
                    message = "Token inválido."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar usuários.");

                return StatusCode(500, new
                {
                    message = "Erro ao buscar usuários.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpGet("Profiles")]
        public async Task<IActionResult> GetProfiles(
            [FromHeader(Name = "Authorization")] string authHeader)
        {
            try
            {
                // Valida Bearer Token
                if (string.IsNullOrWhiteSpace(authHeader) ||
                    !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(new
                    {
                        message = EndPointsMessages.TokenMissingOrInvalid
                    });
                }

                string token = authHeader.Substring("Bearer ".Length).Trim();

                // Valida JWT
                var tokenHandler = new JwtSecurityTokenHandler();

                var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

                tokenHandler.ValidateToken(
                    token,
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = _config["Jwt:Issuer"],
                        ValidAudience = _config["Jwt:Audience"],

                        IssuerSigningKey = new SymmetricSecurityKey(key),

                        ClockSkew = TimeSpan.Zero
                    },
                    out SecurityToken validatedToken
                );

                // Busca todos os perfis
                var profiles = await _context.Profile
                    .Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        description = p.Description,
                        createdAt = p.CreatedAt
                    })
                    .OrderBy(p => p.name)
                    .ToListAsync();

                return Ok(profiles);
            }
            catch (SecurityTokenExpiredException)
            {
                return Unauthorized(new
                {
                    message = "Token expirado."
                });
            }
            catch (SecurityTokenException)
            {
                return Unauthorized(new
                {
                    message = "Token inválido."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar perfis.");

                return StatusCode(500, new
                {
                    message = "Erro ao buscar perfis.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

    }
}
