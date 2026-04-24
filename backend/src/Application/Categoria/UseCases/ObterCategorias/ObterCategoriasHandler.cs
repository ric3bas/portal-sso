using Portal.Domain.Base;
using Portal.Domain.Common;
using Portal.Domain.Categoria;
using Portal.Domain.Categoria.Interfaces;

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

    public async Task<Result<TabelaPaginadaResponse<ObterCategoriasResponse>>> Handle(ObterCategoriasRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listando todas as categorias");

        var usuario = ObterUsuario();
        var parceiroId = usuario.IsMaster ? Guid.Empty : usuario.ParceiroId;

        var resultado = usuario.IsMaster
            ? await _repository.ObterTodasAsync(request.DirecaoEnum, request.Pagina, request.TamanhoPagina, cancellationToken)
            : await _repository.ObterPorParceiroAsync(parceiroId, request.DirecaoEnum, request.Pagina, request.TamanhoPagina, cancellationToken);

        var response = resultado.Itens.Select(c => c.ToResponse()).ToList();
        var tabela = TabelaPaginadaResponse<ObterCategoriasResponse>.Criar(response, resultado.TotalRegistros, resultado.TotalRegistros, resultado.NumeroPagina, resultado.TamanhoPagina);

        return Result.OkResult(tabela);
    }
}
