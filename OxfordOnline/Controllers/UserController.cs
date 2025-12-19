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

            bool emailExists = await _context.UserAccount.AnyAsync(ua => ua.Email == user.Account);
            if (!emailExists)
            {
                return BadRequest(new { message = EndPointsMessages.EmailRequired });
            }

            bool exists = await _context.ApiUser.AnyAsync(u => u.User == user.User);
            if (exists)
                return Conflict(new { message = EndPointsMessages.UserAlreadyRegistered });

            try
            {
                string hash = BCrypt.Net.BCrypt.HashPassword(user.Password);

                _context.ApiUser.Add(new ApiUser
                {
                    User = user.User,
                    Password = hash,
                    Account = user.Account
                });

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
    }
}

/*
[HttpPost("register")]
public async Task<IActionResult> Register([FromHeader] string token, [FromBody] ApiUser user)
{
    if (token != _config["AuthToken"])
        return Unauthorized(new { message = "Token inválido." });

    if (string.IsNullOrWhiteSpace(user.User) || string.IsNullOrWhiteSpace(user.Password))
        return BadRequest(new { message = "Usuário e senha obrigatórios." });

    bool exists = await _context.ApiUser.AnyAsync(u => u.User == user.User);
    if (exists)
        return Conflict(new { message = "Usuário já cadastrado." });

    string hash = BCrypt.Net.BCrypt.HashPassword(user.Password);

    _context.ApiUser.Add(new ApiUser
    {
        User = user.User,
        Password = hash
    });

    await _context.SaveChangesAsync();
    return Ok(new { message = "Usuário cadastrado com sucesso." });
}
*/
