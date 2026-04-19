using Portal.Application.Locacao.Common;
using Portal.Domain.Base;
using Portal.Domain.Locacao.Interfaces;
using Portal.Domain.Portal.Extensions;

namespace Portal.Application.Locacao.UseCases.ObterLocacoesPorFiltro;

public class ObterLocacoesPorFiltroHandler
{
    private readonly ILocacaoRepository _repository;

    public ObterLocacoesPorFiltroHandler(ILocacaoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ObterLocacoesPorFiltroResponse>>> Handle(ObterLocacoesPorFiltroRequest request, CancellationToken cancellationToken)
    {
        var locacoes = await _repository.ObterPorFiltroAsync(request.ClienteId, request.EquipamentoId, request.Status, request.DataRetiradaInicio, request.DataRetiradaFim, cancellationToken);
        if (locacoes == null || !locacoes.Any())
            return Result.NotFoundResult<List<ObterLocacoesPorFiltroResponse>>("Nenhuma locação encontrada");

        return Result.OkResult(locacoes.Select(x => x.ToResponse<ObterLocacoesPorFiltroResponse>()).ToList());
    }
}
