using Portal.Domain.Base;
using Portal.Domain.Categoria;
using Portal.Domain.Categoria.Interfaces;
using Portal.Domain.Portal.Extensions;

namespace Portal.Application.Categoria.UseCases.ObterCategoriasPorFiltro;

public class ObterCategoriasPorFiltroHandler : BaseService
{
    private readonly ICategoriaRepository _repository;
    private readonly ILogger<ObterCategoriasPorFiltroHandler> _logger;

    public ObterCategoriasPorFiltroHandler(
        ICategoriaRepository repository,
        ILogger<ObterCategoriasPorFiltroHandler> logger,
        IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<ObterCategoriasPorFiltroResponse>>> Handle(ObterCategoriasPorFiltroRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Listando categorias com filtro de nome: {request.Nome}");

        var usuario = ObterUsuario();
        var parceiroId = usuario.IsMaster ? Guid.Empty : usuario.ParceiroId;

        var result = usuario.IsMaster
            ? await _repository.ObterPorFiltroAsync(request.Nome ?? string.Empty, cancellationToken)
            : await _repository.ObterPorFiltroEParceiroAsync(parceiroId, request.Nome ?? string.Empty, cancellationToken);

        if (result == null || !result.Any())
            return Result.NotFoundResult<List<ObterCategoriasPorFiltroResponse>>("Nenhuma categoria encontrada");

        return Result.OkResult(result.Select(c => c.ToResponse<ObterCategoriasPorFiltroResponse>()).ToList());
    }
}
