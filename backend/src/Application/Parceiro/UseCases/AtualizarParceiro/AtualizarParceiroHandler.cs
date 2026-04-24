using Portal.Domain.Base;
using Portal.Domain.Parceiro;
using Portal.Domain.Parceiro.Interfaces;

namespace Portal.Application.Parceiro.UseCases.AtualizarParceiro;

public class AtualizarParceiroHandler
{
    private readonly IParceiroRepository _repository;

    public AtualizarParceiroHandler(IParceiroRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<string>> Handle(AtualizarParceiroRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsValid()) return Result.ValidationResult<string>(request.ObterErros());

        var (existe, nomeConflito) = await _repository.ValidarAtualizacaoAsync(request.Id, request.Nome, cancellationToken);
        if (!existe)
            return Result.NotFoundResult<string>("Parceiro não encontrado");

        if (nomeConflito)
            return Result.ValidationResult<string>("Já existe outro parceiro com este nome");

        await _repository.AtualizarAsync(new ParceiroCommand
        {
            Id = request.Id,
            Nome = request.Nome,
            Descricao = request.Descricao,
            CorPrimaria = request.CorPrimaria,
            CorSecundaria = request.CorSecundaria,
            Ativo = request.Ativo
        }, cancellationToken);

        return Result.OkResult("Parceiro atualizado com sucesso");
    }
}
