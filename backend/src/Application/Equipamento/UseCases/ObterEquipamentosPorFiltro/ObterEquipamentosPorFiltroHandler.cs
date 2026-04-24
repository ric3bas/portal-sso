using Portal.Domain.Base;
using Portal.Domain.Common;
using Portal.Domain.Equipamento.Interfaces;

namespace Portal.Application.Equipamento.UseCases.ObterEquipamentosPorFiltro;

public class ObterEquipamentosPorFiltroHandler
{
    private readonly IEquipamentoRepository _repository;

    public ObterEquipamentosPorFiltroHandler(IEquipamentoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TabelaPaginadaResponse<ObterEquipamentosPorFiltroResponse>>> Handle(ObterEquipamentosPorFiltroRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _repository.ObterPorFiltroAsync(request.Nome, request.Marca, request.Modelo, request.CategoriaId, request.Ativo, request.DirecaoEnum, request.Pagina, request.TamanhoPagina, cancellationToken);
        var response = resultado.Itens.Select(x => x.ToResponse<ObterEquipamentosPorFiltroResponse>()).ToList();
        var tabela = TabelaPaginadaResponse<ObterEquipamentosPorFiltroResponse>.Criar(response, resultado.TotalRegistros, resultado.TotalRegistros, resultado.NumeroPagina, resultado.TamanhoPagina);

        return Result.OkResult(tabela);
    }
}
