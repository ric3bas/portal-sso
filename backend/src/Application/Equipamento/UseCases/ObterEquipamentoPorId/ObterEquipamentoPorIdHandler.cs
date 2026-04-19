using Portal.Domain.Base;
using Portal.Domain.Equipamento.Interfaces;
using Portal.Domain.Portal.Extensions;

namespace Portal.Application.Equipamento.UseCases.ObterEquipamentoPorId;

public class ObterEquipamentoPorIdHandler
{
    private readonly IEquipamentoRepository _repository;

    public ObterEquipamentoPorIdHandler(IEquipamentoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ObterEquipamentoPorIdResponse>> Handle(ObterEquipamentoPorIdRequest request, CancellationToken cancellationToken)
    {
        var equipamento = await _repository.ObterPorIdAsync(request.Id, cancellationToken);
        if (equipamento is null)
        {
            return Result.NotFoundResult<ObterEquipamentoPorIdResponse>("Equipamento não encontrado");
        }

        return Result.OkResult(equipamento.ToResponse<ObterEquipamentoPorIdResponse>());
    }
}
