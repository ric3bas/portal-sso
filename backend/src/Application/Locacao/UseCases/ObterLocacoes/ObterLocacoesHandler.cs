using Portal.Application.Locacao.Common;
using Portal.Domain.Base;
using Portal.Domain.Common;
using Portal.Domain.Locacao.Interfaces;

namespace Portal.Application.Locacao.UseCases.ObterLocacoes;

public class ObterLocacoesHandler
{
    private readonly ILocacaoRepository _repository;

    public ObterLocacoesHandler(ILocacaoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TabelaPaginadaResponse<ObterLocacoesResponse>>> Handle(ObterLocacoesRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _repository.ObterTodasAsync(request.DirecaoEnum, request.Pagina, request.TamanhoPagina, cancellationToken);
        var response = resultado.Itens.Select(x => x.ToResponse<ObterLocacoesResponse>()).ToList();
        var tabela = TabelaPaginadaResponse<ObterLocacoesResponse>.Criar(response, resultado.TotalRegistros, resultado.TotalRegistros, resultado.NumeroPagina, resultado.TamanhoPagina);

        return Result.OkResult(tabela);
    }
}
