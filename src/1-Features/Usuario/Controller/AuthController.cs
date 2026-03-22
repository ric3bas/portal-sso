using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Domain.Base;
using Portal.Features.Auth.Domain.Requests;
using Portal.Features.Usuario.Domain.Interfaces;
using Portal.Features.Usuario.Domain.Responses;
using Swashbuckle.AspNetCore.Annotations;

namespace Portal.Features.Usuario.Controller
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
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var response = await _authService.LoginAsync(request, cancellationToken);
            return Ok(response);
        }

        [SwaggerOperation(Summary = "Renova o access token a partir de um refresh token válido")]
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesBadRequestProblem]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var response = await _authService.RefreshAsync(request, cancellationToken);
            return Ok(response);
        }

        [SwaggerOperation(Summary = "Realiza o logout revogando o refresh token")]
        [HttpPost("logout")]
        [ProducesResponseType(204)]
        [ProducesBadRequestProblem]
        public async Task<IActionResult> LogoutAsync([FromBody] LogoutRequest request, CancellationToken cancellationToken)
        {
            await _authService.LogoutAsync(request, cancellationToken);
            return NoContent();
        }

        [SwaggerOperation(Summary = "Solicita recuperação de senha (envia e-mail)")]
        [HttpPost("esqueceu-senha")]
        [ProducesResponseType(typeof(RecuperarSenhaResponse), 200)]
        [ProducesBadRequestProblem]
        public async Task<ActionResult<RecuperarSenhaResponse>> SolicitarAsync([FromBody] RecuperarSenhaRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.SolicitarRecuperacaoAsync(request, cancellationToken);
            return Ok(result);
        }

        [SwaggerOperation(Summary = "Valida o token de recuperação de senha")]
        [HttpGet("validar-token")]
        [ProducesResponseType(typeof(ValidarTokenRecuperacaoResponse), 200)]
        [ProducesBadRequestProblem]
        public async Task<ActionResult<ValidarTokenRecuperacaoResponse>> ValidarTokenAsync([FromQuery] string token, CancellationToken cancellationToken)
        {
            var request = new ValidarTokenRecuperacaoRequest { Token = token };
            var result = await _authService.ValidarTokenAsync(request, cancellationToken);
            return Ok(result);
        }

        [SwaggerOperation(Summary = "Troca a senha do usuário usando token de recuperação")]
        [HttpPost("trocar-senha")]
        [ProducesResponseType(typeof(TrocarSenhaResponse), 200)]
        [ProducesBadRequestProblem]
        public async Task<ActionResult<TrocarSenhaResponse>> TrocarSenhaAsync([FromBody] TrocarSenhaRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.TrocarSenhaAsync(request, cancellationToken);
            return Ok(result);
        }

        #region Google
        //[ProducesResponseType(typeof(LoginResponse), 200)]
        //[HttpGet("callback")]
        //public async Task<IActionResult> Callback(string code)
        //{
        //    var response = _authService.TesteGoogle(codd);
        //    return Ok(response);
        //}
        #endregion
    }
}
