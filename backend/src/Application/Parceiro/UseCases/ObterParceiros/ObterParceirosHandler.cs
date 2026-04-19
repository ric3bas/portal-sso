using Portal.Domain.Base;
using Portal.Domain.Parceiro.Interfaces;
using Portal.Domain.Portal.Extensions;

namespace Portal.Application.Parceiro.UseCases.ObterParceiros;

public class ObterParceirosHandler : BaseService
{
    private readonly IParceiroRepository _repository;

    public ObterParceirosHandler(IParceiroRepository repository, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
        _repository = repository;
    }

    public async Task<Result<List<ObterParceirosResponse>>> Handle(ObterParceirosRequest request, CancellationToken cancellationToken)
    {
        var usuario = ObterUsuario();
        var parceiroId = usuario.IsMaster ? Guid.Empty : usuario.ParceiroId;

        var result = await _repository.ObterTodosAsync(parceiroId, cancellationToken);
        if (result == null || !result.Any())
            return Result.NotFoundResult<List<ObterParceirosResponse>>("Nenhum parceiro encontrado");

        return Result.OkResult(result.Select(c => c.ToResponse<ObterParceirosResponse>()).ToList());
    }
}
