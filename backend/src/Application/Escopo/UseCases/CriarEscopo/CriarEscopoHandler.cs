using Portal.Domain.Base;
using Portal.Domain.Escopo;
using Portal.Domain.Escopo.Interfaces;

namespace Portal.Application.Escopo.UseCases.CriarEscopo;

public class CriarEscopoHandler
{
    private readonly IEscopoRepository _repository;

    public CriarEscopoHandler(IEscopoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<string>> Handle(CriarEscopoRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsValid()) return Result.ValidationResult<string>(request.ObterErros());

        if (await _repository.ExisteNomeAsync(request.Nome.Trim(), cancellationToken: cancellationToken))
        {
            return Result.BusinessResult<string>("Nome do escopo já existe");
        }

        _ = await _repository.CriarAsync(new EscopoCommand { Nome = request.Nome.Trim() }, cancellationToken);
        return Result.OkResult("Escopo criado com sucesso");
    }
}
