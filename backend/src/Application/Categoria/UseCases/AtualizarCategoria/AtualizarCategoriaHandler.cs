using FluentValidation;
using Portal.Domain.Base;
using Portal.Domain.Categoria;
using Portal.Domain.Categoria.Interfaces;

namespace Portal.Application.Categoria.UseCases.AtualizarCategoria;

public class AtualizarCategoriaHandler : BaseService
{
    private readonly ICategoriaRepository _repository;
    // Temporariamente removido para debug
    // private readonly IValidator<AtualizarCategoriaRequest> _validator;
    private readonly ILogger<AtualizarCategoriaHandler> _logger;

    public AtualizarCategoriaHandler(
        ICategoriaRepository repository,
        // IValidator<AtualizarCategoriaRequest> validator,
        ILogger<AtualizarCategoriaHandler> logger,
        IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
        _repository = repository;
        // _validator = validator;
        _logger = logger;
    }

    public async Task<Result<AtualizarCategoriaResponse>> Handle(AtualizarCategoriaRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando categoria {Id}", request.Id);

        // Validação temporariamente comentada
        // var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        // if (!validationResult.IsValid)
        // {
        //     return Result.ValidationResult<AtualizarCategoriaResponse>(
        //         validationResult.Errors.Select(x => x.ErrorMessage));
        // }

        // Validação simples manual para debug
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return Result.ValidationResult<AtualizarCategoriaResponse>("Nome é obrigatório");
        }

        var usuario = ObterUsuario();
        var parceiroId = usuario.IsMaster ? Guid.Empty : usuario.ParceiroId;

        // Verificar se categoria existe
        var categoriaExistente = parceiroId == Guid.Empty 
            ? await _repository.ObterPorIdAsync(request.Id, cancellationToken)
            : await _repository.ObterPorIdEParceiroAsync(request.Id, parceiroId, cancellationToken);

        if (categoriaExistente == null)
        {
            return Result.NotFoundResult<AtualizarCategoriaResponse>("Categoria não encontrada");
        }

        // Verificar se nome já existe (excluindo o próprio registro)
        if (await _repository.ExisteNomeAsync(request.Nome, categoriaExistente.ParceiroId, request.Id, cancellationToken))
        {
            return Result.BusinessResult<AtualizarCategoriaResponse>("Já existe uma categoria com este nome");
        }

        // Atualizar categoria
        var linhasAfetadas = await _repository.AtualizarAsync(new CategoriaCommand
        {
            Id = request.Id,
            Nome = request.Nome,
            Ativo = request.Ativo ?? true,
            ParceiroId = categoriaExistente.ParceiroId
        }, cancellationToken);

        if (linhasAfetadas == 0)
        {
            return Result.BusinessResult<AtualizarCategoriaResponse>("Erro ao atualizar categoria");
        }

        // Obter categoria atualizada
        var categoriaAtualizada = await _repository.ObterPorIdAsync(request.Id, cancellationToken);

        if (categoriaAtualizada == null)
        {
            return Result.BusinessResult<AtualizarCategoriaResponse>("Erro ao obter categoria atualizada");
        }

        var response = new AtualizarCategoriaResponse
        {
            Id = categoriaAtualizada.Id,
            Nome = categoriaAtualizada.Nome,
            Ativo = categoriaAtualizada.Ativo,
            ParceiroId = categoriaAtualizada.ParceiroId
        };

        return Result.OkResult(response);
    }
}
