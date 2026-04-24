using Portal.Application.Locacao.Common;
using Portal.Domain.Base;
using Portal.Domain.Common;
using Portal.Domain.Locacao.Interfaces;

namespace Portal.Application.Locacao.UseCases.ObterLocacoesAtrasadas;

public class ObterLocacoesAtrasadasHandler
{
    private readonly ILocacaoRepository _repository;

    public ObterLocacoesAtrasadasHandler(ILocacaoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TabelaPaginadaResponse<ObterLocacoesAtrasadasResponse>>> Handle(ObterLocacoesAtrasadasRequest request, CancellationToken cancellationToken)
    {
        await _repository.AtualizarStatusAtrasadasAsync(cancellationToken);

        var resultado = await _repository.ObterAtrasadasAsync(request.DirecaoEnum, request.Pagina, request.TamanhoPagina, cancellationToken);
        var response = resultado.Itens.Select(c=>c.ToResponse<ObterLocacoesAtrasadasResponse>()).ToList();
        var tabela = TabelaPaginadaResponse<ObterLocacoesAtrasadasResponse>.Criar(response, resultado.TotalRegistros, resultado.TotalRegistros, resultado.NumeroPagina, resultado.TamanhoPagina);

        return Result.OkResult(tabela);
    }
}
