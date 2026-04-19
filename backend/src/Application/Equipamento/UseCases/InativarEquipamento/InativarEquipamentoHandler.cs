using Portal.Domain.Base;
using Portal.Domain.Equipamento.Interfaces;

namespace Portal.Application.Equipamento.UseCases.InativarEquipamento;

public class InativarEquipamentoHandler
{
    private readonly IEquipamentoRepository _repository;

    public InativarEquipamentoHandler(IEquipamentoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<InativarEquipamentoResponse>> Handle(InativarEquipamentoRequest request, CancellationToken cancellationToken)
    {
        if (!await _repository.ExisteAsync(request.Id, cancellationToken))
        {
            return Result.NotFoundResult<InativarEquipamentoResponse>("Equipamento não encontrado");
        }

        var rows = await _repository.InativarAsync(request.Id, cancellationToken);
        if (rows == 0)
        {
            return Result.NotFoundResult<InativarEquipamentoResponse>("Equipamento não encontrado");
        }

        return Result.OkResult(new InativarEquipamentoResponse { Mensagem = "Equipamento inativado com sucesso" });
    }
}
