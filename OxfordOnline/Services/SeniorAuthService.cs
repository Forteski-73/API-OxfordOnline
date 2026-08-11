using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OxfordOnline.Data;
using OxfordOnline.Models;
using OxfordOnline.Models.Dto;
using OxfordOnline.Repositories.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace OxfordOnline.Services
{
    public class SeniorAuthService : ISeniorAuthService
    {
        private const string TokenCacheKey = "SeniorAuthToken";

        private readonly HttpClient _httpClient;
        private readonly SeniorSettings _settings;
        private readonly ILogger<SeniorAuthService> _logger;
        private readonly IMemoryCache _cache;
        private readonly AppDbContext _context;

        public SeniorAuthService(
            HttpClient httpClient,
            IOptions<SeniorSettings> options,
            ILogger<SeniorAuthService> logger,
            IMemoryCache cache,
            AppDbContext context)
        {
            _settings = options.Value;
            _logger = logger;
            _cache = cache;
            _context = context;

            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        }

        public async Task<string> GetSeniorTokenAsync()
        {
            if (_cache.TryGetValue(TokenCacheKey, out string? cachedToken) && !string.IsNullOrEmpty(cachedToken))
            {
                return cachedToken!;
            }

            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "/platform/authentication/anonymous/loginWithKey");

                request.Headers.Add("client_id", _settings.ClientId);

                var body = new SeniorLoginRequest
                {
                    AccessKey = _settings.AccessKey,
                    Secret = _settings.Secret,
                    TenantName = _settings.TenantName
                };

                var json = System.Text.Json.JsonSerializer.Serialize(body);
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                _logger.LogInformation("[SeniorAuthService] Autenticando na API do Sênior...");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "[SeniorAuthService] Erro ao autenticar. Status: {StatusCode}. Corpo: {Body}",
                        response.StatusCode, errorBody);
                }

                response.EnsureSuccessStatusCode();

                var rawBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("[SeniorAuthService] Resposta bruta do login: {Body}", rawBody);

                var envelope = System.Text.Json.JsonSerializer.Deserialize<SeniorLoginEnvelope>(rawBody);

                if (envelope is null || string.IsNullOrEmpty(envelope.JsonToken))
                {
                    throw new InvalidOperationException("Envelope de token do Sênior não retornado.");
                }

                var result = System.Text.Json.JsonSerializer.Deserialize<SeniorLoginResponse>(envelope.JsonToken);

                if (result is null || string.IsNullOrEmpty(result.AccessToken))
                {
                    throw new InvalidOperationException("Token do Sênior não retornado.");
                }

                var expiration = result.ExpiresIn > 60
                    ? TimeSpan.FromSeconds(result.ExpiresIn - 60)
                    : TimeSpan.FromMinutes(5);

                _cache.Set(TokenCacheKey, result.AccessToken, expiration);

                _logger.LogInformation("[SeniorAuthService] Token do Sênior obtido e cacheado com sucesso.");

                return result.AccessToken;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "[SeniorAuthService] Erro ao autenticar na API do Sênior.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SeniorAuthService] Erro inesperado ao obter token do Sênior.");
                throw;
            }
        }

        public async Task<SeniorEmployeeData?> ValidateBadgeAsync(string badgeCode)
        {
            if (string.IsNullOrWhiteSpace(badgeCode))
                throw new ArgumentException("Código de crachá inválido.", nameof(badgeCode));

            try
            {
                var token = await GetSeniorTokenAsync();

                var url = $"/hcm/employeejourney/entities/employee?filter=registerNumber%20eq%20{badgeCode}";

                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    url);

                request.Headers.Add("client_id", _settings.ClientId);
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                _logger.LogInformation(
                    "[SeniorAuthService] Validando crachá: {BadgeCode}",
                    badgeCode);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();

                    _logger.LogWarning(
                        "[SeniorAuthService] Erro ao validar crachá {BadgeCode}. Status: {StatusCode}. Resposta: {Response}",
                        badgeCode,
                        response.StatusCode,
                        errorBody);

                    return null;
                }

                var result =
                    await response.Content.ReadFromJsonAsync<SeniorEmployeeResponse>();

                var employee = result?.Contents?.FirstOrDefault();

                if (employee is not null && !string.IsNullOrWhiteSpace(employee.Email))
                {
                    var saveResult = await SaveUserAccountAsync(employee);

                    if (!saveResult.Success)
                    {
                        _logger.LogWarning(
                            "[SeniorAuthService] Falha ao persistir UserAccount para {Email}: {Message}",
                            employee.Email, saveResult.Message);

                        return null;
                    }
                }

                return employee;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[SeniorAuthService] Erro ao validar crachá: {BadgeCode}",
                    badgeCode);

                throw;
            }
        }

        /// <summary>
        /// Grava ou atualiza o funcionário na tabela UserAccount após validação bem-sucedida no Sênior.
        /// Retorna um UserAccountSaveResult indicando sucesso ou falha.
        /// </summary>
        private async Task<UserAccountSaveResult> SaveUserAccountAsync(SeniorEmployeeData employee)
        {
            try
            {
                // Normaliza o domínio: oxfordporcelanas.com.br é tratado como grupooxford.com.br,
                // já que ambos representam o mesmo funcionário.
                var normalizedEmail = employee.Email.Replace(
                    "@oxfordporcelanas.com.br",
                    "@grupooxford.com.br",
                    StringComparison.OrdinalIgnoreCase);

                var userAccount = await _context.UserAccount
                    .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

                bool isNew = userAccount is null;

                if (isNew)
                {
                    userAccount = new UserAccount
                    {
                        Email = normalizedEmail
                    };
                    _context.UserAccount.Add(userAccount);
                }

                // Campos não-chave: sempre atualizados, seja inclusão ou alteração
                userAccount!.Name       = employee.Name;
                userAccount.Position    = employee.Position;
                userAccount.Department  = employee.Department?.Name;
                userAccount.Group       = employee.CostCenter?.Name;
                userAccount.WorkShift   = employee.WorkShift?.Name;
                userAccount.Phone       = employee.Phone;
                userAccount.IdBadge     = employee.Registration.ToString();
                userAccount.Branch      = employee.Headquarter?.CompanyName;
                userAccount.AddLocation = employee.CostCenter?.Name;

                await _context.SaveChangesAsync();

                return UserAccountSaveResult.Ok();
            }
            catch (Exception ex)
            {
                return UserAccountSaveResult.Failed(ex.InnerException?.Message ?? ex.Message);
            }
        }


    }
}

