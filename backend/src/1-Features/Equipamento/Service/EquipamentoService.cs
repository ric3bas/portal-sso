using Portal.Domain.Base;
using Portal.Features.Equipamento.Domain;
using Portal.Features.Equipamento.Domain.Interfaces;
using Portal.Features.Equipamento.Domain.Validations;
using static Portal.Domain.Base.Result;

namespace Portal.Features.Equipamento.Service
{
    public class EquipamentoService : BaseService, IEquipamentoService
    {
        private readonly IEquipamentoRepository _repository;
        private readonly ILogger<EquipamentoService> _logger;

        public EquipamentoService(
            IEquipamentoRepository repository,
            ILogger<EquipamentoService> logger,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<EquipamentoResponse>>> ObterTodosEquipamentosAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Listando todos os equipamentos");

            var result = await _repository.ObterTodosAsync(cancellationToken);
            if (result == null || !result.Any())
                return NotFoundResult<IEnumerable<EquipamentoResponse>>("Nenhum equipamento encontrado");

            return OkResult(result.Select(e => e.ToResponse()));
        }

        public async Task<Result<IEnumerable<EquipamentoResponse>>> ObterEquipamentosPorFiltroAsync(FiltroEquipamentoRequest filtro, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Listando equipamentos com filtros");

            var result = await _repository.ObterPorFiltroAsync(filtro, cancellationToken);
            if (result == null || !result.Any())
                return NotFoundResult<IEnumerable<EquipamentoResponse>>("Nenhum equipamento encontrado");

            return OkResult(result.Select(e => e.ToResponse()));
        }

        public async Task<Result<EquipamentoResponse>> ObterEquipamentoAsync(string? id, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Obtendo equipamento com Id: {id}");
            if (string.IsNullOrWhiteSpace(id)) return ValidationResult<EquipamentoResponse>("Id do equipamento inválido");

            if (!Guid.TryParse(id, out var equipamentoId))
                return ValidationResult<EquipamentoResponse>("Id do equipamento inválido");

            var result = await _repository.ObterPorIdAsync(equipamentoId, cancellationToken);
            if (result == null)
                return NotFoundResult<EquipamentoResponse>("Equipamento não encontrado");

            return OkResult(result.ToResponse());
        }

        public async Task<Result<string>> CriarEquipamentoAsync(EquipamentoRequest equipamento, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Criando equipamento: {equipamento.Nome}");

            var validator = new EquipamentoRequestValidator();
            var validationResult = await validator.ValidateAsync(equipamento, cancellationToken);
            if (!validationResult.IsValid)
                return ValidationResult<string>(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            // Verificar se a categoria existe
            if (!Guid.TryParse(equipamento.CategoriaId, out var categoriaId))
                return ValidationResult<string>("Categoria inválida");

            if (!await _repository.CategoriaExisteAsync(categoriaId, cancellationToken))
                return ValidationResult<string>("Categoria não encontrada");

            // Verificar se já existe equipamento com o mesmo número de série
            if (await _repository.ExisteNumeroSerieAsync(equipamento.NumeroSerie, null, cancellationToken))
                return ValidationResult<string>("Já existe um equipamento com este número de série");

            var id = await _repository.CriarAsync(equipamento, cancellationToken);
            return OkResult(id.ToString());
        }

        public async Task<Result<string>> AtualizarEquipamentoAsync(AtualizarEquipamentoRequest equipamento, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Atualizando equipamento: {equipamento.Id}");

            var validator = new AtualizarEquipamentoRequestValidator();
            var validationResult = await validator.ValidateAsync(equipamento, cancellationToken);
            if (!validationResult.IsValid)
                return ValidationResult<string>(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            if (!Guid.TryParse(equipamento.Id, out var equipamentoId))
                return ValidationResult<string>("Id do equipamento inválido");

            if (!await _repository.ExisteAsync(equipamentoId, cancellationToken))
                return NotFoundResult<string>("Equipamento não encontrado");

            // Verificar se a categoria existe
            if (!Guid.TryParse(equipamento.CategoriaId, out var categoriaId))
                return ValidationResult<string>("Categoria inválida");

            if (!await _repository.CategoriaExisteAsync(categoriaId, cancellationToken))
                return ValidationResult<string>("Categoria não encontrada");

            // Verificar se já existe equipamento com o mesmo número de série (exceto o atual)
            if (await _repository.ExisteNumeroSerieAsync(equipamento.NumeroSerie, equipamentoId, cancellationToken))
                return ValidationResult<string>("Já existe um equipamento com este número de série");

            var linhasAfetadas = await _repository.AtualizarAsync(equipamentoId, equipamento, cancellationToken);
            if (linhasAfetadas == 0)
                return NotFoundResult<string>("Equipamento não encontrado");

            return OkResult("Equipamento atualizado com sucesso");
        }

        public async Task<Result<string>> InativarEquipamentoAsync(string? id, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Inativando equipamento: {id}");

            if (string.IsNullOrWhiteSpace(id)) return ValidationResult<string>("Id do equipamento inválido");

            if (!Guid.TryParse(id, out var equipamentoId))
                return ValidationResult<string>("Id do equipamento inválido");

            if (!await _repository.ExisteAsync(equipamentoId, cancellationToken))
                return NotFoundResult<string>("Equipamento não encontrado");

            var linhasAfetadas = await _repository.InativarAsync(equipamentoId, cancellationToken);
            if (linhasAfetadas == 0)
                return NotFoundResult<string>("Equipamento não encontrado");

            return OkResult("Equipamento inativado com sucesso");
        }
    }
}