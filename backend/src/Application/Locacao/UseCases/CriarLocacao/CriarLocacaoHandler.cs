using Portal.Domain.Base;
using Portal.Domain.Locacao;
using Portal.Domain.Locacao.Interfaces;

namespace Portal.Application.Locacao.UseCases.CriarLocacao;

public class CriarLocacaoHandler
{
    private readonly ILocacaoRepository _repository;

    public CriarLocacaoHandler(ILocacaoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<string>> Handle(CriarLocacaoRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsValid()) return Result.ValidationResult<string>(request.ObterErros());

        if (!await _repository.ClienteExisteAsync(Guid.Parse(request.ClienteId), cancellationToken))
            return Result.ValidationResult<string>("Cliente não encontrado");

        if (!await _repository.EquipamentoExisteAsync(Guid.Parse(request.EquipamentoId), cancellationToken))
            return Result.ValidationResult<string>("Equipamento não encontrado");

        if (await _repository.ClienteBloqueadoAsync(Guid.Parse(request.ClienteId), cancellationToken))
            return Result.ValidationResult<string>("Cliente está bloqueado e não pode fazer locações");

        if (!await _repository.EquipamentoDisponivelAsync(Guid.Parse(request.EquipamentoId), request.DataRetirada, request.PrevisaoDevolucao, null, cancellationToken))
            return Result.ValidationResult<string>("Equipamento não está disponível no período solicitado");

        var valorDiariaEquipamento = await _repository.ObterValorDiariaEquipamentoAsync(Guid.Parse(request.EquipamentoId), cancellationToken);
        if (Math.Abs(request.ValorDiaria - valorDiariaEquipamento) > 0.01m)
            return Result.ValidationResult<string>($"Valor da diária informado ({request.ValorDiaria:C}) não confere com o valor do equipamento ({valorDiariaEquipamento:C})");

        var entity = new LocacaoCommand
        {
            ClienteId = Guid.Parse(request.ClienteId),
            EquipamentoId = Guid.Parse(request.EquipamentoId),
            Status = StatusLocacao.Ativa,
            DataRetirada = request.DataRetirada,
            PrevisaoDevolucao = request.PrevisaoDevolucao,
            ValorDiaria = request.ValorDiaria,
            Observacao = request.Observacao
        };

        _ = await _repository.CriarAsync(entity, cancellationToken);
        return Result.OkResult("Locação criada com sucesso");
    }
}
