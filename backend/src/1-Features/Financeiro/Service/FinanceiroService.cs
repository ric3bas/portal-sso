using Portal.Domain.Base;
using Portal.Features.Financeiro.Domain;
using Portal.Features.Financeiro.Domain.Interfaces;
using static Portal.Domain.Base.Result;

namespace Portal.Features.Financeiro.Service
{
    public class FinanceiroService : BaseService, IFinanceiroService
    {
        private readonly IFinanceiroRepository _repository;
        private readonly ILogger<FinanceiroService> _logger;

        public FinanceiroService(
            IFinanceiroRepository repository,
            ILogger<FinanceiroService> logger,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<FinanceiroResponse>>> ObterTodosLancamentosAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Listando todos os lançamentos financeiros");

            var result = await _repository.ObterTodosAsync(cancellationToken);
            if (result == null || !result.Any())
                return NotFoundResult<IEnumerable<FinanceiroResponse>>("Nenhum lançamento financeiro encontrado");

            return OkResult(result.Select(f => f.ToResponse()));
        }

        public async Task<Result<IEnumerable<FinanceiroResponse>>> ObterLancamentosPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Listando lançamentos financeiros de {dataInicio:dd/MM/yyyy} até {dataFim:dd/MM/yyyy}");

            if (dataInicio > dataFim)
                return ValidationResult<IEnumerable<FinanceiroResponse>>("Data início deve ser anterior à data fim");

            var result = await _repository.ObterPorPeriodoAsync(dataInicio.Date, dataFim.Date.AddDays(1).AddTicks(-1), cancellationToken);
            if (result == null || !result.Any())
                return NotFoundResult<IEnumerable<FinanceiroResponse>>("Nenhum lançamento financeiro encontrado no período");

            return OkResult(result.Select(f => f.ToResponse()));
        }
    }
}