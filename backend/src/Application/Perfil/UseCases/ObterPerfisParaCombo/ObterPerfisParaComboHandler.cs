using Portal.Domain.Base;
using Portal.Domain.Perfil.Interfaces;
using Portal.Domain.Portal.Extensions;

namespace Portal.Application.Perfil.UseCases.ObterPerfisParaCombo;

public class ObterPerfisParaComboHandler : BaseService
{
    private readonly IPerfilRepository _repository;

    public ObterPerfisParaComboHandler(IPerfilRepository repository, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
        _repository = repository;
    }

    public async Task<Result<List<ObterPerfisParaComboResponse>>> Handle(ObterPerfisParaComboRequest request, CancellationToken cancellationToken)
    {
        var result = await _repository.ObterPerfilParaComboAsync(ObterUsuario().IsMaster, cancellationToken);
        if (!result.Any())
            return Result.NotFoundResult<List<ObterPerfisParaComboResponse>>("Nenhum perfil encontrado");

        return Result.OkResult(result.Select(c => c.ToResponse<ObterPerfisParaComboResponse>()).ToList());

    }
}
