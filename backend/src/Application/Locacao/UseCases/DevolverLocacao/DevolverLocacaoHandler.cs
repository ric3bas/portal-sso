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
    private readonly ILogger<DevolverLocacaoHandler> _logger;

    public DevolverLocacaoHandler(
        ILocacaoRepository repository,
        IFinanceiroRepository financeiroRepository,
        ILogger<DevolverLocacaoHandler> logger)
    {
        _repository = repository;
        _financeiroRepository = financeiroRepository;
        _logger = logger;
    }

    public async Task<Result<DevolverLocacaoResponse>> Handle(DevolverLocacaoRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsValid()) return Result.ValidationResult<DevolverLocacaoResponse>(request.ObterErros());

        var locacao = await _repository.ObterPorIdAsync(request.Id, cancellationToken);
        if (locacao is null)
            return Result.NotFoundResult<DevolverLocacaoResponse>("LocaÃ§Ã£o nÃ£o encontrada");

        if (locacao.Status != StatusLocacao.Ativa && locacao.Status != StatusLocacao.Atrasada)
            return Result.ValidationResult<DevolverLocacaoResponse>("Apenas locaÃ§Ãµes ativas ou atrasadas podem ser devolvidas");

        if (request.DataDevolucao < locacao.DataRetirada.Date)
            return Result.ValidationResult<DevolverLocacaoResponse>("Data de devoluÃ§Ã£o nÃ£o pode ser anterior Ã  data de retirada");

        var rows = await _repository.DevolverAsync(request.Id, request.DataDevolucao, request.Observacao, cancellationToken);
        if (rows == 0)
            return Result.NotFoundResult<DevolverLocacaoResponse>("LocaÃ§Ã£o nÃ£o encontrada");

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
            _logger.LogError(ex, "Erro ao criar lanÃ§amento financeiro para locaÃ§Ã£o {LocacaoId}", request.Id);
        }

        return Result.OkResult(new DevolverLocacaoResponse { Mensagem = "LocaÃ§Ã£o devolvida com sucesso" });
    }
}
