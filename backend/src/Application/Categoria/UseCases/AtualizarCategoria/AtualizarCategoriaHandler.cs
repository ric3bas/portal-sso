using FluentValidation;
using Portal.Application.Parceiro.UseCases.CriarParceiro;
using Portal.Domain.Base;
using Portal.Domain.Categoria;
using Portal.Domain.Categoria.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Portal.Application.Categoria.UseCases.AtualizarCategoria;

public class AtualizarCategoriaHandler : BaseService
{
    private readonly ICategoriaRepository _repository;
    private readonly ILogger<AtualizarCategoriaHandler> _logger;

    public AtualizarCategoriaHandler(
        ICategoriaRepository repository,
        ILogger<AtualizarCategoriaHandler> logger,
        IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(AtualizarCategoriaRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando categoria {Id}", request.Id);

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

        if (await _repository.ExisteNomeAsync(request.Nome, categoriaExistente.ParceiroId, request.Id, cancellationToken))
        {
            return Result.BusinessResult<string>("JÃ¡ existe uma categoria com este nome");
        }
        await _repository.AtualizarAsync(new CategoriaCommand
        {
            Id = request.Id,
            Nome = request.Nome,
            Ativo = request.Ativo ?? true,
            ParceiroId = categoriaExistente.ParceiroId
        }, cancellationToken);


        return Result.OkResult("Categoria atualizada com sucesso");
    }
}
