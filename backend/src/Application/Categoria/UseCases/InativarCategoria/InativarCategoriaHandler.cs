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

    public async Task<Result<string>> Handle(InativarCategoriaRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Inativando categoria {Id}", request.Id);

        if (!request.IsValid()) return Result.ValidationResult<string>(request.ObterErros());


        var usuario = ObterUsuario();
        var parceiroId = usuario.IsMaster ? Guid.Empty : usuario.ParceiroId;

        var categoriaExistente = parceiroId == Guid.Empty 
            ? await _repository.ObterPorIdAsync(request.Id, cancellationToken)
            : await _repository.ObterPorIdEParceiroAsync(request.Id, parceiroId, cancellationToken);

        if (categoriaExistente == null)
        {
            return Result.NotFoundResult<string>("Categoria nÃ£o encontrada");
        }

        await _repository.InativarAsync(request.Id, categoriaExistente.ParceiroId, cancellationToken);


        return Result.OkResult("Categoria inativada com sucesso");
    }
}
