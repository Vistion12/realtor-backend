using AgencyStore.Core.Abstractions;
using Microsoft.AspNetCore.Mvc;
using rieltor_web_api.Contracts;

namespace rieltor_web_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] AuthRequest request)
        {
            var result = await _authService.AuthenticateAsync(request.Username, request.Password);

            if (result == null)
                return Unauthorized("Invalid username or password");

            var (token, username, role, expires) = result.Value;

            var response = new AuthResponse(token, username, role, expires);
            return Ok(response);
        }

        [HttpPost("check")]
        public async Task<ActionResult> CheckUserExists([FromBody] AuthRequest request)
        {
            var exists = await _authService.UserExistsAsync(request.Username);
            return Ok(new { exists });
        }
    }
}
