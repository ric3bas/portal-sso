using Portal.Domain.Base;
using Portal.Domain.Common;
using Portal.Domain.Equipamento.Interfaces;

namespace Portal.Application.Equipamento.UseCases.ObterEquipamentos;

public class ObterEquipamentosHandler
{
    private readonly IEquipamentoRepository _repository;

    public ObterEquipamentosHandler(IEquipamentoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TabelaPaginadaResponse<ObterEquipamentosResponse>>> Handle(ObterEquipamentosRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _repository.ObterTodosAsync(request.DirecaoEnum, request.Pagina, request.TamanhoPagina, cancellationToken);
        var response = resultado.Itens.Select(x => x.ToResponse<ObterEquipamentosResponse>()).ToList();
        var tabela = TabelaPaginadaResponse<ObterEquipamentosResponse>.Criar(response, resultado.TotalRegistros, resultado.TotalRegistros, resultado.NumeroPagina, resultado.TamanhoPagina);

        return Result.OkResult(tabela);
    }
}
