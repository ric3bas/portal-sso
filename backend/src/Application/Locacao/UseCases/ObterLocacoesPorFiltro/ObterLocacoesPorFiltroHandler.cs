using Portal.Application.Locacao.Common;
using Portal.Domain.Base;
using Portal.Domain.Common;
using Portal.Domain.Locacao.Interfaces;

namespace Portal.Application.Locacao.UseCases.ObterLocacoesPorFiltro;

public class ObterLocacoesPorFiltroHandler
{
    private readonly ILocacaoRepository _repository;

    public ObterLocacoesPorFiltroHandler(ILocacaoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TabelaPaginadaResponse<ObterLocacoesPorFiltroResponse>>> Handle(ObterLocacoesPorFiltroRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _repository.ObterPorFiltroAsync(request.ClienteId, request.EquipamentoId, request.Status, request.DataRetiradaInicio, request.DataRetiradaFim, request.DirecaoEnum, request.Pagina, request.TamanhoPagina, cancellationToken);
        var response = resultado.Itens.Select(x => x.ToResponse<ObterLocacoesPorFiltroResponse>()).ToList();
        var tabela = TabelaPaginadaResponse<ObterLocacoesPorFiltroResponse>.Criar(response, resultado.TotalRegistros, resultado.TotalRegistros, resultado.NumeroPagina, resultado.TamanhoPagina);

        return Result.OkResult(tabela);
    }
}
