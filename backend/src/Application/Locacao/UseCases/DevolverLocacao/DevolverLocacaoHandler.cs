using FluentValidation;
using Portal.Domain.Base;
using Portal.Domain.Financeiro;
using Portal.Domain.Financeiro.Interfaces;
using Portal.Domain.Locacao;
using Portal.Domain.Locacao.Interfaces;

namespace Portal.Application.Locacao.UseCases.DevolverLocacao;

public class DevolverLocacaoHandler
{
    private readonly ILocacaoRepository _repository;
    private readonly IFinanceiroRepository _financeiroRepository;
    private readonly IValidator<DevolverLocacaoRequest> _validator;
    private readonly ILogger<DevolverLocacaoHandler> _logger;

    public DevolverLocacaoHandler(
        ILocacaoRepository repository,
        IFinanceiroRepository financeiroRepository,
        IValidator<DevolverLocacaoRequest> validator,
        ILogger<DevolverLocacaoHandler> logger)
    {
        _repository = repository;
        _financeiroRepository = financeiroRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<DevolverLocacaoResponse>> Handle(DevolverLocacaoRequest request, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.ValidationResult<DevolverLocacaoResponse>(validation.Errors.Select(x => x.ErrorMessage));

        var locacao = await _repository.ObterPorIdAsync(request.Id, cancellationToken);
        if (locacao is null)
            return Result.NotFoundResult<DevolverLocacaoResponse>("Locação não encontrada");

        if (locacao.Status != StatusLocacao.Ativa && locacao.Status != StatusLocacao.Atrasada)
            return Result.ValidationResult<DevolverLocacaoResponse>("Apenas locações ativas ou atrasadas podem ser devolvidas");

        if (request.DataDevolucao < locacao.DataRetirada.Date)
            return Result.ValidationResult<DevolverLocacaoResponse>("Data de devolução não pode ser anterior à data de retirada");

        var rows = await _repository.DevolverAsync(request.Id, request.DataDevolucao, request.Observacao, cancellationToken);
        if (rows == 0)
            return Result.NotFoundResult<DevolverLocacaoResponse>("Locação não encontrada");

        try
        {
            await _financeiroRepository.CriarLancamentoAsync(new FinanceiroCommand
            {
                LocacaoId = request.Id,
                DataDevolucao = request.DataDevolucao
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar lançamento financeiro para locação {LocacaoId}", request.Id);
        }

        return Result.OkResult(new DevolverLocacaoResponse { Mensagem = "Locação devolvida com sucesso" });
    }
}
