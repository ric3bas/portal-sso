using Portal.Domain.Base;
using Portal.Domain.Equipamento.Interfaces;
using Portal.Domain.Portal.Extensions;

namespace Portal.Application.Equipamento.UseCases.ObterEquipamentos;

public class ObterEquipamentosHandler
{
    private readonly IEquipamentoRepository _repository;

    public ObterEquipamentosHandler(IEquipamentoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ObterEquipamentosResponse>>> Handle(ObterEquipamentosRequest request, CancellationToken cancellationToken)
    {
        var equipamentos = await _repository.ObterTodosAsync(cancellationToken);
        if (equipamentos == null || !equipamentos.Any())
        {
            return Result.NotFoundResult<List<ObterEquipamentosResponse>>("Nenhum equipamento encontrado");
        }

        return Result.OkResult(equipamentos.Select(x => x.ToResponse<ObterEquipamentosResponse>()).ToList());
    }
}
