using Portal.Domain.Base;
using Portal.Features.Cliente.Domain;
using Portal.Features.Cliente.Domain.Interfaces;
using Portal.Features.Cliente.Domain.Validations;
using static Portal.Domain.Base.Result;

namespace Portal.Features.Cliente.Service
{
    public class ClienteService : BaseService, IClienteService
    {
        private readonly IClienteRepository _repository;
        private readonly ILogger<ClienteService> _logger;

        public ClienteService(
            IClienteRepository repository,
            ILogger<ClienteService> logger,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<ClienteResponse>>> ObterTodosClientesAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Listando todos os clientes");

            var result = await _repository.ObterTodosAsync(cancellationToken);
            if (result == null || !result.Any())
                return NotFoundResult<IEnumerable<ClienteResponse>>("Nenhum cliente encontrado");

            return OkResult(result.Select(c => c.ToResponse()));
        }

        public async Task<Result<IEnumerable<ClienteResponse>>> ObterClientesPorFiltroAsync(FiltroClienteRequest filtro, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Listando clientes com filtros");

            var result = await _repository.ObterPorFiltroAsync(filtro, cancellationToken);
            if (result == null || !result.Any())
                return NotFoundResult<IEnumerable<ClienteResponse>>("Nenhum cliente encontrado");

            return OkResult(result.Select(c => c.ToResponse()));
        }

        public async Task<Result<ClienteResponse>> ObterClienteAsync(string? id, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Obtendo cliente com Id: {id}");
            if (string.IsNullOrWhiteSpace(id)) return ValidationResult<ClienteResponse>("Id do cliente inválido");

            if (!Guid.TryParse(id, out var clienteId))
                return ValidationResult<ClienteResponse>("Id do cliente inválido");

            var result = await _repository.ObterPorIdAsync(clienteId, cancellationToken);
            if (result == null)
                return NotFoundResult<ClienteResponse>("Cliente não encontrado");

            return OkResult(result.ToResponse());
        }

        public async Task<Result<string>> CriarClienteAsync(ClienteRequest cliente, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Criando cliente: {cliente.Nome}");

            var validator = new ClienteRequestValidator();
            var validationResult = await validator.ValidateAsync(cliente, cancellationToken);
            if (!validationResult.IsValid)
                return ValidationResult<string>(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            // Verificar se já existe cliente com o mesmo CPF
            if (await _repository.ExisteCpfAsync(cliente.Cpf, null, cancellationToken))
                return ValidationResult<string>("Já existe um cliente com este CPF");

            // Verificar se já existe cliente com o mesmo email
            if (await _repository.ExisteEmailAsync(cliente.Email, null, cancellationToken))
                return ValidationResult<string>("Já existe um cliente com este email");

            var id = await _repository.CriarAsync(cliente, cancellationToken);
            return OkResult(id.ToString());
        }

        public async Task<Result<string>> AtualizarClienteAsync(AtualizarClienteRequest cliente, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Atualizando cliente: {cliente.Id}");

            var validator = new AtualizarClienteRequestValidator();
            var validationResult = await validator.ValidateAsync(cliente, cancellationToken);
            if (!validationResult.IsValid)
                return ValidationResult<string>(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            if (!Guid.TryParse(cliente.Id, out var clienteId))
                return ValidationResult<string>("Id do cliente inválido");

            if (!await _repository.ExisteAsync(clienteId, cancellationToken))
                return NotFoundResult<string>("Cliente não encontrado");

            // Verificar se já existe cliente com o mesmo CPF (exceto o atual)
            if (await _repository.ExisteCpfAsync(cliente.Cpf, clienteId, cancellationToken))
                return ValidationResult<string>("Já existe um cliente com este CPF");

            // Verificar se já existe cliente com o mesmo email (exceto o atual)
            if (await _repository.ExisteEmailAsync(cliente.Email, clienteId, cancellationToken))
                return ValidationResult<string>("Já existe um cliente com este email");

            var linhasAfetadas = await _repository.AtualizarAsync(clienteId, cliente, cancellationToken);
            if (linhasAfetadas == 0)
                return NotFoundResult<string>("Cliente não encontrado");

            return OkResult("Cliente atualizado com sucesso");
        }

        public async Task<Result<string>> BloquearClienteAsync(string? id, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Bloqueando cliente: {id}");

            if (string.IsNullOrWhiteSpace(id)) return ValidationResult<string>("Id do cliente inválido");

            if (!Guid.TryParse(id, out var clienteId))
                return ValidationResult<string>("Id do cliente inválido");

            if (!await _repository.ExisteAsync(clienteId, cancellationToken))
                return NotFoundResult<string>("Cliente não encontrado");

            var linhasAfetadas = await _repository.BloquearAsync(clienteId, cancellationToken);
            if (linhasAfetadas == 0)
                return NotFoundResult<string>("Cliente não encontrado");

            return OkResult("Cliente bloqueado com sucesso");
        }

        public async Task<Result<string>> DesbloquearClienteAsync(string? id, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Desbloqueando cliente: {id}");

            if (string.IsNullOrWhiteSpace(id)) return ValidationResult<string>("Id do cliente inválido");

            if (!Guid.TryParse(id, out var clienteId))
                return ValidationResult<string>("Id do cliente inválido");

            if (!await _repository.ExisteAsync(clienteId, cancellationToken))
                return NotFoundResult<string>("Cliente não encontrado");

            var linhasAfetadas = await _repository.DesbloquearAsync(clienteId, cancellationToken);
            if (linhasAfetadas == 0)
                return NotFoundResult<string>("Cliente não encontrado");

            return OkResult("Cliente desbloqueado com sucesso");
        }

        public async Task<Result<string>> InativarClienteAsync(string? id, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Inativando cliente: {id}");

            if (string.IsNullOrWhiteSpace(id)) return ValidationResult<string>("Id do cliente inválido");

            if (!Guid.TryParse(id, out var clienteId))
                return ValidationResult<string>("Id do cliente inválido");

            if (!await _repository.ExisteAsync(clienteId, cancellationToken))
                return NotFoundResult<string>("Cliente não encontrado");

            var linhasAfetadas = await _repository.InativarAsync(clienteId, cancellationToken);
            if (linhasAfetadas == 0)
                return NotFoundResult<string>("Cliente não encontrado");

            return OkResult("Cliente inativado com sucesso");
        }
    }
}