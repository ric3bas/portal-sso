using Portal.Domain.Base;
using Portal.Domain.Categoria;
using Portal.Domain.Categoria.Interfaces;
using Portal.Domain.Portal.Extensions;

namespace Portal.Application.Categoria.UseCases.ObterCategorias;

public class ObterCategoriasHandler : BaseService
{
    private readonly ICategoriaRepository _repository;
    private readonly ILogger<ObterCategoriasHandler> _logger;

    public ObterCategoriasHandler(
        ICategoriaRepository repository,
        ILogger<ObterCategoriasHandler> logger,
        IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<ObterCategoriasResponse>>> Handle(ObterCategoriasRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listando todas as categorias");

        var usuario = ObterUsuario();
        var parceiroId = usuario.IsMaster ? Guid.Empty : usuario.ParceiroId;

        var result = usuario.IsMaster
            ? await _repository.ObterTodasAsync(cancellationToken)
            : await _repository.ObterPorParceiroAsync(parceiroId, cancellationToken);

        if (result == null || !result.Any())
            return Result.NotFoundResult<List<ObterCategoriasResponse>>("Nenhuma categoria encontrada");

        return Result.OkResult(result.Select(c => c.ToResponse<ObterCategoriasResponse>()).ToList());
    }
}
