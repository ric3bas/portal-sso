using Microsoft.AspNetCore.Mvc;
using Portal.Domain;
using Portal.Domain.Base;
using Portal.Features.Cliente.Domain;
using Portal.Features.Cliente.Domain.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Portal.Features.Cliente.Controller
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/clientes")]
    [AuthorizeTenantId]
    public class ClientesController : BaseController
    {
        private readonly IClienteService _service;

        public ClientesController(IClienteService service)
        {
            _service = service;
        }

        [ScopesAuthorize("cliente.ler")]
        [SwaggerOperation(Summary = "Lista todos os clientes")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ClienteResponse>), StatusCodes.Status200OK)]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterTodosAsync(CancellationToken cancellationToken)
            => HandleResult(await _service.ObterTodosClientesAsync(cancellationToken));

        [ScopesAuthorize("cliente.ler")]
        [SwaggerOperation(Summary = "Lista clientes por filtros")]
        [HttpGet("filtro")]
        [ProducesResponseType(typeof(IEnumerable<ClienteResponse>), StatusCodes.Status200OK)]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterPorFiltroAsync([FromQuery] FiltroClienteRequest filtro, CancellationToken cancellationToken)
            => HandleResult(await _service.ObterClientesPorFiltroAsync(filtro, cancellationToken));

        [ScopesAuthorize("cliente.ler")]
        [HttpGet("id")]
        [SwaggerOperation(Summary = "Retorna um cliente pelo Id")]
        [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterPorIdAsync([FromQuery] string? id, CancellationToken cancellationToken)
            => HandleResult(await _service.ObterClienteAsync(id, cancellationToken));

        [ScopesAuthorize("cliente.criar")]
        [HttpPost]
        [SwaggerOperation(Summary = "Cria um novo cliente")]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesBadRequestProblem]
        public async Task<IActionResult> CreateAsync([FromBody] ClienteRequest cliente, CancellationToken cancellationToken)
            => HandleResult(await _service.CriarClienteAsync(cliente, cancellationToken));

        [ScopesAuthorize("cliente.atualizar")]
        [HttpPatch("id")]
        [SwaggerOperation(Summary = "Atualiza um cliente existente")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> UpdateAsync([FromQuery] string? id, [FromBody] AtualizarClienteRequest request, CancellationToken cancellationToken)
        {
            request.Id = id;
            var result = await _service.AtualizarClienteAsync(request, cancellationToken);
            return HandleResult(result);
        }

        [ScopesAuthorize("cliente.bloquear")]
        [HttpPatch("{id}/bloquear")]
        [SwaggerOperation(Summary = "Bloqueia um cliente")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> BloquearAsync([FromRoute] string? id, CancellationToken cancellationToken)
            => HandleResult(await _service.BloquearClienteAsync(id, cancellationToken));

        [ScopesAuthorize("cliente.desbloquear")]
        [HttpPatch("{id}/desbloquear")]
        [SwaggerOperation(Summary = "Desbloqueia um cliente")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> DesbloquearAsync([FromRoute] string? id, CancellationToken cancellationToken)
            => HandleResult(await _service.DesbloquearClienteAsync(id, cancellationToken));

        [ScopesAuthorize("cliente.inativar")]
        [HttpPatch("{id}/inativar")]
        [SwaggerOperation(Summary = "Inativa um cliente")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> InativarAsync([FromRoute] string? id, CancellationToken cancellationToken)
            => HandleResult(await _service.InativarClienteAsync(id, cancellationToken));
    }
}