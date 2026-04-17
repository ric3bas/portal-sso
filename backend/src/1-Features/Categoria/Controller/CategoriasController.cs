using Microsoft.AspNetCore.Mvc;
using Portal.Domain;
using Portal.Domain.Base;
using Portal.Features.Categoria.Domain;
using Portal.Features.Categoria.Domain.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Portal.Features.Categoria.Controller
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/categorias")]
    [AuthorizeTenantId]
    public class CategoriasController : BaseController
    {
        private readonly ICategoriaService _service;

        public CategoriasController(ICategoriaService service)
        {
            _service = service;
        }

        [ScopesAuthorize("categoria.ler")]
        [SwaggerOperation(Summary = "Lista todas as categorias")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CategoriaResponse>), StatusCodes.Status200OK)]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterTodasAsync(CancellationToken cancellationToken)
            => HandleResult(await _service.ObterTodasCategoriasAsync(cancellationToken));

        [ScopesAuthorize("categoria.ler")]
        [SwaggerOperation(Summary = "Lista categorias por filtro de nome")]
        [HttpGet("filtro")]
        [ProducesResponseType(typeof(IEnumerable<CategoriaResponse>), StatusCodes.Status200OK)]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterPorFiltroAsync([FromQuery] string? nome, CancellationToken cancellationToken)
            => HandleResult(await _service.ObterCategoriasPorFiltroAsync(nome, cancellationToken));

        [ScopesAuthorize("categoria.ler")]
        [HttpGet("id")]
        [SwaggerOperation(Summary = "Retorna uma categoria pelo Id")]
        [ProducesResponseType(typeof(CategoriaResponse), StatusCodes.Status200OK)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> ObterPorIdAsync([FromQuery] string? id, CancellationToken cancellationToken)
            => HandleResult(await _service.ObterCategoriaAsync(id, cancellationToken));

        [ScopesAuthorize("categoria.criar")]
        [HttpPost]
        [SwaggerOperation(Summary = "Cria uma nova categoria")]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesBadRequestProblem]
        public async Task<IActionResult> CreateAsync([FromBody] CategoriaRequest categoria, CancellationToken cancellationToken)
            => HandleResult(await _service.CriarCategoriaAsync(categoria, cancellationToken));

        [ScopesAuthorize("categoria.atualizar")]
        [HttpPatch("id")]
        [SwaggerOperation(Summary = "Atualiza uma categoria existente")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> UpdateAsync([FromQuery] string? id, [FromBody] AtualizarCategoriaRequest request, CancellationToken cancellationToken)
        {
            request.Id = id;
            var result = await _service.AtualizarCategoriaAsync(request, cancellationToken);
            return HandleResult(result);
        }

        [ScopesAuthorize("categoria.inativar")]
        [HttpPatch("{id}/inativar")]
        [SwaggerOperation(Summary = "Inativa uma categoria")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesBadRequestProblem]
        [ProducesNotFoundProblem]
        public async Task<IActionResult> InativarAsync([FromRoute] string? id, CancellationToken cancellationToken)
            => HandleResult(await _service.InativarCategoriaAsync(id, cancellationToken));
    }
}