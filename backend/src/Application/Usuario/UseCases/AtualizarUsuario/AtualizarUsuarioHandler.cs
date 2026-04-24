using Portal.Domain.Base;
using Portal.Domain.Usuario;
using Portal.Domain.Usuario.Interfaces;

namespace Portal.Application.Usuario.UseCases.AtualizarUsuario;

public class AtualizarUsuarioHandler
{
    private readonly IUsuarioRepository _usuarioRepository;

    public AtualizarUsuarioHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Result<AtualizarUsuarioResponse>> Handle(AtualizarUsuarioRequest request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(request.Id, cancellationToken);
        if (usuario == null)
            return Result.NotFoundResult<AtualizarUsuarioResponse>("UsuÃ¡rio nÃ£o encontrado");

        await _usuarioRepository.AtualizarAsync(new UsuarioCommand
        {
            Id = request.Id,
            Nome = request.Nome,
            Login = request.Login,
            Email = request.Email,
            Ativo = request.Ativo,
            Bloqueado = request.Bloqueado
        }, cancellationToken);

        return Result.OkResult(new AtualizarUsuarioResponse { Mensagem = "Alterado com sucesso" });
    }
}
