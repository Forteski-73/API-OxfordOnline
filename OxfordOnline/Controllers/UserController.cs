using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OxfordOnline.Data;
using OxfordOnline.Models;
using OxfordOnline.Models.Dto;
using OxfordOnline.Resources;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
                var dbUser = await _context.ApiUser.FirstOrDefaultAsync(u => u.User == user.User);

                // Valida usuário e senha
                if (dbUser == null || !BCrypt.Net.BCrypt.Verify(user.Password, dbUser.Password))
                {
                    return Unauthorized(new { message = EndPointsMessages.InvalidUserOrPassword });
                }

                Profile? profile = null;

                // ============================================
                // ATUALIZA PROFILE ID SE ENVIADO E VÁLIDO
                // ============================================
                if (user.ProfileId.HasValue && user.ProfileId.Value > 0)
                {
                    profile = await _context.Profile.FirstOrDefaultAsync(p => p.Id == user.ProfileId.Value);

                    if (profile == null)
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
                
                Profile? currentProfile = null;
                if (dbUser.ProfileId.HasValue)
                {
                    currentProfile = await _context.Profile.FirstOrDefaultAsync(p => p.Id == dbUser.ProfileId.Value);
                }
                bool isReadOnly = currentProfile?.IsReadOnly ?? false;

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
                            imagePath = m.ImagePath,
                            isReadOnly = isReadOnly,
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
        

        /*
        [HttpPost("LoginUser")]
        public async Task<IActionResult> LoginUser([FromBody] LoginUserRequest request)
        {
            try
            {
                var dbUser = await _context.ApiUser.FirstOrDefaultAsync(u => u.User == request.User);

                // Valida usuário e senha
                if (dbUser == null || !BCrypt.Net.BCrypt.Verify(request.Password, dbUser.Password))
                {
                    return Unauthorized(new { message = EndPointsMessages.InvalidUserOrPassword });
                }

                // ============================================
                // REGISTRA/ATUALIZA O DEVICE (best-effort, mas bloqueia se dados inválidos)
                // ============================================
                var deviceError = await UpsertDeviceAsync(dbUser.Id, request.Device);
                if (deviceError != null) return deviceError;

                Profile? profile = null;

                // ============================================
                // ATUALIZA PROFILE ID SE ENVIADO E VÁLIDO
                // ============================================
                if (request.ProfileId.HasValue && request.ProfileId.Value > 0)
                {
                    profile = await _context.Profile.FirstOrDefaultAsync(p => p.Id == request.ProfileId.Value);

                    if (profile == null)
                    {
                        return BadRequest(new { message = $"O perfil de ID {request.ProfileId.Value} não existe na tabela de perfis." });
                    }

                    if (dbUser.ProfileId != request.ProfileId.Value)
                    {
                        dbUser.ProfileId = request.ProfileId.Value;
                        await _context.SaveChangesAsync();
                    }
                }

                // ============================================
                // BUSCA MENUS LIBERADOS (Baseado no ProfileId atualizado)
                // ============================================
                Profile? currentProfile = null;
                if (dbUser.ProfileId.HasValue)
                {
                    currentProfile = await _context.Profile.FirstOrDefaultAsync(p => p.Id == dbUser.ProfileId.Value);
                }
                bool isReadOnly = currentProfile?.IsReadOnly ?? false;

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
                            imagePath = m.ImagePath,
                            isReadOnly = isReadOnly,
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
        */

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

        [HttpGet("ProfilesMenu")]
        public async Task<IActionResult> GetProfilesForMenu(
            [FromHeader(Name = "Authorization")] string authHeader)
        {
            try
            {
                // ============================================
                // VALIDAÇÃO DO TOKEN (Mantendo seu padrão)
                // ============================================
                if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(new { message = "Token ausente ou inválido." });
                }

                string token = authHeader.Substring("Bearer ".Length).Trim();
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

                // ============================================
                // BUSCA A LISTA SIMPLES DE MENUS DEFAULT
                // ============================================
                var defaultMenus = await _context.Menu
                    //.Where(m => m.IsActive) // Filtra apenas os ativos
                    .Select(m => new
                    {
                        id = m.Id,
                        title = m.Title
                    })
                    .OrderBy(m => m.title)
                    .ToListAsync();

                // ============================================
                // BUSCA OS PERFIS COM SEUS MENUS VINCULADOS
                // ============================================
                var profilesWithMenus = await _context.Profile
                    .Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        menus = _context.ProfileMenu
                            .Where(pm => pm.ProfileId == p.Id)
                            .Join(_context.Menu,
                                pm => pm.MenuId,
                                m => m.Id,
                                (pm, m) => new
                                {
                                    id = m.Id,
                                    title = m.Title
                                })
                            .ToList()
                    })
                    .OrderBy(p => p.name)
                    .ToListAsync();

                // ============================================
                // RETORNO ENVELOPADO COM OS DOIS BLOCOS
                // ============================================
                return Ok(new
                {
                    menus_default = defaultMenus,
                    profiles = profilesWithMenus
                });
            }
            catch (SecurityTokenExpiredException)
            {
                return Unauthorized(new { message = "Token expirado." });
            }
            catch (SecurityTokenException)
            {
                return Unauthorized(new { message = "Token inválido." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar perfis e menus configurados.");

                return StatusCode(500, new
                {
                    message = "Erro ao buscar perfis e menus configurados.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpPut("UpdateUserProfile")]
        public async Task<IActionResult> UpdateUserProfile(
                    [FromHeader(Name = "Authorization")] string authHeader,
                    [FromBody] ApiUser user)
        {
            try
            {
                // ============================================
                // VALIDAÇÃO DO TOKEN JWT
                // ============================================
                if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(new { message = "Token ausente ou inválido." });
                }

                string token = authHeader.Substring("Bearer ".Length).Trim();
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

                // ============================================
                // VALIDAÇÃO DOS DADOS ENVIADOS
                // ============================================
                if (string.IsNullOrWhiteSpace(user.User))
                {
                    return BadRequest(new { message = "O nome de usuário (User) é obrigatório." });
                }

                if (!user.ProfileId.HasValue || user.ProfileId.Value <= 0)
                {
                    return BadRequest(new { message = "O ProfileId é obrigatório e deve ser um ID válido (maior que zero)." });
                }

                // ============================================
                // BUSCA O USUÁRIO NO BANCO DE DADOS
                // ============================================
                var dbUser = await _context.ApiUser.FirstOrDefaultAsync(u => u.User == user.User);

                if (dbUser == null)
                {
                    return NotFound(new { message = "Usuário não encontrado." });
                }

                // ============================================
                // ATUALIZA PROFILE ID SE ENVIADO E VÁLIDO
                // ============================================
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

                // ============================================
                // RETORNO
                // ============================================
                return Ok(new
                {
                    message = "Perfil do usuário atualizado com sucesso!",
                    user = dbUser.User,
                    newProfileId = dbUser.ProfileId
                });
            }
            catch (SecurityTokenExpiredException)
            {
                return Unauthorized(new { message = "Token expirado." });
            }
            catch (SecurityTokenException)
            {
                return Unauthorized(new { message = "Token inválido." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar o perfil do usuário.");

                return StatusCode(500, new
                {
                    message = "Erro ao atualizar o perfil do usuário.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpPost("UpdateProfileMenus")]
        public async Task<IActionResult> UpdateProfileMenus(
                    [FromHeader(Name = "Authorization")] string authHeader,
                    [FromBody] ProfileMenuUpdateRequest request)
        {
            try
            {
                // ============================================
                // VALIDAÇÃO DO TOKEN
                // ============================================
                if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(new { message = "Token ausente ou inválido." });
                }

                string token = authHeader.Substring("Bearer ".Length).Trim();
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

                // ============================================
                // VALIDA SE O PROFILE EXISTE
                // ============================================
                var profileExists = await _context.Profile.AnyAsync(p => p.Id == request.Id);
                if (!profileExists)
                {
                    return BadRequest(new { message = $"O perfil de ID {request.Id} não existe." });
                }

                // ============================================
                // ATUALIZAÇÃO DA TABELA RELATION (profile_menus)
                // ============================================

                // 1. Busca e remove todos os vínculos atuais desse perfil
                var existingRelations = await _context.ProfileMenu
                    .Where(pm => pm.ProfileId == request.Id)
                    .ToListAsync();

                if (existingRelations.Any())
                {
                    _context.ProfileMenu.RemoveRange(existingRelations);
                }

                // 2. Adiciona os novos vínculos passados no array do JSON
                if (request.Menus != null && request.Menus.Any())
                {
                    var newRelations = request.Menus.Select(m => new ProfileMenu
                    {
                        ProfileId = request.Id,
                        MenuId = m.Id
                    }).ToList();

                    _context.ProfileMenu.AddRange(newRelations);
                }

                // 3. Salva as alterações no banco de dados
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Menus do perfil atualizados com sucesso!",
                    profileId = request.Id
                });
            }
            catch (SecurityTokenExpiredException)
            {
                return Unauthorized(new { message = "Token expirado." });
            }
            catch (SecurityTokenException)
            {
                return Unauthorized(new { message = "Token inválido." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar menus do perfil.");

                return StatusCode(500, new
                {
                    message = "Erro ao atualizar menus do perfil.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        private static readonly HashSet<string> ValidPlatforms =
            new(StringComparer.OrdinalIgnoreCase) { "ios", "android", "web" };

        /// <summary>
        /// Cria ou atualiza o registro de device vinculado ao usuário autenticado.
        /// Usa INSERT ... ON DUPLICATE KEY UPDATE para operação atômica de
        /// upsert em uma única ida ao banco, aproveitando o UNIQUE INDEX
        /// (guid, user_id) da tabela `device`.
        /// </summary>
        private async Task<IActionResult?> UpsertDeviceAsync(int userId, DeviceInfo? device)
        {
            if (device == null)
                return BadRequest(new { message = "Dados do dispositivo (device) são obrigatórios." });

            if (string.IsNullOrWhiteSpace(device.Guid))
                return BadRequest(new { message = "device.guid é obrigatório para registrar o dispositivo." });

            if (string.IsNullOrWhiteSpace(device.Platform) || !ValidPlatforms.Contains(device.Platform))
                return BadRequest(new { message = "device.platform deve ser 'ios', 'android' ou 'web'." });

            var platform = device.Platform.ToLowerInvariant();

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO device (guid, user_id, device_name, platform, app_version, first_login_at, last_seen_at, is_active)
                VALUES ({device.Guid}, {userId}, {device.DeviceName}, {platform}, {device.AppVersion}, NOW(), NOW(), 1)
                ON DUPLICATE KEY UPDATE
                    last_seen_at = NOW(),
                    is_active    = 1,
                    device_name  = VALUES(device_name),
                    platform     = VALUES(platform),
                    app_version  = VALUES(app_version);
            ");

            return null;
        }

        [HttpPost("Device")]
        public async Task<IActionResult> RegisterDevice(
            [FromHeader(Name = "Authorization")] string authHeader,
            [FromBody] DeviceInfo device)
        {
            try
            {
                // ============================================
                // VALIDAÇÃO DO TOKEN JWT
                // ============================================
                if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
                    return Unauthorized(new { message = EndPointsMessages.TokenMissingOrInvalid });

                string token = authHeader.Substring("Bearer ".Length).Trim();
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

                ClaimsPrincipal principal;
                try
                {
                    principal = tokenHandler.ValidateToken(
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
                        out _
                    );
                }
                catch (SecurityTokenExpiredException)
                {
                    return Unauthorized(new { message = "Token expirado." });
                }
                catch (SecurityTokenException)
                {
                    return Unauthorized(new { message = "Token inválido." });
                }

                var username = principal.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrWhiteSpace(username))
                    return Unauthorized(new { message = "Token não contém identificação do usuário." });

                var dbUser = await _context.ApiUser.FirstOrDefaultAsync(u => u.User == username);
                if (dbUser == null)
                    return Unauthorized(new { message = "Usuário do token não encontrado." });

                // ============================================
                // UPSERT DO DEVICE (mesma lógica usada no login)
                // ============================================
                var deviceError = await UpsertDeviceAsync(dbUser.Id, device);
                if (deviceError != null) return deviceError;

                return Ok(new { message = "Dispositivo registrado com sucesso." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar dispositivo.");
                return StatusCode(500, new
                {
                    message = "Erro ao registrar dispositivo.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }



        [HttpGet("api")]
        public IActionResult CheckStatus()
        {
            return Ok(new { status = "Online", timestamp = DateTime.UtcNow });
        }
    }
}
