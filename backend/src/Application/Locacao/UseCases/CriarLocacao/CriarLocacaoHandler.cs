using FluentValidation;
using Portal.Domain.Base;
using Portal.Domain.Locacao;
using Portal.Domain.Locacao.Interfaces;

namespace Portal.Application.Locacao.UseCases.CriarLocacao;

public class CriarLocacaoHandler
{
    private readonly ILocacaoRepository _repository;
    private readonly IValidator<CriarLocacaoRequest> _validator;

    public CriarLocacaoHandler(ILocacaoRepository repository, IValidator<CriarLocacaoRequest> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Result<CriarLocacaoResponse>> Handle(CriarLocacaoRequest request, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.ValidationResult<CriarLocacaoResponse>(validation.Errors.Select(x => x.ErrorMessage));

        if (!await _repository.ClienteExisteAsync(Guid.Parse(request.ClienteId), cancellationToken))
            return Result.ValidationResult<CriarLocacaoResponse>("Cliente não encontrado");

        if (!await _repository.EquipamentoExisteAsync(Guid.Parse(request.EquipamentoId), cancellationToken))
            return Result.ValidationResult<CriarLocacaoResponse>("Equipamento não encontrado");

        if (await _repository.ClienteBloqueadoAsync(Guid.Parse(request.ClienteId), cancellationToken))
            return Result.ValidationResult<CriarLocacaoResponse>("Cliente está bloqueado e não pode fazer locações");

        if (!await _repository.EquipamentoDisponivelAsync(Guid.Parse(request.EquipamentoId), request.DataRetirada, request.PrevisaoDevolucao, null, cancellationToken))
            return Result.ValidationResult<CriarLocacaoResponse>("Equipamento não está disponível no período solicitado");

        var valorDiariaEquipamento = await _repository.ObterValorDiariaEquipamentoAsync(Guid.Parse(request.EquipamentoId), cancellationToken);
        if (Math.Abs(request.ValorDiaria - valorDiariaEquipamento) > 0.01m)
            return Result.ValidationResult<CriarLocacaoResponse>($"Valor da diária informado ({request.ValorDiaria:C}) não confere com o valor do equipamento ({valorDiariaEquipamento:C})");

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

        var id = await _repository.CriarAsync(entity, cancellationToken);
        return Result.OkResult(new CriarLocacaoResponse { Id = id });
    }
}
