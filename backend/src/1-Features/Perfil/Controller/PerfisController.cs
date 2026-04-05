using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Domain;
using Portal.Domain.Base;
using Portal.Features.Perfil.Domain;
using Portal.Features.Perfil.Domain.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Portal.Features.Perfil.Controller
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/perfis")]
    [Authorize]
    public class PerfisController : BaseController
    {
        private readonly IPerfilService _service;
        public PerfisController(IPerfilService service)
        {
            _service = service;
        }

        [ScopesAuthorize("perfil.apagar")]
        [HttpDelete("{id:int}")]
        [SwaggerOperation(Summary = "Remove um perfil e seus vínculos")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ApagarAsync([FromRoute] int id, CancellationToken cancellationToken)
            =>  HandleResult(await _service.ApagarAsync(id, cancellationToken));
        

        [ScopesAuthorize("perfil.criar")]
        [HttpPost("{id:int}/clonar")]
        [SwaggerOperation(Summary = "Clona um perfil (nome + escopos)")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ClonarAsync([FromRoute] int id, CancellationToken cancellationToken)
            => HandleResult(await _service.ClonarAsync(id, cancellationToken));

        [ScopesAuthorize("perfil.ler")]
        [HttpGet]
        [SwaggerOperation(Summary = "Lista todos os perfis com seus escopos")]
        [ProducesResponseType(typeof(IEnumerable<PerfilComEscopoResponse>), 200)]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterPerfilComEscoposAsync(CancellationToken cancellationToken)
            => HandleResult(await _service.ListarComEscoposAsync(cancellationToken));

        [ScopesAuthorize("perfil.ler")]
        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Retorna um perfil pelo Id")]
        [ProducesResponseType(typeof(PerfilComEscopoResponse), 200)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterPorIdAsync([FromRoute] int id, CancellationToken cancellationToken)
            => HandleResult(await _service.ObterPorIdAsync(id, cancellationToken));

        [ScopesAuthorize("perfil.criar")]
        [HttpPost]
        [SwaggerOperation(Summary = "Cria um novo perfil")]
        [ProducesResponseType(typeof(int), 201)]
        [ProducesBadRequestProblem]
        [ProducesBusinessProblem]
        public async Task<IActionResult> CriarAsync([FromBody] PerfilRequest request, CancellationToken cancellationToken)
            => HandleResult(await _service.CriarAsync(request, cancellationToken));

        [ScopesAuthorize("perfil.atualizar")]
        [HttpPost("vincular/{id:int}/escopos")]
        [SwaggerOperation(Summary = "Vincula uma lista de escopos a um perfil")]
        [ProducesResponseType(204)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        [ProducesBusinessProblem]
        public async Task<IActionResult> VincularEscoposAsync([FromRoute] int id, [FromBody] VincularEscopoRequest request, CancellationToken cancellationToken)
            => HandleResult(await _service.VincularEscoposAsync(id, request.EscopoIds, cancellationToken));

        [ScopesAuthorize("perfil.atualizar")]
        [HttpPut("{id:int}")]
        [SwaggerOperation(Summary = "Altera o nome do perfil")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesNotFoundProblem]
        [ProducesBadRequestProblem]
        public async Task<IActionResult> AtualizarNomeAsync([FromRoute] int id, [FromBody] string nome, CancellationToken cancellationToken)
            => HandleResult(await _service.AtualizarNomeAsync(id, nome, cancellationToken));

    }
}
