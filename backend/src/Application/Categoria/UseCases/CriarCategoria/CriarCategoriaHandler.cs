using FluentValidation;
using Portal.Domain.Base;
using Portal.Domain.Categoria;
using Portal.Domain.Categoria.Interfaces;

namespace Portal.Application.Categoria.UseCases.CriarCategoria;

public class CriarCategoriaHandler : BaseService
{
    private readonly ICategoriaRepository _repository;
    private readonly ILogger<CriarCategoriaHandler> _logger;

    public CriarCategoriaHandler(
        ICategoriaRepository repository,
        ILogger<CriarCategoriaHandler> logger,
        IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(CriarCategoriaRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Criando nova categoria: {Nome}", request.Nome);

        if (!request.IsValid()) return Result.ValidationResult<string>(request.ObterErros());

        var usuario = ObterUsuario();
        var parceiroId = usuario.IsMaster ? Guid.Empty : usuario.ParceiroId;

        if (await _repository.ExisteNomeAsync(request.Nome, parceiroId, cancellationToken: cancellationToken))
        {
            return Result.BusinessResult<string>("JÃ¡ existe uma categoria com este nome");
        }

        await _repository.CriarAsync(new CategoriaCommand
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Ativo = true,
            ParceiroId = parceiroId
        }, cancellationToken);

        return Result.OkResult("Categoria criada com sucesso");
    }
}
