using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Portal.Domain;
using Portal.Domain.Base;
using Portal.Features.Escopo.Domain;
using Portal.Features.Escopo.Domain.Interfaces;
using Portal.Features.Escopo.Infra;
using Portal.Features.Parceiro.Domain;
using Swashbuckle.AspNetCore.Annotations;


namespace Portal.Features.Escopo.Controller
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/escopos")]
    [Authorize]
    public class EscoposController(IEscopoService _service) : BaseController
    {
        [ScopesAuthorize("escopo.ler")]
        [HttpGet]
        [SwaggerOperation(Summary = "Lista todos os escopos")]
        [ProducesResponseType(typeof(IEnumerable<EscopoResponse>), StatusCodes.Status200OK)]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterTodosAsync(CancellationToken cancellationToken)
        {
            var result = await _service.ObterTodosAsync(cancellationToken);
            return HandleResult(result);

        }

        [ScopesAuthorize("escopo.ler")]
        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Retorna um escopo pelo Id")]
        [ProducesResponseType(typeof(EscopoResponse), StatusCodes.Status200OK)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterIdAsync([FromRoute] int id, CancellationToken cancellationToken)
        {
            var result = await _service.ObterPorIdAsync(id, cancellationToken);
            return HandleResult(result);

        }

        [ScopesAuthorize("escopo.criar")]
        [HttpPost]
        [SwaggerOperation(Summary = "Cria um novo escopo")]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesBadRequestProblem]
        [ProducesBusinessProblem]
        public async Task<IActionResult> CriarAsync([FromBody] EscopoRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.CriarAsync(request, cancellationToken);
            return HandleResult(result);
        }

        [ScopesAuthorize("escopo.atualizar")]
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Atualiza um escopo existente")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> AtualizarAsync([FromRoute] int id, [FromBody] AtualizarEscopoRequest request, CancellationToken cancellationToken)
        {
            request.Id = id;
            var result = await _service.AtualizarAsync(request, cancellationToken);
            return HandleResult(result);
        }
    }
}
