using FluentValidation;
using Portal.Domain.Base;
using Portal.Domain.Parceiro;
using Portal.Domain.Parceiro.Interfaces;

namespace Portal.Application.Parceiro.UseCases.CriarParceiro;

public class CriarParceiroHandler
{
    private readonly IParceiroRepository _repository;
    private readonly IValidator<CriarParceiroRequest> _validator;

    public CriarParceiroHandler(IParceiroRepository repository, IValidator<CriarParceiroRequest> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Result<CriarParceiroResponse>> Handle(CriarParceiroRequest request, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.ValidationResult<CriarParceiroResponse>(validation.Errors.Select(e => e.ErrorMessage));

        var existente = await _repository.ObterPorNomeAsync(request.Nome, cancellationToken);
        if (existente != null)
            return Result.ValidationResult<CriarParceiroResponse>("Já existe um parceiro com este nome");

        var entity = new ParceiroCommand
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Descricao = request.Descricao,
            Ativo = true
        };

        var id = await _repository.InserirAsync(entity, cancellationToken);
        return Result.OkResult(new CriarParceiroResponse { Id = id, Mensagem = "Criado com sucesso" });
    }
}
