using Portal.Domain.Base;
using Portal.Domain.Common;
using Portal.Domain.Perfil.Interfaces;

namespace Portal.Application.Perfil.UseCases.ObterPerfisParaCombo;

public class ObterPerfisParaComboHandler : BaseService
{
    private readonly IPerfilRepository _repository;

    public ObterPerfisParaComboHandler(IPerfilRepository repository, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
        _repository = repository;
    }

    public async Task<Result<TabelaPaginadaResponse<ObterPerfisParaComboResponse>>> Handle(ObterPerfisParaComboRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _repository.ObterPerfilParaComboAsync(ObterUsuario().IsMaster, request.DirecaoEnum, request.Pagina, request.TamanhoPagina, cancellationToken);
        var response = resultado.Itens.Select(c => c.ToResponseParaCombo()).ToList();
        var tabela = TabelaPaginadaResponse<ObterPerfisParaComboResponse>.Criar(response, resultado.TotalRegistros, resultado.TotalRegistros, resultado.NumeroPagina, resultado.TamanhoPagina);

        return Result.OkResult(tabela);

    }
}
