using Microsoft.AspNetCore.Mvc;
using Portal.Domain;
using Portal.Domain.Base;
using Portal.Features.Locacao.Domain;
using Portal.Features.Locacao.Domain.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Portal.Features.Locacao.Controller
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/locacoes")]
    [AuthorizeTenantId]
    public class LocacoesController : BaseController
    {
        private readonly ILocacaoService _service;

        public LocacoesController(ILocacaoService service)
        {
            _service = service;
        }

        [ScopesAuthorize("locacao.ler")]
        [SwaggerOperation(Summary = "Lista todas as locações")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LocacaoResponse>), StatusCodes.Status200OK)]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterTodasAsync(CancellationToken cancellationToken)
            => HandleResult(await _service.ObterTodasLocacoesAsync(cancellationToken));

        [ScopesAuthorize("locacao.ler")]
        [SwaggerOperation(Summary = "Lista locações por filtros")]
        [HttpGet("filtro")]
        [ProducesResponseType(typeof(IEnumerable<LocacaoResponse>), StatusCodes.Status200OK)]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterPorFiltroAsync([FromQuery] FiltroLocacaoRequest filtro, CancellationToken cancellationToken)
            => HandleResult(await _service.ObterLocacoesPorFiltroAsync(filtro, cancellationToken));

        [ScopesAuthorize("locacao.ler")]
        [SwaggerOperation(Summary = "Lista locações atrasadas")]
        [HttpGet("atrasadas")]
        [ProducesResponseType(typeof(IEnumerable<LocacaoResponse>), StatusCodes.Status200OK)]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterAtrasadasAsync(CancellationToken cancellationToken)
            => HandleResult(await _service.ObterLocacoesAtrasadasAsync(cancellationToken));

        [ScopesAuthorize("locacao.ler")]
        [HttpGet("id")]
        [SwaggerOperation(Summary = "Retorna uma locação pelo Id")]
        [ProducesResponseType(typeof(LocacaoResponse), StatusCodes.Status200OK)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterPorIdAsync([FromQuery] string? id, CancellationToken cancellationToken)
            => HandleResult(await _service.ObterLocacaoAsync(id, cancellationToken));

        [ScopesAuthorize("locacao.criar")]
        [HttpPost]
        [SwaggerOperation(Summary = "Cria uma nova locação")]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesBadRequestProblem]
        public async Task<IActionResult> CreateAsync([FromBody] LocacaoRequest locacao, CancellationToken cancellationToken)
            => HandleResult(await _service.CriarLocacaoAsync(locacao, cancellationToken));

        [ScopesAuthorize("locacao.atualizar")]
        [HttpPatch("id")]
        [SwaggerOperation(Summary = "Atualiza uma locação existente")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> UpdateAsync([FromQuery] string? id, [FromBody] AtualizarLocacaoRequest request, CancellationToken cancellationToken)
        {
            request.Id = id;
            var result = await _service.AtualizarLocacaoAsync(request, cancellationToken);
            return HandleResult(result);
        }

        [ScopesAuthorize("locacao.devolver")]
        [HttpPatch("{id}/devolver")]
        [SwaggerOperation(Summary = "Devolve uma locação")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> DevolverAsync([FromRoute] string? id, [FromBody] DevolverLocacaoRequest request, CancellationToken cancellationToken)
        {
            request.Id = id;
            var result = await _service.DevolverLocacaoAsync(request, cancellationToken);
            return HandleResult(result);
        }

        [ScopesAuthorize("locacao.cancelar")]
        [HttpPatch("{id}/cancelar")]
        [SwaggerOperation(Summary = "Cancela uma locação")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> CancelarAsync([FromRoute] string? id, CancellationToken cancellationToken)
            => HandleResult(await _service.CancelarLocacaoAsync(id, cancellationToken));
    }
}