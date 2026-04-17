using Microsoft.AspNetCore.Mvc;
using Portal.Domain;
using Portal.Domain.Base;
using Portal.Features.Equipamento.Domain;
using Portal.Features.Equipamento.Domain.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Portal.Features.Equipamento.Controller
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/equipamentos")]
    [AuthorizeTenantId]
    public class EquipamentosController : BaseController
    {
        private readonly IEquipamentoService _service;

        public EquipamentosController(IEquipamentoService service)
        {
            _service = service;
        }

        [ScopesAuthorize("equipamento.ler")]
        [SwaggerOperation(Summary = "Lista todos os equipamentos")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EquipamentoResponse>), StatusCodes.Status200OK)]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterTodosAsync(CancellationToken cancellationToken)
            => HandleResult(await _service.ObterTodosEquipamentosAsync(cancellationToken));

        [ScopesAuthorize("equipamento.ler")]
        [SwaggerOperation(Summary = "Lista equipamentos por filtros")]
        [HttpGet("filtro")]
        [ProducesResponseType(typeof(IEnumerable<EquipamentoResponse>), StatusCodes.Status200OK)]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterPorFiltroAsync([FromQuery] FiltroEquipamentoRequest filtro, CancellationToken cancellationToken)
            => HandleResult(await _service.ObterEquipamentosPorFiltroAsync(filtro, cancellationToken));

        [ScopesAuthorize("equipamento.ler")]
        [HttpGet("id")]
        [SwaggerOperation(Summary = "Retorna um equipamento pelo Id")]
        [ProducesResponseType(typeof(EquipamentoResponse), StatusCodes.Status200OK)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterPorIdAsync([FromQuery] string? id, CancellationToken cancellationToken)
            => HandleResult(await _service.ObterEquipamentoAsync(id, cancellationToken));

        [ScopesAuthorize("equipamento.criar")]
        [HttpPost]
        [SwaggerOperation(Summary = "Cria um novo equipamento")]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesBadRequestProblem]
        public async Task<IActionResult> CreateAsync([FromBody] EquipamentoRequest equipamento, CancellationToken cancellationToken)
            => HandleResult(await _service.CriarEquipamentoAsync(equipamento, cancellationToken));

        [ScopesAuthorize("equipamento.atualizar")]
        [HttpPatch("id")]
        [SwaggerOperation(Summary = "Atualiza um equipamento existente")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> UpdateAsync([FromQuery] string? id, [FromBody] AtualizarEquipamentoRequest request, CancellationToken cancellationToken)
        {
            request.Id = id;
            var result = await _service.AtualizarEquipamentoAsync(request, cancellationToken);
            return HandleResult(result);
        }

        [ScopesAuthorize("equipamento.inativar")]
        [HttpPatch("{id}/inativar")]
        [SwaggerOperation(Summary = "Inativa um equipamento")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> InativarAsync([FromRoute] string? id, CancellationToken cancellationToken)
            => HandleResult(await _service.InativarEquipamentoAsync(id, cancellationToken));
    }
}