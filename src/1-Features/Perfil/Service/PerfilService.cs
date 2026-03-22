using Portal.Domain.Exceptions;

using Portal.Features.Escopo.Domain.Interfaces;
using Portal.Features.Perfil.Domain;
using Portal.Features.Perfil.Domain.Interfaces;
using Portal.Features.Perfil.Infra;

namespace Portal.Features.Perfil.Service
{
    public class PerfilService : IPerfilService
    {
        private readonly IPerfilRepository _repository;
        private readonly IEscopoRepository _escopoRepository;
        private readonly ILogger<PerfilService> _logger;

        public PerfilService(IPerfilRepository repository, IEscopoRepository escopoRepository, ILogger<PerfilService> logger)
        {
            _repository      = repository;
            _escopoRepository = escopoRepository;
            _logger          = logger;
        }

        public async Task<IEnumerable<PerfilComEscopoResponse>> ListarComEscoposAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Listando perfis com escopos");
            var result = await _repository.ListarComEscoposAsync(cancellationToken);
            if (!result.Any())
                throw new NotFoundException("Nenhum perfil encontrado");
            return result.Select(c=>c.ToResponse());
        }

        public async Task<int> CriarAsync(string nome, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Criando perfil '{Nome}'", nome);

            if (string.IsNullOrWhiteSpace(nome))
                throw new ValidationException("Nome do perfil é obrigatório");

            if (nome.Trim().Length < 3)
                throw new ValidationException("Nome do perfil deve ter no mínimo 3 caracteres");

            if (nome.Trim().Length > 100)
                throw new ValidationException("Nome do perfil deve ter no máximo 100 caracteres");

            var nomeExiste = await _repository.ExisteNomeAsync(nome.Trim(), cancellationToken);
            if (nomeExiste)
                throw new BusinessException($"Já existe um perfil com o nome '{nome}'");

            var perfil = new PerfilCommand { Nome = nome.Trim() };

            return await _repository.InserirAsync(perfil, cancellationToken);
        }

        public async Task<PerfilResponse?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Obtendo perfil {Id}", id);

            if (id <= 0)
                throw new ValidationException("Id do perfil inválido");

            var perfil = await _repository.ObterPorIdAsync(id, cancellationToken);
            if (perfil is null)
                throw new NotFoundException($"Perfil {id} não encontrado");

            return perfil.ToResponse();
        }

        public async Task VincularEscoposAsync(int perfilId, IEnumerable<int> escopoIds, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Vinculando escopos ao perfil {PerfilId}", perfilId);

            if (perfilId <= 0)
                throw new ValidationException("PerfilId inválido");

            var ids = escopoIds?.Distinct().ToList();

            if (ids is null || ids.Count == 0)
                throw new ValidationException("Informe ao menos um EscopoId");

            if (ids.Any(id => id <= 0))
                throw new ValidationException("Todos os EscopoIds devem ser maiores que zero");

            var perfilExiste = await _repository.ExistePerfilAsync(perfilId, cancellationToken);
            if (!perfilExiste)
                throw new NotFoundException($"Perfil {perfilId} não encontrado");

            var idsExistentes = (await _escopoRepository.ObterIdsExistentesAsync(ids, cancellationToken)).ToHashSet();
            var idsInvalidos  = ids.Where(id => !idsExistentes.Contains(id)).ToList();

            if (idsInvalidos.Count > 0)
                throw new ValidationException($"Os seguintes EscopoIds não existem: {string.Join(", ", idsInvalidos)}");

            await _repository.VincularEscoposAsync(perfilId, ids, cancellationToken);
        }
    }
}
