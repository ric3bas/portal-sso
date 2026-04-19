using FluentValidation;
using Portal.Domain.Base;
using Portal.Domain.Parceiro;
using Portal.Domain.Parceiro.Interfaces;

namespace Portal.Application.Parceiro.UseCases.AtualizarParceiro;

public class AtualizarParceiroHandler
{
    private readonly IParceiroRepository _repository;
    private readonly IValidator<AtualizarParceiroRequest> _validator;

    public AtualizarParceiroHandler(IParceiroRepository repository, IValidator<AtualizarParceiroRequest> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Result<AtualizarParceiroResponse>> Handle(AtualizarParceiroRequest request, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.ValidationResult<AtualizarParceiroResponse>(validation.Errors.Select(e => e.ErrorMessage));

        var (existe, nomeConflito) = await _repository.ValidarAtualizacaoAsync(request.Id, request.Nome, cancellationToken);
        if (!existe)
            return Result.NotFoundResult<AtualizarParceiroResponse>("Parceiro não encontrado");

        if (nomeConflito)
            return Result.ValidationResult<AtualizarParceiroResponse>("Já existe outro parceiro com este nome");

        await _repository.AtualizarAsync(new ParceiroCommand
        {
            Id = request.Id,
            Nome = request.Nome,
            Descricao = request.Descricao,
            Ativo = request.Ativo
        }, cancellationToken);

        return Result.OkResult(new AtualizarParceiroResponse { Mensagem = "Atualizado com sucesso" });
    }
}
