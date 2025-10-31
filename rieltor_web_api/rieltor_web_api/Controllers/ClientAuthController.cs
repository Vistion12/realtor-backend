using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using rieltor_web_api.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace rieltor_web_api.Controllers
{
    [ApiController]
    [Route("api/client/auth")]
    [AllowAnonymous]
    public class ClientAuthController : ControllerBase
    {
        private readonly IClientAccountService _clientAccountService;
        private readonly IConfiguration _configuration;

        public ClientAuthController(IClientAccountService clientAccountService, IConfiguration configuration)
        {
            _clientAccountService = clientAccountService;
            _configuration = configuration;
        }

        [HttpPost("login")]        
        public async Task<ActionResult<ClientAuthResponse>> Login([FromBody] ClientAuthRequest request)
        {
            try
            {
                // Находим клиента по логину (email)
                var client = await _clientAccountService.GetClientByAccountLogin(request.Login);
                if (client == null)
                    return Unauthorized("Неверный логин или пароль");

                if (!client.IsAccountActive)
                    return Unauthorized("Аккаунт не активирован");

                //  Теперь сравниваем с незахешированным паролем
                if (request.Password != client.TemporaryPassword)
                    return Unauthorized("Неверный логин или пароль");

                // Генерируем JWT токен для клиента
                var token = GenerateClientJwtToken(client);

                var response = new ClientAuthResponse(
                    Token: token,
                    ClientName: client.Name,
                    Login: client.AccountLogin!,
                    Expires: DateTime.UtcNow.AddHours(24) // Токен на 24 часа
                );

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка авторизации: {ex.Message}");
            }
        }

        [HttpPost("activate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ActivateClientAccount(
    [FromBody] ActivateClientAccountRequest request,
    [FromQuery] Guid clientId)
        {
            try
            {
                var (success, error) = await _clientAccountService.ActivateClientAccount(clientId, request.TemporaryPassword);

                if (!success)
                    return BadRequest(error);

                return Ok(new { message = "Личный кабинет клиента активирован" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка активации: {ex.Message}");
            }
        }


        [HttpPost("change-password")]
        [Authorize(Roles = "Client")]
        public async Task<ActionResult> ChangePassword([FromBody] ChangeClientPasswordRequest request)
        {
            try
            {
                var clientId = GetClientIdFromToken();
                if (clientId == Guid.Empty)
                    return Unauthorized("Неверный токен");

                var (success, error) = await _clientAccountService.ChangeClientPassword(clientId, request.NewPassword);

                if (!success)
                    return BadRequest(error);

                return Ok(new { message = "Пароль успешно изменен" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка смены пароля: {ex.Message}");
            }
        }

        [HttpPost("consent")]
        [Authorize(Roles = "Client")]
        public async Task<ActionResult> GiveConsent([FromBody] ClientConsentRequest request)
        {
            try
            {
                if (!request.AcceptPersonalData)
                    return BadRequest("Необходимо согласие на обработку персональных данных");

                var clientId = GetClientIdFromToken();
                if (clientId == Guid.Empty)
                    return Unauthorized("Неверный токен");

                var (success, error) = await _clientAccountService.GiveConsent(clientId, request.IpAddress);

                if (!success)
                    return BadRequest(error);

                return Ok(new { message = "Согласие успешно сохранено" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сохранения согласия: {ex.Message}");
            }
        }

        private string GenerateClientJwtToken(Client client)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Secret"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, client.Name),
                    new Claim(ClaimTypes.Email, client.AccountLogin!),
                    new Claim(ClaimTypes.Role, "Client"),
                    new Claim("ClientId", client.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(24),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private Guid GetClientIdFromToken()
        {
            var clientIdClaim = User.FindFirst("ClientId");
            if (clientIdClaim == null || !Guid.TryParse(clientIdClaim.Value, out var clientId))
                return Guid.Empty;

            return clientId;
        }
    }
}