/// <summary>
/// Resultado simples da tentativa de gravar/atualizar um UserAccount.
/// </summary>
public class UserAccountSaveResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    public static UserAccountSaveResult Ok() => new()
    {
        Success = true
    };

    public static UserAccountSaveResult Failed(string message) => new()
    {
        Success = false,
        Message = message
    };
}


/*
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OxfordOnline.Models.Dto;
using OxfordOnline.Repositories.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace OxfordOnline.Services
{
    public class SeniorAuthService : ISeniorAuthService
    {
        private const string TokenCacheKey = "SeniorAuthToken";

        private readonly HttpClient _httpClient;
        private readonly SeniorSettings _settings;
        private readonly ILogger<SeniorAuthService> _logger;
        private readonly IMemoryCache _cache;

        public SeniorAuthService(
            HttpClient httpClient,
            IOptions<SeniorSettings> options,
            ILogger<SeniorAuthService> logger,
            IMemoryCache cache)
        {
            _settings = options.Value;
            _logger = logger;
            _cache = cache;

            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        }

        public async Task<string> GetSeniorTokenAsync()
        {
            if (_cache.TryGetValue(TokenCacheKey, out string? cachedToken) && !string.IsNullOrEmpty(cachedToken))
            {
                return cachedToken!;
            }

            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "/platform/authentication/anonymous/loginWithKey");

                request.Headers.Add("client_id", _settings.ClientId);

                var body = new SeniorLoginRequest
                {
                    AccessKey = _settings.AccessKey,
                    Secret = _settings.Secret,
                    TenantName = _settings.TenantName
                };

                var json = System.Text.Json.JsonSerializer.Serialize(body);
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                _logger.LogInformation("[SeniorAuthService] Autenticando na API do Sênior...");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "[SeniorAuthService] Erro ao autenticar. Status: {StatusCode}. Corpo: {Body}",
                        response.StatusCode, errorBody);
                }

                response.EnsureSuccessStatusCode();

                var rawBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("[SeniorAuthService] Resposta bruta do login: {Body}", rawBody);

                var envelope = System.Text.Json.JsonSerializer.Deserialize<SeniorLoginEnvelope>(rawBody);

                if (envelope is null || string.IsNullOrEmpty(envelope.JsonToken))
                {
                    throw new InvalidOperationException("Envelope de token do Sênior não retornado.");
                }

                var result = System.Text.Json.JsonSerializer.Deserialize<SeniorLoginResponse>(envelope.JsonToken);

                if (result is null || string.IsNullOrEmpty(result.AccessToken))
                {
                    throw new InvalidOperationException("Token do Sênior não retornado.");
                }

                var expiration = result.ExpiresIn > 60
                    ? TimeSpan.FromSeconds(result.ExpiresIn - 60)
                    : TimeSpan.FromMinutes(5);

                _cache.Set(TokenCacheKey, result.AccessToken, expiration);

                _logger.LogInformation("[SeniorAuthService] Token do Sênior obtido e cacheado com sucesso.");

                return result.AccessToken;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "[SeniorAuthService] Erro ao autenticar na API do Sênior.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SeniorAuthService] Erro inesperado ao obter token do Sênior.");
                throw;
            }
        }

        public async Task<SeniorEmployeeData?> ValidateBadgeAsync(string badgeCode)
        {
            if (string.IsNullOrWhiteSpace(badgeCode))
                throw new ArgumentException("Código de crachá inválido.", nameof(badgeCode));

            try
            {
                var token = await GetSeniorTokenAsync();

                var url = $"/hcm/employeejourney/entities/employee?filter=registerNumber%20eq%20{badgeCode}";

                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    url);

                request.Headers.Add("client_id", _settings.ClientId);
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                _logger.LogInformation(
                    "[SeniorAuthService] Validando crachá: {BadgeCode}",
                    badgeCode);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();

                    _logger.LogWarning(
                        "[SeniorAuthService] Erro ao validar crachá {BadgeCode}. Status: {StatusCode}. Resposta: {Response}",
                        badgeCode,
                        response.StatusCode,
                        errorBody);

                    return null;
                }

                var result =
                    await response.Content.ReadFromJsonAsync<SeniorEmployeeResponse>();

                return result?.Contents?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[SeniorAuthService] Erro ao validar crachá: {BadgeCode}",
                    badgeCode);

                throw;
            }
        }



    }
}
*/