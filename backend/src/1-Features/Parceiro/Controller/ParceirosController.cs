using Microsoft.AspNetCore.Mvc;
using Portal.Features.Parceiro.Domain.Interfaces;
using Portal.Features.Parceiro.Domain;
using Swashbuckle.AspNetCore.Annotations;
using Portal.Domain.Base;

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

        [SwaggerOperation(Summary = "Lista todos os parceiros ou filtra por nome")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ParceiroResponse>), StatusCodes.Status200OK)]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterTodosAsync([FromQuery] string? nome, CancellationToken cancellationToken)
            => HandleResult(await _service.ObterTodosParceirosAsync(nome, cancellationToken));

        [HttpGet("id")]
        [SwaggerOperation(Summary = "Retorna um parceiro pelo Id")]
        [ProducesResponseType(typeof(ParceiroResponse), StatusCodes.Status200OK)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterPorIdAsync([FromQuery] string? id, CancellationToken cancellationToken)
            => HandleResult(await _service.ObterParceiroAsync(id, cancellationToken));

        [HttpPost]
        [SwaggerOperation(Summary = "Cria um novo parceiro")]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesBadRequestProblem]
        public async Task<IActionResult> CreateAsync([FromBody] ParceiroRequest parceiro, CancellationToken cancellationToken)
            => HandleResult(await _service.CriarParceiroAsync(parceiro, cancellationToken));


        [HttpPut("id")]
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