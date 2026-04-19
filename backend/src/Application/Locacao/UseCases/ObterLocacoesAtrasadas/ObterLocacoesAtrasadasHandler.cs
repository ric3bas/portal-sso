using Portal.Application.Locacao.Common;
using Portal.Domain.Base;
using Portal.Domain.Locacao.Interfaces;
using Portal.Domain.Portal.Extensions;

namespace Portal.Application.Locacao.UseCases.ObterLocacoesAtrasadas;

public class ObterLocacoesAtrasadasHandler
{
    private readonly ILocacaoRepository _repository;

    public ObterLocacoesAtrasadasHandler(ILocacaoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ObterLocacoesAtrasadasResponse>>> Handle(ObterLocacoesAtrasadasRequest request, CancellationToken cancellationToken)
    {
        await _repository.AtualizarStatusAtrasadasAsync(cancellationToken);

        var locacoes = await _repository.ObterAtrasadasAsync(cancellationToken);
        if (locacoes == null || !locacoes.Any())
            return Result.NotFoundResult<List<ObterLocacoesAtrasadasResponse>>("Nenhuma locação atrasada encontrada");

        return Result.OkResult(locacoes.Select(c=>c.ToResponse<ObterLocacoesAtrasadasResponse>()).ToList());
    }
}
