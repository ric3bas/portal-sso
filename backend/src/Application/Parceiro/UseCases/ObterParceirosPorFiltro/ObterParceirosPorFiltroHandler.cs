
using Portal.Domain.Base;
using Portal.Domain.Parceiro.Interfaces;
using Portal.Domain.Portal.Extensions;

namespace Portal.Application.Parceiro.UseCases.ObterParceirosPorFiltro;

public class ObterParceirosPorFiltroHandler
{
    private readonly IParceiroRepository _repository;

    public ObterParceirosPorFiltroHandler(IParceiroRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ObterParceirosPorFiltroResponse>>> Handle(ObterParceirosPorFiltroRequest request, CancellationToken cancellationToken)
    {
        var result = await _repository.ObterTodosPorFiltroAsync(request.Nome, cancellationToken);
        if (result == null || !result.Any())
            return Result.NotFoundResult<List<ObterParceirosPorFiltroResponse>>("Nenhum parceiro encontrado");

        return Result.OkResult(result.Select(x => x.ToResponse<ObterParceirosPorFiltroResponse>()).ToList());
    }
}
