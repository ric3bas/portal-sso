using Portal.Domain.Base;
using Portal.Domain.Parceiro;
using Portal.Domain.Parceiro.Interfaces;

namespace Portal.Application.Parceiro.UseCases.CriarParceiro;

public class CriarParceiroHandler
{
    private readonly IParceiroRepository _repository;

    public CriarParceiroHandler(IParceiroRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<string>> Handle(CriarParceiroRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsValid()) return Result.ValidationResult<string>(request.ObterErros());

        var existente = await _repository.ObterPorNomeAsync(request.Nome, cancellationToken);
        if (existente != null)
            return Result.ValidationResult<string>("Já existe um parceiro com este nome");

        var entity = new ParceiroCommand
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Descricao = request.Descricao,
            CorPrimaria = request.CorPrimaria,
            CorSecundaria = request.CorSecundaria,
            Ativo = true
        };

        _ = await _repository.InserirAsync(entity, cancellationToken);
        return Result.OkResult("Parceiro criado com sucesso");
    }
}
