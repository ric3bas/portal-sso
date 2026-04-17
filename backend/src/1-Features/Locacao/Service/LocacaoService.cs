using Portal.Domain.Base;
using Portal.Features.Locacao.Domain;
using Portal.Features.Locacao.Domain.Interfaces;
using Portal.Features.Locacao.Domain.Validations;
using Portal.Features.Financeiro.Domain.Interfaces;
using static Portal.Domain.Base.Result;

namespace Portal.Features.Locacao.Service
{
    public class LocacaoService : BaseService, ILocacaoService
    {
        private readonly ILocacaoRepository _repository;
        private readonly IFinanceiroRepository _financeiroRepository;
        private readonly ILogger<LocacaoService> _logger;

        public LocacaoService(
            ILocacaoRepository repository,
            IFinanceiroRepository financeiroRepository,
            ILogger<LocacaoService> logger,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repository = repository;
            _financeiroRepository = financeiroRepository;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<LocacaoResponse>>> ObterTodasLocacoesAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Listando todas as locações");
            
            // Atualizar status das locações atrasadas antes de retornar
            //await _repository.AtualizarStatusAtrasadasAsync(cancellationToken);

            var result = await _repository.ObterTodasAsync(cancellationToken);
            if (result == null || !result.Any())
                return NotFoundResult<IEnumerable<LocacaoResponse>>("Nenhuma locação encontrada");

            return OkResult(result.Select(l => l.ToResponse()));
        }

        public async Task<Result<IEnumerable<LocacaoResponse>>> ObterLocacoesPorFiltroAsync(FiltroLocacaoRequest filtro, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Listando locações com filtros");

            // Atualizar status das locações atrasadas antes de retornar
            await _repository.AtualizarStatusAtrasadasAsync(cancellationToken);

            var result = await _repository.ObterPorFiltroAsync(filtro, cancellationToken);
            if (result == null || !result.Any())
                return NotFoundResult<IEnumerable<LocacaoResponse>>("Nenhuma locação encontrada");

            return OkResult(result.Select(l => l.ToResponse()));
        }

        public async Task<Result<LocacaoResponse>> ObterLocacaoAsync(string? id, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Obtendo locação com Id: {id}");
            if (string.IsNullOrWhiteSpace(id)) return ValidationResult<LocacaoResponse>("Id da locação inválido");

            if (!Guid.TryParse(id, out var locacaoId))
                return ValidationResult<LocacaoResponse>("Id da locação inválido");

            var result = await _repository.ObterPorIdAsync(locacaoId, cancellationToken);
            if (result == null)
                return NotFoundResult<LocacaoResponse>("Locação não encontrada");

            return OkResult(result.ToResponse());
        }

