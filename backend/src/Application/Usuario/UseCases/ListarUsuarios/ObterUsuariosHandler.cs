using Portal.Domain.Base;
using Portal.Domain.Common;
using Portal.Domain.Usuario.Interfaces;

namespace Portal.Application.Usuario.UseCases.ObterUsuarios;

public class ObterUsuariosHandler : BaseService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ObterUsuariosHandler(IUsuarioRepository usuarioRepository, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Result<TabelaPaginadaResponse<ObterUsuariosResponse>>> Handle(ObterUsuariosRequest request, CancellationToken cancellationToken)
    {
        var usuario = ObterUsuario();
        Guid.TryParse(request.ParceiroId, out var parceiroParsed);

        var idParceiroQuery = usuario.IsMaster && string.IsNullOrEmpty(request.ParceiroId)
            ? Guid.Empty
            : !usuario.IsMaster && string.IsNullOrEmpty(request.ParceiroId)
                ? usuario.ParceiroId
                : parceiroParsed;

        var resultado = await _usuarioRepository.ObterPorParceiroAsync(idParceiroQuery, request.DirecaoEnum, request.Pagina, request.TamanhoPagina, cancellationToken);
        var response = resultado.Itens.Select(x => x.ToResponse()).ToList();
        var tabela = TabelaPaginadaResponse<ObterUsuariosResponse>.Criar(response, resultado.TotalRegistros, resultado.TotalRegistros, resultado.NumeroPagina, resultado.TamanhoPagina);

        return Result.OkResult(tabela);
    }
}

