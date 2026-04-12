using Microsoft.AspNetCore.Http.HttpResults;
using Portal.Domain.Base;
using Portal.Features.Escopo.Domain;
using Portal.Features.Parceiro.Domain;
using Portal.Features.Parceiro.Domain.Interfaces;
using static Portal.Domain.Base.Result;

namespace Portal.Features.Parceiro.Service {
    public class ParceiroService : BaseService, IParceiroService {
        private readonly IParceiroRepository _repository;
        private readonly ILogger<ParceiroService> _logger;

        public ParceiroService(
            IParceiroRepository repository,
            ILogger<ParceiroService> logger,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repository  = repository;
            _logger      = logger;
        }
        public async Task<Result<IEnumerable<ParceiroResponse>>> ObterTodosParceirosAsync(CancellationToken cancellationToken){
            _logger.LogInformation($"Listando parceiros com filtro de nome");

            var usuario = ObterUsuario();
            var parceiroId = usuario.IsMaster ? Guid.Empty : ObterUsuario().ParceiroId;

            var result = await _repository.ObterTodosAsync(parceiroId, cancellationToken);
            if (result == null || !result.Any()) 
                return NotFoundResult<IEnumerable<ParceiroResponse>>("Nenhum parceiro encontrado");
                
            return OkResult(result.Select(c=>c.ToResponse()));
        }

        public async Task<Result<IEnumerable<ParceiroResponse>>> ObterTodosPorFiltroAsync(string? nome, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Listando parceiros com filtro de nome: {nome}");

            var result = await _repository.ObterTodosPorFiltroAsync(nome ?? string.Empty, cancellationToken);
            if (result == null || !result.Any())
                return NotFoundResult<IEnumerable<ParceiroResponse>>("Nenhum parceiro encontrado");

            return OkResult(result.Select(c => c.ToResponse()));
        }

        public async Task<Result<ParceiroResponse>> ObterParceiroAsync(string? id, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Obtendo parceiro com Id: {id}");
            if (string.IsNullOrWhiteSpace(id)) return ValidationResult<ParceiroResponse>("Id do escopo inválido");

            var result = await _repository.ObterPorIdAsync(Guid.Parse(id ?? string.Empty), cancellationToken);

            if (result == null)
                return NotFoundResult<ParceiroResponse>("Parceiro não encontrado");

            return OkResult(result.ToResponse());
        }

        public async Task<Result<string>> CriarParceiroAsync(ParceiroRequest parceiro, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Criando novo parceiro com nome: {parceiro.Nome}");

            if (!parceiro.IsValid()) return ValidationResult<string>(parceiro.ObterErros());

            var existente = await _repository.ObterPorNomeAsync(parceiro.Nome!, cancellationToken);
            if (existente != null) return ValidationResult<string>("Já existe um parceiro com este nome");

            var entity = parceiro.ToCommand(Guid.NewGuid());

            var id = await _repository.InserirAsync(entity, cancellationToken);
            return OkResult("Criado com sucesso");
        }

        public async Task<Result<string>> AtualizarParceiroAsync(AtualizarParceiroRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Atualizando parceiro {request.Id}");

            if (!request.IsValid())
                return ValidationResult<string>(request.ObterErros());

            var idParceiro = Guid.Parse(request.Id ?? string.Empty);

            var (existe, nomeConflito) = await _repository.ValidarAtualizacaoAsync(idParceiro, request.Nome, cancellationToken);

            if (!existe) return NotFoundResult<string>("Parceiro não encontrado");

            if (nomeConflito) return ValidationResult<string>("Já existe outro parceiro com este nome");

            await _repository.AtualizarAsync(request.ToCommand(idParceiro), cancellationToken);
            return OkResult("Atualizado com sucesso");
        }
    }
}