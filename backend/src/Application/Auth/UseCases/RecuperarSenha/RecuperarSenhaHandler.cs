using Portal.Domain.Base;
using Portal.Domain.Base.Email;
using Portal.Domain.Usuario.Interfaces;

namespace Portal.Application.Auth.UseCases.RecuperarSenha;

public class RecuperarSenhaHandler
{
    private readonly IAuthRepository _authRepository;
    private readonly IEmailService _emailService;

    public RecuperarSenhaHandler(IAuthRepository authRepository, IEmailService emailService)
    {
        _authRepository = authRepository;
        _emailService = emailService;
    }

    public async Task<Result<string>> Handle(RecuperarSenhaRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsValid())
            return Result.ValidationResult<string>(request.ObterErros());

        var loginData = await _authRepository.ObterDadosLoginAsync(request.Login, cancellationToken);
        var usuario = loginData.Usuario;
        if (usuario == null || string.IsNullOrEmpty(usuario.Email))
            return Result.BusinessResult<string>("UsuÃ¡rio nÃ£o encontrado");

        if (!usuario.Ativo)
            return Result.BusinessResult<string>("UsuÃ¡rio inativo no sistema");

        var token = TokenBase.GerarToken();
        await _authRepository.InserirRecuperacaoSenhaAsync(usuario.Id, token, DateTime.UtcNow.AddMinutes(15), false, cancellationToken);

        var assunto = "RecuperaÃ§Ã£o de Senha";
        var corpo = $@"<!DOCTYPE html>
<html><body style='font-family: Arial, sans-serif;'>
<p>OlÃ¡,</p>
<p>Clique no <a href='http://localhost:5173/auth/trocar-senha?token={token}'>Link</a> para alterar sua senha.</p>
<p>Se vocÃª nÃ£o solicitou a alteraÃ§Ã£o, ignore este e-mail.</p>
</body></html>";

        await _emailService.EnviarEmailAsync(usuario.Email, assunto, corpo);
        return Result.OkResult("E-mail enviado com sucesso");
    }

    public async Task<Result<string>> HandleLogado(RecuperarSenhaRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsValid())
            return Result.ValidationResult<string>(request.ObterErros());

        var loginData = await _authRepository.ObterDadosLoginAsync(request.Login, cancellationToken);
        var usuario = loginData.Usuario;
        if (usuario == null || string.IsNullOrEmpty(usuario.Email))
            return Result.BusinessResult<string>("UsuÃ¡rio nÃ£o encontrado");

        if (!usuario.Ativo)
            return Result.BusinessResult<string>("UsuÃ¡rio inativo no sistema");

        var token = TokenBase.GerarToken();
        await _authRepository.InserirRecuperacaoSenhaAsync(usuario.Id, token, DateTime.UtcNow.AddMinutes(15), false, cancellationToken);

        return Result.OkResult(token);
    }
}
