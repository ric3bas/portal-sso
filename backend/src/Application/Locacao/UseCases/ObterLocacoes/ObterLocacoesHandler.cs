using Portal.Application.Locacao.Common;
using Portal.Domain.Base;
using Portal.Domain.Locacao.Interfaces;
using Portal.Domain.Portal.Extensions;

namespace Portal.Application.Locacao.UseCases.ObterLocacoes;

public class ObterLocacoesHandler
{
    private readonly ILocacaoRepository _repository;

    public ObterLocacoesHandler(ILocacaoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ObterLocacoesResponse>>> Handle(ObterLocacoesRequest request, CancellationToken cancellationToken)
    {
        var locacoes = await _repository.ObterTodasAsync(cancellationToken);
        if (locacoes == null || !locacoes.Any())
            return Result.NotFoundResult<List<ObterLocacoesResponse>>("Nenhuma locação encontrada");

        return Result.OkResult(locacoes.Select(x => x.ToResponse<ObterLocacoesResponse>()).ToList());
    }
}
