
using Portal.Domain.Base;
using Portal.Domain.Common;
using Portal.Domain.Parceiro.Interfaces;

namespace Portal.Application.Parceiro.UseCases.ObterParceirosPorFiltro;

public class ObterParceirosPorFiltroHandler
{
    private readonly IParceiroRepository _repository;

    public ObterParceirosPorFiltroHandler(IParceiroRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TabelaPaginadaResponse<ObterParceirosPorFiltroResponse>>> Handle(ObterParceirosPorFiltroRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _repository.ObterTodosPorFiltroAsync(request.Nome, request.DirecaoEnum, request.Pagina, request.TamanhoPagina, cancellationToken);
        var response = resultado.Itens.Select(x => x.ToResponse<ObterParceirosPorFiltroResponse>()).ToList();
        var tabela = TabelaPaginadaResponse<ObterParceirosPorFiltroResponse>.Criar(response, resultado.TotalRegistros, resultado.TotalRegistros, resultado.NumeroPagina, resultado.TamanhoPagina);

        return Result.OkResult(tabela);
    }
}
