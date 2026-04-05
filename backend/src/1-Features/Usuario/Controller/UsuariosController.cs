using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal._1_Features.Usuario.Domain.Responses;
using Portal.Domain.Base;
using Portal.Features.Usuario.Domain;
using Portal.Features.Usuario.Domain.Interfaces;
using Portal.Features.Usuario.Infra;
using Portal.Features.Usuario.Service;
using Swashbuckle.AspNetCore.Annotations;

namespace Portal.Features.Usuario.Controller
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/usuarios")]
    [Authorize]
    public class UsuariosController : BaseController
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Lista os usuários do parceiro")]
        [ProducesResponseType(typeof(IEnumerable<UsuarioComPerfilResponse>), 200)]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ListarPorParceiroAsync([FromQuery] string? parceiroId, CancellationToken cancellationToken)
            => HandleResult(await _usuarioService.ListarPorParceiroAsync(parceiroId, cancellationToken));

        [HttpPost]
        [SwaggerOperation(Summary = "Registra um novo usuário")]
        [ProducesResponseType(200)]
        [ProducesBadRequestProblem]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request, CancellationToken cancellationToken)
            => HandleResult(await _usuarioService.RegisterAsync(request, cancellationToken));

    }
}
