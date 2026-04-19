using Portal.Domain.Base;
using Portal.Domain.Categoria.Interfaces;

namespace Portal.Application.Categoria.UseCases.InativarCategoria;

public class InativarCategoriaHandler : BaseService
{
    private readonly ICategoriaRepository _repository;
    private readonly ILogger<InativarCategoriaHandler> _logger;

    public InativarCategoriaHandler(
        ICategoriaRepository repository,
        ILogger<InativarCategoriaHandler> logger,
        IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<InativarCategoriaResponse>> Handle(InativarCategoriaRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Inativando categoria {Id}", request.Id);

        var usuario = ObterUsuario();
        var parceiroId = usuario.IsMaster ? Guid.Empty : usuario.ParceiroId;

        // Verificar se categoria existe
        var categoriaExistente = parceiroId == Guid.Empty 
            ? await _repository.ObterPorIdAsync(request.Id, cancellationToken)
            : await _repository.ObterPorIdEParceiroAsync(request.Id, parceiroId, cancellationToken);

        if (categoriaExistente == null)
        {
            return Result.NotFoundResult<InativarCategoriaResponse>("Categoria não encontrada");
        }

        // Inativar categoria
        var linhasAfetadas = await _repository.InativarAsync(request.Id, categoriaExistente.ParceiroId, cancellationToken);

        if (linhasAfetadas == 0)
        {
            return Result.BusinessResult<InativarCategoriaResponse>("Erro ao inativar categoria");
        }

        // Obter categoria inativada
        var categoriaInativada = await _repository.ObterPorIdAsync(request.Id, cancellationToken);

        if (categoriaInativada == null)
        {
            return Result.BusinessResult<InativarCategoriaResponse>("Erro ao obter categoria inativada");
        }

        var response = new InativarCategoriaResponse
        {
            Id = categoriaInativada.Id,
            Nome = categoriaInativada.Nome,
            Ativo = categoriaInativada.Ativo,
            ParceiroId = categoriaInativada.ParceiroId
        };

        return Result.OkResult(response);
    }
}
