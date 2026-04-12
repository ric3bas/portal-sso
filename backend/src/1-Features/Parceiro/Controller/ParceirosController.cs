using Microsoft.AspNetCore.Mvc;
using Portal.Domain;
using Portal.Domain.Base;
using Portal.Features.Parceiro.Domain;
using Portal.Features.Parceiro.Domain.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Portal.Features.Parceiro.Controller {
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/parceiros")]
    [AuthorizeTenantId]
    public class ParceirosController : BaseController
    {
        private readonly IParceiroService _service;
        public ParceirosController(IParceiroService service)
        {
            _service = service;
        }

        [ScopesAuthorize("parceiro.ler")]
        [SwaggerOperation(Summary = "Lista todos os parceiros")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ParceiroResponse>), StatusCodes.Status200OK)]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterTodosAsync(CancellationToken cancellationToken)
            => HandleResult(await _service.ObterTodosParceirosAsync(cancellationToken));

        [ScopesAuthorize("parceiro.ler")]
        [SwaggerOperation(Summary = "Lista todos os parceiros por filtro")]
        [HttpGet("filtro")]
        [ProducesResponseType(typeof(IEnumerable<ParceiroResponse>), StatusCodes.Status200OK)]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterTodosPorFiltroAsync([FromQuery] string? nome, CancellationToken cancellationToken)
          => HandleResult(await _service.ObterTodosPorFiltroAsync(nome, cancellationToken));

        [ScopesAuthorize("parceiro.ler")]
        [HttpGet("id")]
        [SwaggerOperation(Summary = "Retorna um parceiro pelo Id")]
        [ProducesResponseType(typeof(ParceiroResponse), StatusCodes.Status200OK)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterPorIdAsync([FromQuery] string? id, CancellationToken cancellationToken)
            => HandleResult(await _service.ObterParceiroAsync(id, cancellationToken));

        [ScopesAuthorize("parceiro.criar")]
        [HttpPost]
        [SwaggerOperation(Summary = "Cria um novo parceiro")]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesBadRequestProblem]
        public async Task<IActionResult> CreateAsync([FromBody] ParceiroRequest parceiro, CancellationToken cancellationToken)
            => HandleResult(await _service.CriarParceiroAsync(parceiro, cancellationToken));

        [ScopesAuthorize("parceiro.atualizar")]
        [HttpPatch("id")]
        [SwaggerOperation(Summary = "Atualiza um parceiro existente")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> UpdateAsync([FromQuery] string? id, [FromBody] AtualizarParceiroRequest request, CancellationToken cancellationToken)
        {
            request.Id = id;
            var result = await _service.AtualizarParceiroAsync(request, cancellationToken);
            return HandleResult(result);
        }
    }
}