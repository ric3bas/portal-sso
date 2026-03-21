using Portal.Dominio;
using Portal.Dominio.Validations;
using Portal.Features.Parceiro.Domain;
using Portal.Features.Parceiro.Domain.Interfaces;

namespace Portal.Features.Parceiro.Service {
    public class ParceiroService : BaseService, IParceiroService {
        private readonly IParceiroRepository _repository;
        private readonly Domain.Validations.ParceiroRequestValidator _validator;
        private readonly Domain.Validations.ParceiroIdRequestValidator _validatorId;
        private readonly ILogger<ParceiroService> _logger;

        public ParceiroService(
            IParceiroRepository repository,
            Domain.Validations.ParceiroRequestValidator validator,
            Domain.Validations.ParceiroIdRequestValidator validatorId,
            ILogger<ParceiroService> logger,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repository  = repository;
            _validator   = validator;
            _validatorId = validatorId;
            _logger      = logger;
        }
        public async Task<IEnumerable<ParceiroResponse>> ListarParceirosAsync(string? nome, CancellationToken cancellationToken){
            _logger.LogInformation("Listando parceiros com filtro de nome: {Nome}", nome);
            var result = await _repository.ObterTodosAsync(nome, cancellationToken);
            if (result == null || !result.Any())
                throw new NotFoundException("Nenhum parceiro encontrado");
            return result;
        }

        public async Task<ParceiroResponse?> ObterParceiroAsync(string? id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Obtendo parceiro com ID: {Id}", id);
            var validationResult = _validatorId.Validate(id ?? string.Empty);

            if (!validationResult.IsValid)
                throw new Dominio.Validations.ValidationException(
                    validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                );
            var result = await _repository.ObterPorIdAsync(Guid.Parse(id ?? string.Empty), cancellationToken);

            if (result == null)
                throw new NotFoundException("Parceiro não encontrado");

            return result;
        }

        public async Task<Guid> CriarParceiroAsync(ParceiroRequest parceiro, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Criando novo parceiro com nome: {Nome}", parceiro.Nome);

            if (!parceiro.IsValid())
                throw new ValidationException(parceiro.ObterErros());

            var existente = await _repository.ObterPorNomeAsync(parceiro.Nome!, cancellationToken);
            if (existente != null)
                throw new ValidationException(new List<string> { "Já existe um parceiro com este nome." });

            var entity = parceiro.ToEntity(ObterTenantId());

            // Transação/commit agora é responsabilidade do repositório, se necessário
            var id = await _repository.InserirAsync(entity, cancellationToken);
            return id;
        }

        public async Task AtualizarParceiroAsync(AtualizarParceiroRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Atualizando parceiro {Id}", request.Id);

            if (!request.IsValid())
                throw new ValidationException(request.ObterErros());

            var idParceiro = Guid.Parse(request.Id ?? string.Empty);

            // 1 única chamada ao banco: verifica existência + conflito de nome
            var (existe, nomeConflito) = await _repository.ValidarAtualizacaoAsync(idParceiro, request.Nome, cancellationToken);

            if (!existe)
                throw new NotFoundException("Parceiro não encontrado");

            if (nomeConflito)
                throw new ValidationException(new List<string> { "Já existe outro parceiro com este nome." });

            var entity = new Portal.Domain.Entities.ParceiroEntity
            {
                Id        = idParceiro,
                Nome      = request.Nome,
                Descricao = request.Descricao,
                Ativo     = request.Ativo
            };

            await _repository.AtualizarAsync(entity, cancellationToken);
        }
    }
}