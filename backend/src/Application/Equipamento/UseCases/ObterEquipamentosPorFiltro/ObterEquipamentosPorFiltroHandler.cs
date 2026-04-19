using Portal.Domain.Base;
using Portal.Domain.Equipamento.Interfaces;
using Portal.Domain.Portal.Extensions;

namespace Portal.Application.Equipamento.UseCases.ObterEquipamentosPorFiltro;

public class ObterEquipamentosPorFiltroHandler
{
    private readonly IEquipamentoRepository _repository;

    public ObterEquipamentosPorFiltroHandler(IEquipamentoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ObterEquipamentosPorFiltroResponse>>> Handle(ObterEquipamentosPorFiltroRequest request, CancellationToken cancellationToken)
    {
        var equipamentos = await _repository.ObterPorFiltroAsync(request.Nome, request.Marca, request.Modelo, request.CategoriaId, request.Ativo, cancellationToken);
        if (equipamentos == null || !equipamentos.Any())
        {
            return Result.NotFoundResult<List<ObterEquipamentosPorFiltroResponse>>("Nenhum equipamento encontrado");
        }

        return Result.OkResult(equipamentos.Select(x => x.ToResponse<ObterEquipamentosPorFiltroResponse>()).ToList());
    }
}
