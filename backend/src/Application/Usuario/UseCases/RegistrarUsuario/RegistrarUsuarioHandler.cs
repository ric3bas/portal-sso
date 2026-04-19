using FluentValidation;
using Portal.Domain.Base;
using Portal.Domain.Usuario;
using Portal.Domain.Usuario.Interfaces;

namespace Portal.Application.Usuario.UseCases.RegistrarUsuario;

public class RegistrarUsuarioHandler
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IValidator<RegistrarUsuarioRequest> _validator;

    public RegistrarUsuarioHandler(IUsuarioRepository usuarioRepository, IValidator<RegistrarUsuarioRequest> validator)
    {
        _usuarioRepository = usuarioRepository;
        _validator = validator;
    }

    public async Task<Result<RegistrarUsuarioResponse>> Handle(RegistrarUsuarioRequest request, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.ValidationResult<RegistrarUsuarioResponse>(validation.Errors.Select(e => e.ErrorMessage));

        Guid.TryParse(request.ParceiroId, out var parceiroParsed);

        var validacao = await _usuarioRepository.ValidarRegistroAsync(request.Login, parceiroParsed, request.PerfilId, cancellationToken);

        if (!validacao.ParceiroExiste)
            return Result.NotFoundResult<RegistrarUsuarioResponse>($"Parceiro '{parceiroParsed}' não encontrado");

        if (validacao.LoginExiste)
            return Result.ValidationResult<RegistrarUsuarioResponse>($"Login '{request.Login}' já existe");

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

        return Result.OkResult(new RegistrarUsuarioResponse { Mensagem = "Cadastrado com sucesso" });
    }
}
