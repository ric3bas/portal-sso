using FluentValidation;
using Portal.Domain.Base;
using Portal.Domain.Categoria;
using Portal.Domain.Categoria.Interfaces;

namespace Portal.Application.Categoria.UseCases.CriarCategoria;

public class CriarCategoriaHandler : BaseService
{
    private readonly ICategoriaRepository _repository;
    // Temporariamente removido para debug
    // private readonly IValidator<CriarCategoriaRequest> _validator;
    private readonly ILogger<CriarCategoriaHandler> _logger;

    public CriarCategoriaHandler(
        ICategoriaRepository repository,
        // IValidator<CriarCategoriaRequest> validator,
        ILogger<CriarCategoriaHandler> logger,
        IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
        _repository = repository;
        // _validator = validator;
        _logger = logger;
    }

    public async Task<Result<CriarCategoriaResponse>> Handle(CriarCategoriaRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Criando nova categoria: {Nome}", request.Nome);

        // Validação temporariamente comentada
        // var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        // if (!validationResult.IsValid)
        // {
        //     return Result.ValidationResult<CriarCategoriaResponse>(
        //         validationResult.Errors.Select(x => x.ErrorMessage));
        // }

        // Validação simples manual para debug
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return Result.ValidationResult<CriarCategoriaResponse>("Nome é obrigatório");
        }

        // Obter contexto do usuário
        var usuario = ObterUsuario();
        var parceiroId = usuario.IsMaster ? Guid.Empty : usuario.ParceiroId;

        // Verificar se categoria já existe
        if (await _repository.ExisteNomeAsync(request.Nome, parceiroId, cancellationToken: cancellationToken))
        {
            return Result.BusinessResult<CriarCategoriaResponse>("Já existe uma categoria com este nome");
        }

        var categoriaId = await _repository.CriarAsync(new CategoriaCommand
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Ativo = true,
            ParceiroId = parceiroId
        }, cancellationToken);
        var categoria = await _repository.ObterPorIdAsync(categoriaId, cancellationToken);

        if (categoria == null)
        {
            return Result.BusinessResult<CriarCategoriaResponse>("Erro ao criar categoria");
        }

        var response = new CriarCategoriaResponse
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Ativo = categoria.Ativo,
            ParceiroId = categoria.ParceiroId
        };

        return Result.OkResult(response);
    }
}
