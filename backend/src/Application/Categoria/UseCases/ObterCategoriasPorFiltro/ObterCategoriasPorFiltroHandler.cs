using Portal.Domain.Base;
using Portal.Domain.Common;
using Portal.Domain.Categoria;
using Portal.Domain.Categoria.Interfaces;

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

    public async Task<Result<TabelaPaginadaResponse<ObterCategoriasPorFiltroResponse>>> Handle(ObterCategoriasPorFiltroRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Listando categorias com filtro de nome: {request.Nome}");

        var usuario = ObterUsuario();
        var parceiroId = usuario.IsMaster ? Guid.Empty : usuario.ParceiroId;

        var resultado = usuario.IsMaster
            ? await _repository.ObterPorFiltroAsync(request.Nome ?? string.Empty, request.DirecaoEnum, request.Pagina, request.TamanhoPagina, cancellationToken)
            : await _repository.ObterPorFiltroEParceiroAsync(parceiroId, request.Nome ?? string.Empty, request.DirecaoEnum, request.Pagina, request.TamanhoPagina, cancellationToken);

        var response = resultado.Itens.Select(c => c.ToResponsePorFiltro()).ToList();
        var tabela = TabelaPaginadaResponse<ObterCategoriasPorFiltroResponse>.Criar(response, resultado.TotalRegistros, resultado.TotalRegistros, resultado.NumeroPagina, resultado.TamanhoPagina);

        return Result.OkResult(tabela);
    }
}
