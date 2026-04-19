using FluentValidation;
using Portal.Domain.Base;
using Portal.Domain.Escopo;
using Portal.Domain.Escopo.Interfaces;

namespace Portal.Application.Escopo.UseCases.CriarEscopo;

public class CriarEscopoHandler
{
    private readonly IEscopoRepository _repository;
    private readonly IValidator<CriarEscopoRequest> _validator;

    public CriarEscopoHandler(IEscopoRepository repository, IValidator<CriarEscopoRequest> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Result<CriarEscopoResponse>> Handle(CriarEscopoRequest request, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.ValidationResult<CriarEscopoResponse>(validation.Errors.Select(x => x.ErrorMessage));
        }

        if (await _repository.ExisteNomeAsync(request.Nome.Trim(), cancellationToken: cancellationToken))
        {
            return Result.BusinessResult<CriarEscopoResponse>("Nome do escopo já existe");
        }

        var id = await _repository.CriarAsync(new EscopoCommand { Nome = request.Nome.Trim() }, cancellationToken);
        return Result.OkResult(new CriarEscopoResponse { Id = id });
    }
}
