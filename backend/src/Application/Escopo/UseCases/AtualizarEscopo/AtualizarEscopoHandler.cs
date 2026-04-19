using FluentValidation;
using Portal.Domain.Base;
using Portal.Domain.Escopo;
using Portal.Domain.Escopo.Interfaces;

namespace Portal.Application.Escopo.UseCases.AtualizarEscopo;

public class AtualizarEscopoHandler
{
    private readonly IEscopoRepository _repository;
    private readonly IValidator<AtualizarEscopoRequest> _validator;

    public AtualizarEscopoHandler(IEscopoRepository repository, IValidator<AtualizarEscopoRequest> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Result<AtualizarEscopoResponse>> Handle(AtualizarEscopoRequest request, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.ValidationResult<AtualizarEscopoResponse>(validation.Errors.Select(x => x.ErrorMessage));
        }

        var atual = await _repository.ObterPorIdAsync(request.Id, cancellationToken);
        if (atual is null)
        {
            return Result.NotFoundResult<AtualizarEscopoResponse>("Escopo não encontrado");
        }

        if (await _repository.ExisteNomeAsync(request.Nome.Trim(), request.Id, cancellationToken))
        {
            return Result.BusinessResult<AtualizarEscopoResponse>("Nome do escopo já existe");
        }

        await _repository.AtualizarAsync(new EscopoCommand { Id = request.Id, Nome = request.Nome.Trim() }, cancellationToken);
        return Result.OkResult(new AtualizarEscopoResponse { Mensagem = "Atualizado com sucesso" });
    }
}
