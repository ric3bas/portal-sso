using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Portal.Domain.Base;
using Portal.Features.Escopo.Domain;
using Portal.Features.Escopo.Domain.Interfaces;
using Portal.Features.Escopo.Infra;
using Swashbuckle.AspNetCore.Annotations;


namespace Portal.Features.Escopo.Controller
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/escopos")]
    [Authorize]
    public class EscoposController(IEscopoService _service) : BaseController
    {
        [HttpGet]
        [SwaggerOperation(Summary = "Lista todos os escopos")]
        [ProducesResponseType(typeof(IEnumerable<EscopoResponse>), 200)]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
        {
            var result = await _service.ListarAsync(cancellationToken);
            return HandleResult(result);

        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Retorna um escopo pelo Id")]
        [ProducesResponseType(typeof(EscopoResponse), 200)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> GetByIdAsync([FromRoute] int id, CancellationToken cancellationToken)
        {
            var result = await _service.ObterPorIdAsync(id, cancellationToken);
            return HandleResult(result);

        }

        [HttpPost]
        [SwaggerOperation(Summary = "Cria um novo escopo")]
        [ProducesResponseType(typeof(int), 201)]
        [ProducesBadRequestProblem]
        [ProducesBusinessProblem]
        public async Task<IActionResult> CreateAsync([FromBody] EscopoRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.CriarAsync(request.Nome, cancellationToken);
            return HandleResult(result);

        }
    }
}
