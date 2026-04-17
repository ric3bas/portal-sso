using Portal.Domain.Base;
using Portal.Features.Categoria.Domain;
using Portal.Features.Categoria.Domain.Interfaces;
using Portal.Features.Categoria.Domain.Validations;
using static Portal.Domain.Base.Result;

namespace Portal.Features.Categoria.Service
{
    public class CategoriaService : BaseService, ICategoriaService
    {
        private readonly ICategoriaRepository _repository;
        private readonly ILogger<CategoriaService> _logger;

        public CategoriaService(
            ICategoriaRepository repository,
            ILogger<CategoriaService> logger,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<CategoriaResponse>>> ObterTodasCategoriasAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Listando todas as categorias");

            var usuario = ObterUsuario();
            var parceiroId = usuario.IsMaster ? Guid.Empty : usuario.ParceiroId;

            var result = usuario.IsMaster 
                ? await _repository.ObterTodasAsync(cancellationToken)
                : await _repository.ObterPorParceiroAsync(parceiroId, cancellationToken);

            if (result == null || !result.Any())
                return NotFoundResult<IEnumerable<CategoriaResponse>>("Nenhuma categoria encontrada");

            return OkResult(result.Select(c => c.ToResponse()));
        }

        public async Task<Result<IEnumerable<CategoriaResponse>>> ObterCategoriasPorFiltroAsync(string? nome, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Listando categorias com filtro de nome: {nome}");

            var usuario = ObterUsuario();
            var parceiroId = usuario.IsMaster ? Guid.Empty : usuario.ParceiroId;

            var result = usuario.IsMaster 
                ? await _repository.ObterPorFiltroAsync(nome ?? string.Empty, cancellationToken)
                : await _repository.ObterPorFiltroEParceiroAsync(parceiroId, nome ?? string.Empty, cancellationToken);

            if (result == null || !result.Any())
                return NotFoundResult<IEnumerable<CategoriaResponse>>("Nenhuma categoria encontrada");

            return OkResult(result.Select(c => c.ToResponse()));
        }

        public async Task<Result<CategoriaResponse>> ObterCategoriaAsync(string? id, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Obtendo categoria com Id: {id}");
            if (string.IsNullOrWhiteSpace(id)) return ValidationResult<CategoriaResponse>("Id da categoria inválido");

            if (!Guid.TryParse(id, out var categoriaId))
                return ValidationResult<CategoriaResponse>("Id da categoria inválido");

            var usuario = ObterUsuario();
            var parceiroId = usuario.IsMaster ? Guid.Empty : usuario.ParceiroId;

            var result = usuario.IsMaster 
                ? await _repository.ObterPorIdAsync(categoriaId, cancellationToken)
                : await _repository.ObterPorIdEParceiroAsync(categoriaId, parceiroId, cancellationToken);

            if (result == null)
                return NotFoundResult<CategoriaResponse>("Categoria não encontrada");

            return OkResult(result.ToResponse());
        }

        public async Task<Result<string>> CriarCategoriaAsync(CategoriaRequest categoria, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Criando categoria: {categoria.Nome}");

            var validator = new CategoriaRequestValidator();
            var validationResult = await validator.ValidateAsync(categoria, cancellationToken);
            if (!validationResult.IsValid)
                return ValidationResult<string>(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var usuario = ObterUsuario();
            var parceiroId = usuario.ParceiroId;

            // Verificar se já existe categoria com o mesmo nome para este parceiro
            if (await _repository.ExisteNomeAsync(categoria.Nome, parceiroId, null, cancellationToken))
                return ValidationResult<string>("Já existe uma categoria com este nome");

            var id = await _repository.CriarAsync(categoria.Nome, parceiroId, cancellationToken);
            return OkResult(id.ToString());
        }

        public async Task<Result<string>> AtualizarCategoriaAsync(AtualizarCategoriaRequest categoria, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Atualizando categoria: {categoria.Id}");

            var validator = new AtualizarCategoriaRequestValidator();
            var validationResult = await validator.ValidateAsync(categoria, cancellationToken);
            if (!validationResult.IsValid)
                return ValidationResult<string>(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            if (!Guid.TryParse(categoria.Id, out var categoriaId))
                return ValidationResult<string>("Id da categoria inválido");

            var usuario = ObterUsuario();
            var parceiroId = usuario.ParceiroId;

            if (!await _repository.ExisteAsync(categoriaId, cancellationToken))
                return NotFoundResult<string>("Categoria não encontrada");

            // Verificar se já existe categoria com o mesmo nome para este parceiro (exceto a atual)
            if (await _repository.ExisteNomeAsync(categoria.Nome, parceiroId, categoriaId, cancellationToken))
                return ValidationResult<string>("Já existe uma categoria com este nome");

            var linhasAfetadas = await _repository.AtualizarAsync(categoriaId, categoria.Nome, categoria.Ativo, parceiroId, cancellationToken);
            if (linhasAfetadas == 0)
                return NotFoundResult<string>("Categoria não encontrada");

            return OkResult("Categoria atualizada com sucesso");
        }

        public async Task<Result<string>> InativarCategoriaAsync(string? id, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Inativando categoria: {id}");

            if (string.IsNullOrWhiteSpace(id)) return ValidationResult<string>("Id da categoria inválido");

            if (!Guid.TryParse(id, out var categoriaId))
                return ValidationResult<string>("Id da categoria inválido");

            var usuario = ObterUsuario();
            var parceiroId = usuario.ParceiroId;

            if (!await _repository.ExisteAsync(categoriaId, cancellationToken))
                return NotFoundResult<string>("Categoria não encontrada");

            var linhasAfetadas = await _repository.InativarAsync(categoriaId, parceiroId, cancellationToken);
            if (linhasAfetadas == 0)
                return NotFoundResult<string>("Categoria não encontrada");

            return OkResult("Categoria inativada com sucesso");
        }
    }
}