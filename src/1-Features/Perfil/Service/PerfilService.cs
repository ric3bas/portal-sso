using Portal.Domain.Base;
using Portal.Domain.Exceptions;

using Portal.Features.Escopo.Domain.Interfaces;
using Portal.Features.Perfil.Domain;
using Portal.Features.Perfil.Domain.Interfaces;
using Portal.Features.Perfil.Infra;
using static Portal.Domain.Base.Result;


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

        public async Task<Result<IEnumerable<PerfilComEscopoResponse>>> ListarComEscoposAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Listando perfis com escopos");  
            var result = await _repository.ListarComEscoposAsync(cancellationToken);
            if (!result.Any())  
                return NotFoundResult<IEnumerable<PerfilComEscopoResponse>>("Nenhum perfil encontrado");

            return OkResult(result.Select(c => c.ToResponse()));
        }

        public async Task<Result<string>> CriarAsync(string nome, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Criando perfil '{Nome}'", nome);

            if (string.IsNullOrWhiteSpace(nome))
                return ValidationResult<string>("Nome do perfil é obrigatório");

            var nomeNormalizado = nome.Trim();

            if (nomeNormalizado.Length < 3)
                return ValidationResult<string>("Nome do perfil deve ter no mínimo 3 caracteres");

            if (nomeNormalizado.Length > 100)
                return ValidationResult<string>("Nome do perfil deve ter no máximo 100 caracteres");

            var nomeExiste = await _repository.ExisteNomeAsync(nomeNormalizado, cancellationToken);
            if (nomeExiste)
                return BusinessResult<string>($"Já existe um perfil com o nome '{nome}'");

            var perfil = new PerfilCommand { Nome = nomeNormalizado };

            var id = await _repository.InserirAsync(perfil, cancellationToken);
            return OkResult("Criado com sucesso");
        }

        public async Task<Result<PerfilResponse>> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Obtendo perfil {Id}", id);

            if (id <= 0)
                return ValidationResult<PerfilResponse>("Id do perfil inválido");

            var perfil = await _repository.ObterPorIdAsync(id, cancellationToken);
            if (perfil is null)
                return NotFoundResult<PerfilResponse>($"Perfil {id} não encontrado");

            return OkResult(perfil.ToResponse());
        }

        public async Task<Result<string>> VincularEscoposAsync(int perfilId, IEnumerable<int> escopoIds, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Vinculando escopos ao perfil {PerfilId}", perfilId);

            if (perfilId <= 0)
                return ValidationResult<string>("PerfilId inválido");

            var ids = escopoIds?.Distinct().ToList();

            if (ids is null || ids.Count == 0)
                return ValidationResult<string>("Informe ao menos um EscopoId");

            if (ids.Any(id => id <= 0))
                return ValidationResult<string>("Todos os EscopoIds devem ser maiores que zero");

            var perfilExiste = await _repository.ExistePerfilAsync(perfilId, cancellationToken);
            if (!perfilExiste)
                return NotFoundResult<string>($"Perfil {perfilId} não encontrado");

            var idsExistentes = (await _escopoRepository.ObterIdsExistentesAsync(ids, cancellationToken)).ToHashSet();
            var idsInvalidos  = ids.Where(id => !idsExistentes.Contains(id)).ToList();

            if (idsInvalidos.Count > 0)
                return ValidationResult<string>($"Os seguintes EscopoIds não existem: {string.Join(", ", idsInvalidos)}");

            await _repository.VincularEscoposAsync(perfilId, ids, cancellationToken);

            return OkResult("Vinculado com sucesso");
        }
    }
}
