using Portal.Domain.Exceptions;
using Portal.Features.Escopo.Domain;
using Portal.Features.Escopo.Domain.Interfaces;
using Portal.Features.Escopo.Infra;

namespace Portal.Features.Escopo.Service
{
    public class EscopoService : IEscopoService
    {
        private readonly IEscopoRepository _repository;
        private readonly ILogger<EscopoService> _logger;

        public EscopoService(IEscopoRepository repository, ILogger<EscopoService> logger)
        {
            _repository = repository;
            _logger     = logger;
        }

        public async Task<IEnumerable<EscopoResponse>> ListarAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Listando escopos");
            var result = await _repository.ListarAsync(cancellationToken);
            if (!result.Any())
                throw new NotFoundException("Nenhum escopo encontrado");
            return result.Select(c=>c.ToResponse());
        }

        public async Task<int> CriarAsync(string nome, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Criando escopo '{Nome}'", nome);

            if (string.IsNullOrWhiteSpace(nome))
                throw new ValidationException("Nome do escopo é obrigatório");

            if (nome.Trim().Length < 3)
                throw new ValidationException("Nome do escopo deve ter no mínimo 3 caracteres");

            if (nome.Trim().Length > 100)
                throw new ValidationException("Nome do escopo deve ter no máximo 100 caracteres");

            var nomeExiste = await _repository.ExisteNomeAsync(nome.Trim(), cancellationToken);
            if (nomeExiste)
                throw new BusinessException($"Já existe um escopo com o nome '{nome}'");

            var escopo = new EscopoCommand { Nome = nome.Trim() };
            return await _repository.InserirAsync(escopo, cancellationToken);
        }

        public async Task<EscopoResponse?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Obtendo escopo {Id}", id);

            if (id <= 0)
                throw new ValidationException("Id do escopo inválido");

            var escopo = await _repository.ObterPorIdAsync(id, cancellationToken);
            if (escopo is null)
                throw new NotFoundException($"Escopo {id} não encontrado");

            return escopo.ToResponse();
        }
    }
}
