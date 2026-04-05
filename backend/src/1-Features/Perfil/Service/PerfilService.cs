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
            _repository = repository;
            _escopoRepository = escopoRepository;
            _logger = logger;
        }

        public async Task<Result<string>> ApagarAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Apagando perfil {id}");
            var perfilExiste = await _repository.ExistePerfilAsync(id, cancellationToken);
            if (!perfilExiste)
                return NotFoundResult<string>($"Perfil {id} não encontrado");
            await _repository.DeletarAsync(id, cancellationToken);
            return OkResult("Apagado com sucesso");
        }

        public async Task<Result<string>> ClonarAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Clonando perfil {id}");
            var perfil = await _repository.ObterPorIdAsync(id, cancellationToken);
            if (perfil is null)
                return NotFoundResult<string>($"Perfil {id} não encontrado");

            // Cria novo perfil com nome modificado
            var novoPerfil = new PerfilCommand { Nome = perfil.Nome + " (Cópia)" };
            var novoPerfilId = await _repository.InserirAsync(novoPerfil, cancellationToken);

            // Vincula os mesmos escopos
            var escopoIds = perfil.Escopos.Select(e => e.Id);
            await _repository.VincularEscoposAsync(novoPerfilId, escopoIds, cancellationToken);

            return OkResult("Clonado com sucesso");
        }
    
       

        public async Task<Result<IEnumerable<PerfilComEscopoResponse>>> ListarComEscoposAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Listando perfis com escopos");  
            var result = await _repository.ListarComEscoposAsync(cancellationToken);
            if (!result.Any())  
                return NotFoundResult<IEnumerable<PerfilComEscopoResponse>>("Nenhum perfil encontrado");

            return OkResult(result.Select(c => c.ToResponse()));
        }

        public async Task<Result<string>> CriarAsync(PerfilRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Criando perfil {request.Nome}");

            if (!request.IsValid())
                return ValidationResult<string>(request.ObterErros());

            var nomeExiste = await _repository.ExisteNomeAsync(request.Nome, cancellationToken);
            if (nomeExiste)
                return BusinessResult<string>($"Já existe um perfil com o nome {request.Nome}");

            var id = await _repository.InserirAsync(request.ToCommand(), cancellationToken);
            return OkResult("Criado com sucesso");
        }

        public async Task<Result<PerfilComEscopoResponse>> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Obtendo perfil {id}");

            if (id <= 0) return ValidationResult<PerfilComEscopoResponse>("Id do perfil inválido");

            var perfil = await _repository.ObterPorIdAsync(id, cancellationToken);
            if (perfil is null)
                return NotFoundResult<PerfilComEscopoResponse>($"Perfil {id} não encontrado");

            return OkResult(perfil.ToResponse());
        }

        public async Task<Result<string>> VincularEscoposAsync(int perfilId, IEnumerable<int> escopoIds, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Vinculando escopos ao perfil {PerfilId}", perfilId);

            if (perfilId <= 0)
                return ValidationResult<string>("PerfilId inválido");

            var ids = escopoIds?.Distinct().ToList() ?? [];

            var perfilExiste = await _repository.ExistePerfilAsync(perfilId, cancellationToken);
            if (!perfilExiste)
                return NotFoundResult<string>($"Perfil {perfilId} não encontrado");

            var idsExistentes = (await _escopoRepository.ObterIdsExistentesAsync(ids, cancellationToken)).ToHashSet();
            var idsInvalidos  = ids.Except(idsExistentes).ToList();

            if (idsInvalidos.Count > 0)
                return ValidationResult<string>($"Os seguintes EscopoIds não existem: {string.Join(", ", idsInvalidos)}");

            await _repository.VincularEscoposAsync(perfilId, ids, cancellationToken);

            return OkResult("Vinculado com sucesso");
        }

        public async Task<Result<string>> AtualizarNomeAsync(int id, string novoNome, CancellationToken cancellationToken = default)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(novoNome))
                return ValidationResult<string>("Id ou nome inválido");

            var perfilExiste = await _repository.ExistePerfilAsync(id, cancellationToken);
            if (!perfilExiste)
                return NotFoundResult<string>($"Perfil {id} não encontrado");

            await _repository.AtualizarNomeAsync(id, novoNome, cancellationToken);
            return OkResult("Alterado com sucesso");
        }
    }
}
