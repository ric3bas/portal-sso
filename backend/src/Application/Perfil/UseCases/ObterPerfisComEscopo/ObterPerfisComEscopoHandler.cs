using Portal.Domain.Base;
using Portal.Domain.Perfil.Interfaces;
using Portal.Domain.Portal.Extensions;

namespace Portal.Application.Perfil.UseCases.ObterPerfisComEscopo;

public class ObterPerfisComEscopoHandler
{
    private readonly IPerfilRepository _repository;

    public ObterPerfisComEscopoHandler(IPerfilRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ObterPerfisComEscopoResponse>>> Handle(ObterPerfisComEscopoRequest request, CancellationToken cancellationToken)
    {
        var result = await _repository.ListarComEscoposAsync(cancellationToken);
        if (!result.Any())
            return Result.NotFoundResult<List<ObterPerfisComEscopoResponse>> ("Nenhum perfil encontrado");

        return Result.OkResult(result.Select(c=>c.ToResponse<ObterPerfisComEscopoResponse>()).ToList());
    }
}