        public async Task<Result<string>> CriarLocacaoAsync(LocacaoRequest locacao, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Criando locação para cliente: {locacao.ClienteId}");

            var validator = new LocacaoRequestValidator();
            var validationResult = await validator.ValidateAsync(locacao, cancellationToken);
            if (!validationResult.IsValid)
                return ValidationResult<string>(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            // Verificar se cliente e equipamento existem
            if (!Guid.TryParse(locacao.ClienteId, out var clienteId))
                return ValidationResult<string>("Cliente inválido");

            if (!Guid.TryParse(locacao.EquipamentoId, out var equipamentoId))
                return ValidationResult<string>("Equipamento inválido");

            if (!await _repository.ClienteExisteAsync(clienteId, cancellationToken))
                return ValidationResult<string>("Cliente não encontrado");

            if (!await _repository.EquipamentoExisteAsync(equipamentoId, cancellationToken))
                return ValidationResult<string>("Equipamento não encontrado");

            // Verificar se cliente não está bloqueado
            if (await _repository.ClienteBloqueadoAsync(clienteId, cancellationToken))
                return ValidationResult<string>("Cliente está bloqueado e não pode fazer locações");

            // Verificar se equipamento está disponível no período
            if (!await _repository.EquipamentoDisponivelAsync(equipamentoId, locacao.DataRetirada, locacao.PrevisaoDevolucao, null, cancellationToken))
                return ValidationResult<string>("Equipamento não está disponível no período solicitado");

            // Verificar se o valor da diária está correto
            var valorDiariaEquipamento = await _repository.ObterValorDiariaEquipamentoAsync(equipamentoId, cancellationToken);
            if (Math.Abs(locacao.ValorDiaria - valorDiariaEquipamento) > 0.01m) // tolerância para precisão decimal
                return ValidationResult<string>($"Valor da diária informado ({locacao.ValorDiaria:C}) não confere com o valor do equipamento ({valorDiariaEquipamento:C})");

            var id = await _repository.CriarAsync(locacao, cancellationToken);
            return OkResult(id.ToString());
        }

        public async Task<Result<string>> AtualizarLocacaoAsync(AtualizarLocacaoRequest locacao, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Atualizando locação: {locacao.Id}");

            var validator = new AtualizarLocacaoRequestValidator();
            var validationResult = await validator.ValidateAsync(locacao, cancellationToken);
            if (!validationResult.IsValid)
                return ValidationResult<string>(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            if (!Guid.TryParse(locacao.Id, out var locacaoId))
                return ValidationResult<string>("Id da locação inválido");

            if (!await _repository.ExisteAsync(locacaoId, cancellationToken))
                return NotFoundResult<string>("Locação não encontrada");

            // Verificar se a locação pode ser alterada (só pode alterar se estiver ativa)
            var locacaoAtual = await _repository.ObterPorIdAsync(locacaoId, cancellationToken);
            if (locacaoAtual?.Status != StatusLocacao.Ativa)
                return ValidationResult<string>("Apenas locações ativas podem ser alteradas");

            // Verificar se cliente e equipamento existem
            if (!Guid.TryParse(locacao.ClienteId, out var clienteId))
                return ValidationResult<string>("Cliente inválido");

            if (!Guid.TryParse(locacao.EquipamentoId, out var equipamentoId))
                return ValidationResult<string>("Equipamento inválido");

            if (!await _repository.ClienteExisteAsync(clienteId, cancellationToken))
                return ValidationResult<string>("Cliente não encontrado");

            if (!await _repository.EquipamentoExisteAsync(equipamentoId, cancellationToken))
                return ValidationResult<string>("Equipamento não encontrado");

            // Verificar se cliente não está bloqueado
            if (await _repository.ClienteBloqueadoAsync(clienteId, cancellationToken))
                return ValidationResult<string>("Cliente está bloqueado");

            // Verificar se equipamento está disponível no período (ignorando a locação atual)
            if (!await _repository.EquipamentoDisponivelAsync(equipamentoId, locacao.DataRetirada, locacao.PrevisaoDevolucao, locacaoId, cancellationToken))
                return ValidationResult<string>("Equipamento não está disponível no período solicitado");

            var linhasAfetadas = await _repository.AtualizarAsync(locacaoId, locacao, cancellationToken);
            if (linhasAfetadas == 0)
                return NotFoundResult<string>("Locação não encontrada");

            return OkResult("Locação atualizada com sucesso");
        }

        public async Task<Result<string>> DevolverLocacaoAsync(DevolverLocacaoRequest devolucao, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Devolvendo locação: {devolucao.Id}");

            var validator = new DevolverLocacaoRequestValidator();
            var validationResult = await validator.ValidateAsync(devolucao, cancellationToken);
            if (!validationResult.IsValid)
                return ValidationResult<string>(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            if (!Guid.TryParse(devolucao.Id, out var locacaoId))
                return ValidationResult<string>("Id da locação inválido");

            var locacao = await _repository.ObterPorIdAsync(locacaoId, cancellationToken);
            if (locacao == null)
                return NotFoundResult<string>("Locação não encontrada");

            // Verificar se a locação pode ser devolvida
            if (locacao.Status != StatusLocacao.Ativa && locacao.Status != StatusLocacao.Atrasada)
                return ValidationResult<string>("Apenas locações ativas ou atrasadas podem ser devolvidas");

            // Verificar se a data de devolução não é anterior à data de retirada
            if (devolucao.DataDevolucao < locacao.DataRetirada.Date)
                return ValidationResult<string>("Data de devolução não pode ser anterior à data de retirada");

            var linhasAfetadas = await _repository.DevolverAsync(locacaoId, devolucao.DataDevolucao, devolucao.Observacao, cancellationToken);
            if (linhasAfetadas == 0)
                return NotFoundResult<string>("Locação não encontrada");

            // Criar lançamento financeiro
            try
            {
                await _financeiroRepository.CriarLancamentoAsync(locacaoId, devolucao.DataDevolucao, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar lançamento financeiro para locação {LocacaoId}", locacaoId);
                // Continue com a devolução mesmo se houver erro no financeiro
            }

            return OkResult("Locação devolvida com sucesso");
        }

        public async Task<Result<string>> CancelarLocacaoAsync(string? id, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Cancelando locação: {id}");

            if (string.IsNullOrWhiteSpace(id)) return ValidationResult<string>("Id da locação inválido");

            if (!Guid.TryParse(id, out var locacaoId))
                return ValidationResult<string>("Id da locação inválido");

            var locacao = await _repository.ObterPorIdAsync(locacaoId, cancellationToken);
            if (locacao == null)
                return NotFoundResult<string>("Locação não encontrada");

            // Verificar se a locação pode ser cancelada
            if (locacao.Status == StatusLocacao.Devolvida)
                return ValidationResult<string>("Locações já devolvidas não podem ser canceladas");

            if (locacao.Status == StatusLocacao.Cancelada)
                return ValidationResult<string>("Locação já está cancelada");

            var linhasAfetadas = await _repository.CancelarAsync(locacaoId, cancellationToken);
            if (linhasAfetadas == 0)
                return NotFoundResult<string>("Locação não encontrada");

            return OkResult("Locação cancelada com sucesso");
        }

        public async Task<Result<IEnumerable<LocacaoResponse>>> ObterLocacoesAtrasadasAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Listando locações atrasadas");

            // Atualizar status das locações atrasadas antes de retornar
            await _repository.AtualizarStatusAtrasadasAsync(cancellationToken);

            var result = await _repository.ObterAtrasadasAsync(cancellationToken);
            if (result == null || !result.Any())
                return NotFoundResult<IEnumerable<LocacaoResponse>>("Nenhuma locação atrasada encontrada");

            return OkResult(result.Select(l => l.ToResponse()));
        }
    }
}