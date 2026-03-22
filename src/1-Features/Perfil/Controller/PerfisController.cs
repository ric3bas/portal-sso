using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet]
        [SwaggerOperation(Summary = "Lista todos os perfis com seus escopos")]
        [ProducesResponseType(typeof(IEnumerable<PerfilComEscopoResponse>), 200)]
        [ProducesNotFoundProblem]
        public async Task<IEnumerable<PerfilComEscopoResponse>> GetAllComEscoposAsync(CancellationToken cancellationToken)
            => await _service.ListarComEscoposAsync(cancellationToken);

        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Retorna um perfil pelo Id")]
        [ProducesResponseType(typeof(PerfilResponse), 200)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> GetByIdAsync([FromRoute] int id, CancellationToken cancellationToken)
        {
            var perfil = await _service.ObterPorIdAsync(id, cancellationToken);
            return Ok(perfil);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Cria um novo perfil")]
        [ProducesResponseType(typeof(int), 201)]
        [ProducesBadRequestProblem]
        [ProducesBusinessProblem]
        public async Task<IActionResult> CreateAsync([FromBody] PerfilRequest request, CancellationToken cancellationToken)
        {
            var id = await _service.CriarAsync(request.Nome, cancellationToken);
            return StatusCode(201, new { id });
        }

        [HttpPost("{id:int}/escopos")]
        [SwaggerOperation(Summary = "Vincula uma lista de escopos a um perfil")]
        [ProducesResponseType(204)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        [ProducesBusinessProblem]
        public async Task<IActionResult> VincularEscoposAsync([FromRoute] int id, [FromBody] VincularEscopoRequest request, CancellationToken cancellationToken)
        {
            await _service.VincularEscoposAsync(id, request.EscopoIds, cancellationToken);
            return NoContent();
        }
    }
}
