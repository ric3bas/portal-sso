using Microsoft.AspNetCore.Mvc;
using Portal.Domain;
using Portal.Domain.Base;
using Portal.Features.Financeiro.Domain;
using Portal.Features.Financeiro.Domain.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Portal.Features.Financeiro.Controller
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/financeiro")]
    [AuthorizeTenantId]
    public class FinanceiroController : BaseController
    {
        private readonly IFinanceiroService _service;

        public FinanceiroController(IFinanceiroService service)
        {
            _service = service;
        }

        [ScopesAuthorize("financeiro.ler")]
        [SwaggerOperation(Summary = "Lista todos os lançamentos financeiros")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<FinanceiroResponse>), StatusCodes.Status200OK)]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterTodosAsync(CancellationToken cancellationToken)
            => HandleResult(await _service.ObterTodosLancamentosAsync(cancellationToken));

        [ScopesAuthorize("financeiro.ler")]
        [SwaggerOperation(Summary = "Lista lançamentos financeiros por período")]
        [HttpGet("periodo")]
        [ProducesResponseType(typeof(IEnumerable<FinanceiroResponse>), StatusCodes.Status200OK)]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterPorPeriodoAsync([FromQuery] DateTime dataInicio, [FromQuery] DateTime dataFim, CancellationToken cancellationToken)
            => HandleResult(await _service.ObterLancamentosPorPeriodoAsync(dataInicio, dataFim, cancellationToken));
    }
}