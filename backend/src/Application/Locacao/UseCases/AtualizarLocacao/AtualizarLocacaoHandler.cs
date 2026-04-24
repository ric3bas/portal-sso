using Portal.Domain.Base;
using Portal.Domain.Locacao;
using Portal.Domain.Locacao.Interfaces;

namespace Portal.Application.Locacao.UseCases.AtualizarLocacao;

public class AtualizarLocacaoHandler
{
    private readonly ILocacaoRepository _repository;

    public AtualizarLocacaoHandler(ILocacaoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<string>> Handle(AtualizarLocacaoRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsValid()) return Result.ValidationResult<string>(request.ObterErros());

        if (!await _repository.ExisteAsync(request.Id, cancellationToken))
            return Result.NotFoundResult<string>("Locação não encontrada");

        var locacaoAtual = await _repository.ObterPorIdAsync(request.Id, cancellationToken);
        if (locacaoAtual?.Status != StatusLocacao.Ativa)
            return Result.ValidationResult<string>("Apenas locações ativas podem ser alteradas");

        if (!await _repository.ClienteExisteAsync(Guid.Parse(request.ClienteId), cancellationToken))
            return Result.ValidationResult<string>("Cliente não encontrado");

        if (!await _repository.EquipamentoExisteAsync(Guid.Parse(request.EquipamentoId), cancellationToken))
            return Result.ValidationResult<string>("Equipamento não encontrado");

        if (await _repository.ClienteBloqueadoAsync(Guid.Parse(request.ClienteId), cancellationToken))
            return Result.ValidationResult<string>("Cliente está bloqueado");

        if (!await _repository.EquipamentoDisponivelAsync(Guid.Parse(request.EquipamentoId), request.DataRetirada, request.PrevisaoDevolucao, request.Id, cancellationToken))
            return Result.ValidationResult<string>("Equipamento não está disponível no período solicitado");

        var entity = new LocacaoCommand
        {
            Id = request.Id,
            ClienteId = Guid.Parse(request.ClienteId),
            EquipamentoId = Guid.Parse(request.EquipamentoId),
            DataRetirada = request.DataRetirada,
            PrevisaoDevolucao = request.PrevisaoDevolucao,
            ValorDiaria = request.ValorDiaria,
            Observacao = request.Observacao,
            Status = StatusLocacao.Ativa
        };

        var rows = await _repository.AtualizarAsync(entity, cancellationToken);
        if (rows == 0)
            return Result.NotFoundResult<string>("Locação não encontrada");

        return Result.OkResult("Locação atualizada com sucesso");
    }
}
