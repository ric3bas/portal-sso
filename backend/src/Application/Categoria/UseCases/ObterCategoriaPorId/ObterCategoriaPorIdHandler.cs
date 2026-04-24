using Portal.Domain.Base;
using Portal.Domain.Categoria;
using Portal.Domain.Categoria.Interfaces;

namespace Portal.Application.Categoria.UseCases.ObterCategoriaPorId;

public class ObterCategoriaPorIdHandler : BaseService
{
    private readonly ICategoriaRepository _repository;
    private readonly ILogger<ObterCategoriaPorIdHandler> _logger;

    public ObterCategoriaPorIdHandler(
        ICategoriaRepository repository,
        ILogger<ObterCategoriaPorIdHandler> logger,
        IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<ObterCategoriaPorIdResponse>> Handle(ObterCategoriaPorIdRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando categoria por ID {Id}", request.Id);

        if (!request.IsValid()) return Result.ValidationResult<ObterCategoriaPorIdResponse>(request.ObterErros());

        var usuario = ObterUsuario();
        var parceiroId = usuario.IsMaster ? Guid.Empty : usuario.ParceiroId;

        CategoriaQuery? categoria;

        if (parceiroId == Guid.Empty)
        {
            categoria = await _repository.ObterPorIdAsync(request.Id, cancellationToken);
        }
        else
        {
            categoria = await _repository.ObterPorIdEParceiroAsync(request.Id, parceiroId, cancellationToken);
        }

        if (categoria == null)
            return Result.NotFoundResult<ObterCategoriaPorIdResponse>("Categoria nÃ£o encontrada");

        var response = new ObterCategoriaPorIdResponse
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Ativo = categoria.Ativo,
            ParceiroId = categoria.ParceiroId
        };

        return Result.OkResult(response);
    }
}
