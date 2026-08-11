using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OxfordOnline.Models.Dto;
using OxfordOnline.Repositories.Interfaces;
using OxfordOnline.Resources;
using OxfordOnline.Services.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;

namespace OxfordOnline.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class AuthSeniorController : ControllerBase
    {
        private readonly ISeniorAuthService _seniorAuthService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthSeniorController> _logger;

        public AuthSeniorController(
            ISeniorAuthService seniorAuthService,
            IJwtService jwtService,
            ILogger<AuthSeniorController> logger)
        {
            _seniorAuthService = seniorAuthService;
            _jwtService = jwtService;
            _logger = logger;
        }

        /// <summary>
        /// Realiza o login do funcionário via crachá, validando os dados na API do Sênior
        /// e retornando um token JWT próprio da aplicação.
        /// </summary>
        [HttpPost("Login")]
        [ProducesResponseType(typeof(LoginResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.BadgeCode))
            {
                return BadRequest(new { message = "Código de crachá não informado." });
            }

            try
            {
                var employee = await _seniorAuthService.ValidateBadgeAsync(request.BadgeCode);

                if (employee is null)
                {
                    _logger.LogWarning($"[AuthSeniorController] Crachá não encontrado: {request.BadgeCode}");
                    return Unauthorized(new { message = "Crachá inválido." });
                }

                if (string.IsNullOrWhiteSpace(employee.Email))
                {
                    _logger.LogWarning(
                        "[AuthSeniorController] Funcionário sem E-Mail: {Registration}",
                        employee.Registration);

                    return Unauthorized(new
                    {
                        message = "Funcionário sem E-Mail cadastrado."
                    });
                }

                var token = _jwtService.GenerateToken(employee);

                var response = new LoginResponse
                {
                    Token           = token,
                    Name            = employee.Name,
                    Registration    = employee.Registration.ToString(),
                    Email           = employee.Email,
                    Position        = employee.Position,
                    CompanyName     = employee.Headquarter?.CompanyName,
                    Department      = employee.CostCenter?.Name,
                    WorkShift       = employee.WorkShift?.Name,
                    PhoneContact    = employee.Phone
                };

                _logger.LogInformation($"[AuthSeniorController] Login realizado com sucesso: {employee.Registration}");

                return Ok(response);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "[AuthSeniorController] Erro de comunicação com a API do Sênior.");
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = "Erro ao comunicar com o serviço de autenticação." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthSeniorController] Erro inesperado durante o login.");
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = "Erro interno ao processar o login." });
            }
        }
    }
}