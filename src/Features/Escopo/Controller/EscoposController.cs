using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Dominio;
using Portal.Features.Escopo.Domain;
using Portal.Features.Escopo.Domain.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using EscopoEntity = Portal.Dominio.Entities.Escopo;

namespace Portal.Features.Escopo.Controller
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/escopos")]
    [Authorize]
    public class EscoposController : BaseController
    {
        private readonly IEscopoService _service;

        public EscoposController(IEscopoService service)
        {
            _service = service;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Lista todos os escopos")]
        [ProducesResponseType(typeof(IEnumerable<EscopoResponse>), 200)]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
        {
            var result = await _service.ListarAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Retorna um escopo pelo Id")]
        [ProducesResponseType(typeof(EscopoEntity), 200)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> GetByIdAsync([FromRoute] int id, CancellationToken cancellationToken)
        {
            var escopo = await _service.ObterPorIdAsync(id, cancellationToken);
            return Ok(escopo);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Cria um novo escopo")]
        [ProducesResponseType(typeof(int), 201)]
        [ProducesBadRequestProblem]
        [ProducesBusinessProblem]
        public async Task<IActionResult> CreateAsync([FromBody] EscopoRequest request, CancellationToken cancellationToken)
        {
            var id = await _service.CriarAsync(request.Nome, cancellationToken);
            return StatusCode(201, new { id });
        }
    }
}
