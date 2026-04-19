using Portal.Domain.Base;
using Portal.Domain.Parceiro.Interfaces;
using Portal.Domain.Portal.Extensions;

namespace Portal.Application.Parceiro.UseCases.ObterParceiroPorId;

public class ObterParceiroPorIdHandler
{
    private readonly IParceiroRepository _repository;

    public ObterParceiroPorIdHandler(IParceiroRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ObterParceiroPorIdResponse>>Handle(ObterParceiroPorIdRequest request, CancellationToken cancellationToken)
    {
        var parceiro = await _repository.ObterPorIdAsync(request.Id, cancellationToken);
        if (parceiro is null)
            return Result.NotFoundResult<ObterParceiroPorIdResponse>("Parceiro não encontrado");

        return Result.OkResult(parceiro.ToResponse<ObterParceiroPorIdResponse>());
    }
}
