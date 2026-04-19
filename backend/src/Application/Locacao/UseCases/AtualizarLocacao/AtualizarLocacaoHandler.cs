using FluentValidation;
using Portal.Domain.Base;
using Portal.Domain.Locacao;
using Portal.Domain.Locacao.Interfaces;

namespace Portal.Application.Locacao.UseCases.AtualizarLocacao;

public class AtualizarLocacaoHandler
{
    private readonly ILocacaoRepository _repository;
    private readonly IValidator<AtualizarLocacaoRequest> _validator;

    public AtualizarLocacaoHandler(ILocacaoRepository repository, IValidator<AtualizarLocacaoRequest> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Result<AtualizarLocacaoResponse>> Handle(AtualizarLocacaoRequest request, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.ValidationResult<AtualizarLocacaoResponse>(validation.Errors.Select(x => x.ErrorMessage));

        if (!await _repository.ExisteAsync(request.Id, cancellationToken))
            return Result.NotFoundResult<AtualizarLocacaoResponse>("Locação não encontrada");

        var locacaoAtual = await _repository.ObterPorIdAsync(request.Id, cancellationToken);
        if (locacaoAtual?.Status != StatusLocacao.Ativa)
            return Result.ValidationResult<AtualizarLocacaoResponse>("Apenas locações ativas podem ser alteradas");

        if (!await _repository.ClienteExisteAsync(Guid.Parse(request.ClienteId), cancellationToken))
            return Result.ValidationResult<AtualizarLocacaoResponse>("Cliente não encontrado");

        if (!await _repository.EquipamentoExisteAsync(Guid.Parse(request.EquipamentoId), cancellationToken))
            return Result.ValidationResult<AtualizarLocacaoResponse>("Equipamento não encontrado");

        if (await _repository.ClienteBloqueadoAsync(Guid.Parse(request.ClienteId), cancellationToken))
            return Result.ValidationResult<AtualizarLocacaoResponse>("Cliente está bloqueado");

        if (!await _repository.EquipamentoDisponivelAsync(Guid.Parse(request.EquipamentoId), request.DataRetirada, request.PrevisaoDevolucao, request.Id, cancellationToken))
            return Result.ValidationResult<AtualizarLocacaoResponse>("Equipamento não está disponível no período solicitado");

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
            return Result.NotFoundResult<AtualizarLocacaoResponse>("Locação não encontrada");

        return Result.OkResult(new AtualizarLocacaoResponse { Mensagem = "Locação atualizada com sucesso" });
    }
}
