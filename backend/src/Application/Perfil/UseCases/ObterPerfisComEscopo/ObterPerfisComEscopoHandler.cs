using Portal.Domain.Base;
using Portal.Domain.Common;
using Portal.Domain.Perfil.Interfaces;

namespace Portal.Application.Perfil.UseCases.ObterPerfisComEscopo;

public class ObterPerfisComEscopoHandler
{
    private readonly IPerfilRepository _repository;

    public ObterPerfisComEscopoHandler(IPerfilRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TabelaPaginadaResponse<ObterPerfisComEscopoResponse>>> Handle(ObterPerfisComEscopoRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _repository.ObterComEscoposAsync(request.DirecaoEnum, request.Pagina, request.TamanhoPagina, cancellationToken);
        var response = resultado.Itens.Select(c=>c.ToResponse()).ToList();
        var tabela = TabelaPaginadaResponse<ObterPerfisComEscopoResponse>.Criar(response, resultado.TotalRegistros, resultado.TotalRegistros, resultado.NumeroPagina, resultado.TamanhoPagina);

        return Result.OkResult(tabela);
    }
}

