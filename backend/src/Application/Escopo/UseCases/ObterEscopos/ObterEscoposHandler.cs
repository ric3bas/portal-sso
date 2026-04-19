using Portal.Domain.Base;
using Portal.Domain.Escopo.Interfaces;
using Portal.Domain.Portal.Extensions;

namespace Portal.Application.Escopo.UseCases.ObterEscopos;

public class ObterEscoposHandler
{
    private readonly IEscopoRepository _repository;

    public ObterEscoposHandler(IEscopoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ObterEscoposResponse>>> Handle(ObterEscoposRequest request, CancellationToken cancellationToken)
    {
        var escopos = await _repository.ObterTodosAsync(cancellationToken);
        if (escopos == null || !escopos.Any())
        {
            return Result.NotFoundResult<List<ObterEscoposResponse>>("Nenhum escopo encontrado");
        }

        return Result.OkResult(escopos.Select(x => x.ToResponse<ObterEscoposResponse>()).ToList());
    }
}
