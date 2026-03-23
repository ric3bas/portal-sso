using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
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
            => HandleResult(await _authService.LoginAsync(request, cancellationToken));


        [SwaggerOperation(Summary = "Renova o access token a partir de um refresh token válido")]
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesBadRequestProblem]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
            => HandleResult(await _authService.RefreshAsync(request, cancellationToken));

        [SwaggerOperation(Summary = "Realiza o logout revogando o refresh token")]
        [HttpPost("logout")]
        [ProducesResponseType(204)]
        [ProducesBadRequestProblem]
        public async Task<IActionResult> LogoutAsync([FromBody] LogoutRequest request, CancellationToken cancellationToken)
             => HandleResult(await _authService.LogoutAsync(request, cancellationToken));


        [SwaggerOperation(Summary = "Solicita recuperação de senha (envia e-mail)")]
        [HttpPost("esqueceu-senha")]
        [ProducesResponseType(typeof(RecuperarSenhaResponse), 200)]
        [ProducesBadRequestProblem]
        public async Task<IActionResult> SolicitarAsync([FromBody] RecuperarSenhaRequest request, CancellationToken cancellationToken)
            => HandleResult(await _authService.SolicitarRecuperacaoAsync(request, cancellationToken));


        [SwaggerOperation(Summary = "Valida o token de recuperação de senha")]
        [HttpGet("validar-token")]
        [ProducesResponseType(typeof(ValidarTokenRecuperacaoResponse), 200)]
        [ProducesBadRequestProblem]
        public async Task<IActionResult> ValidarTokenAsync([FromQuery] string token, CancellationToken cancellationToken)
            => HandleResult(await _authService.ValidarTokenAsync(new ValidarTokenRecuperacaoRequest { Token = token }, cancellationToken));


        [SwaggerOperation(Summary = "Troca a senha do usuário usando token de recuperação")]
        [HttpPost("trocar-senha")]
        [ProducesResponseType(typeof(TrocarSenhaResponse), 200)]
        [ProducesBadRequestProblem]
        public async Task<IActionResult> TrocarSenhaAsync([FromBody] TrocarSenhaRequest request, CancellationToken cancellationToken)
            => HandleResult(await _authService.TrocarSenhaAsync(request, cancellationToken));


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
