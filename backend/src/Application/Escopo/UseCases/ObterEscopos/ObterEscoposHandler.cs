using Portal.Domain.Base;
using Portal.Domain.Common;
using Portal.Domain.Escopo.Interfaces;

namespace Portal.Application.Escopo.UseCases.ObterEscopos;

public class ObterEscoposHandler
{
    private readonly IEscopoRepository _repository;

    public ObterEscoposHandler(IEscopoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TabelaPaginadaResponse<ObterEscoposResponse>>> Handle(ObterEscoposRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _repository.ObterTodosAsync(request.Nome, request.DirecaoEnum, request.Pagina, request.TamanhoPagina, cancellationToken);
        var response = resultado.Itens.Select(x => x.ToResponse()).ToList();
        var tabela = TabelaPaginadaResponse<ObterEscoposResponse>.Criar(response, resultado.TotalRegistros, resultado.TotalRegistros, resultado.NumeroPagina, resultado.TamanhoPagina);

        return Result.OkResult(tabela);
    }
}
