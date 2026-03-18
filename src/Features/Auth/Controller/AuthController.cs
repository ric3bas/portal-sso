using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Dominio;
using Portal.Features.Auth.Domain.Interfaces;
using Portal.Features.Auth.Domain.Requests;
using Portal.Features.Auth.Domain.Responses;
using Swashbuckle.AspNetCore.Annotations;

namespace Portal.Features.Auth.Controller
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [SwaggerOperation(Summary = "Realiza o login de um usuário")]
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesBadRequestProblem]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }

        [SwaggerOperation(Summary = "Renova o access token a partir de um refresh token válido")]
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesBadRequestProblem]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest request)
        {
            var response = await _authService.RefreshAsync(request);
            return Ok(response);
        }

        [SwaggerOperation(Summary = "Realiza o logout revogando o refresh token")]
        [HttpPost("logout")]
        [ProducesResponseType(204)]
        [ProducesBadRequestProblem]
        public async Task<IActionResult> LogoutAsync([FromBody] LogoutRequest request)
        {
            await _authService.LogoutAsync(request);
            return NoContent();
        }

        [SwaggerOperation(Summary = "Solicita recuperação de senha (envia e-mail)")]
        [HttpPost("esqueceu-senha")]
        [ProducesResponseType(typeof(RecuperarSenhaResponse), 200)]
        [ProducesBadRequestProblem]
        public async Task<ActionResult<RecuperarSenhaResponse>> Solicitar([FromBody] RecuperarSenhaRequest request)
        {
            var result = await _authService.SolicitarRecuperacaoAsync(request);
            return Ok(result);
        }

        [SwaggerOperation(Summary = "Valida o token de recuperação de senha")]
        [HttpGet("validar-token")]
        [ProducesResponseType(typeof(ValidarTokenRecuperacaoResponse), 200)]
        [ProducesBadRequestProblem]
        public async Task<ActionResult<ValidarTokenRecuperacaoResponse>> ValidarToken([FromQuery] string token)
        {
            var request = new ValidarTokenRecuperacaoRequest { Token = token };
            var result = await _authService.ValidarTokenAsync(request);
            return Ok(result);
        }

        [SwaggerOperation(Summary = "Troca a senha do usuário usando token de recuperação")]
        [HttpPost("trocar-senha")]
        [ProducesResponseType(typeof(TrocarSenhaResponse), 200)]
        [ProducesBadRequestProblem]
        public async Task<ActionResult<TrocarSenhaResponse>> TrocarSenha([FromBody] TrocarSenhaRequest request)
        {
            var result = await _authService.TrocarSenhaAsync(request);
            return Ok(result);
        }
    }
}
