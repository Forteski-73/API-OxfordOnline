using System.Security.Claims;
using OxfordOnline.Models.Dto;

namespace OxfordOnline.Services.Interfaces
{
    /// <summary>
    /// Serviço responsável pela geração e validação de tokens JWT da aplicação.
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Gera um token JWT para um usuário local (tabela ApiUser).
        /// Inclui a claim "profileId" quando informado.
        /// </summary>
        string GenerateToken(string username, int? profileId = null);

        /// <summary>
        /// Gera um token JWT para um funcionário autenticado via crachá (API do Sênior).
        /// </summary>
        string GenerateToken(SeniorEmployeeData employee);

        /// <summary>
        /// Gera um token JWT a partir de uma lista de claims customizada,
        /// para casos não cobertos pelos overloads acima.
        /// </summary>
        string GenerateToken(IEnumerable<Claim> claims);

        /// <summary>
        /// Valida um token JWT (assinatura, issuer, audience e expiração) e
        /// retorna o ClaimsPrincipal correspondente.
        /// Lança SecurityTokenExpiredException ou SecurityTokenException em caso de falha,
        /// para os controllers tratarem como já fazem hoje.
        /// </summary>
        ClaimsPrincipal ValidateToken(string token);
    }
}