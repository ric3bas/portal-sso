using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Dominio;
using Portal.Features.Usuario.Domain;
using Portal.Features.Usuario.Domain.Interfaces;
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
        [SwaggerOperation(Summary = "Lista os usuários do parceiro autenticado")]
        [ProducesResponseType(typeof(IEnumerable<UsuarioComPerfilResponse>), 200)]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ListarAsync(CancellationToken cancellationToken)
        {
            var result = await _usuarioService.ListarAsync(cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Registra um novo usuário")]
        [ProducesResponseType(200)]
        [ProducesBadRequestProblem]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            await _usuarioService.RegisterAsync(request, cancellationToken);
            return Ok("Usuário criado com sucesso");
        }

    }
}
