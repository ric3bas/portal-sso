using Portal.Domain.Base;
using Portal.Domain.Portal.Extensions;
using Portal.Domain.Usuario.Interfaces;

namespace Portal.Application.Usuario.UseCases.ListarUsuarios;

public class ListarUsuariosHandler : BaseService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ListarUsuariosHandler(IUsuarioRepository usuarioRepository, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Result<List<ListarUsuariosResponse>>> Handle(ListarUsuariosRequest request, CancellationToken cancellationToken)
    {
        var usuario = ObterUsuario();
        Guid.TryParse(request.ParceiroId, out var parceiroParsed);

        var idParceiroQuery = usuario.IsMaster && string.IsNullOrEmpty(request.ParceiroId)
            ? Guid.Empty
            : !usuario.IsMaster && string.IsNullOrEmpty(request.ParceiroId)
                ? usuario.ParceiroId
                : parceiroParsed;

        var result = await _usuarioRepository.ListarPorParceiroAsync(idParceiroQuery, cancellationToken);
        if (!result.Any())
            return Result.NotFoundResult<List<ListarUsuariosResponse>>("Nenhum usuário encontrado");

        return Result.OkResult(result.Select(x => x.ToResponse<ListarUsuariosResponse>()).ToList());
    }
}
