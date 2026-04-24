using Portal.Domain.Base;
using Portal.Domain.Common;
using Portal.Domain.Parceiro.Interfaces;

namespace Portal.Application.Parceiro.UseCases.ObterParceiros;

public class ObterParceirosHandler : BaseService
{
    private readonly IParceiroRepository _repository;

    public ObterParceirosHandler(IParceiroRepository repository, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
        _repository = repository;
    }

    public async Task<Result<TabelaPaginadaResponse<ObterParceirosResponse>>> Handle(ObterParceirosRequest request, CancellationToken cancellationToken)
    {
        var usuario = ObterUsuario();
        var parceiroId = usuario.IsMaster ? Guid.Empty : usuario.ParceiroId;

        var resultado = await _repository.ObterTodosAsync(parceiroId, request.DirecaoEnum, request.Pagina, request.TamanhoPagina, cancellationToken);
        var response = resultado.Itens.Select(c => c.ToResponse<ObterParceirosResponse>()).ToList();
        var tabela = TabelaPaginadaResponse<ObterParceirosResponse>.Criar(response, resultado.TotalRegistros, resultado.TotalRegistros, resultado.NumeroPagina, resultado.TamanhoPagina);

        return Result.OkResult(tabela);
    }
}
