using Portal.Domain.Base;
using Portal.Domain.Usuario;
using Portal.Domain.Usuario.Interfaces;

namespace Portal.Application.Usuario.UseCases.CriarUsuario;

public class CriarUsuarioHandler
{
    private readonly IUsuarioRepository _usuarioRepository;

    public CriarUsuarioHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Result<string>> Handle(CriarUsuarioRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsValid()) return Result.ValidationResult<string>(request.ObterErros());

        Guid.TryParse(request.ParceiroId, out var parceiroParsed);

        var validacao = await _usuarioRepository.ValidarRegistroAsync(request.Login, parceiroParsed, request.PerfilId, cancellationToken);

        if (!validacao.ParceiroExiste)
            return Result.NotFoundResult<string>($"Parceiro '{parceiroParsed}' não encontrado");

        if (validacao.LoginExiste)
            return Result.ValidationResult<string>($"Login '{request.Login}' já existe");

        var usuario = new UsuarioCommand
        {
            Nome = request.Nome,
            Email = request.Email,
            Login = request.Login,
            Senha = BCrypt.Net.BCrypt.HashPassword(request.Senha),
            ParceiroId = parceiroParsed,
            PerfilId = request.PerfilId,
            TentativasLogin = 0,
            Bloqueado = false,
            Ativo = true
        };

        _ = await _usuarioRepository.InserirAsync(usuario, cancellationToken);

        return Result.OkResult("Usuário cadastrado com sucesso");
    }
}

