using Portal.Application.Auth.UseCases.ValidarTokenRecuperacao;
using Portal.Domain.Base;
using Portal.Domain.Usuario.Interfaces;

namespace Portal.Application.Auth.UseCases.TrocarSenha;

public class TrocarSenhaHandler
{
    private readonly IAuthRepository _authRepository;

    public TrocarSenhaHandler(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public async Task<Result<string>> Handle(TrocarSenhaRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsValid())
            return Result.ValidationResult<string>(request.ObterErros());

        var entity = await _authRepository.ObterRecuperacaoSenhaPorTokenAsync(request.Token, cancellationToken);

        if (entity == null)
            return Result.BusinessResult<string>("Token inválido");

        if (entity.Usado)
            return Result.BusinessResult<string>("Token já utilizado");

        if (entity.ExpiraEm < DateTime.UtcNow)
            return Result.BusinessResult<string>("Token expirado");

        var novaSenhaHash = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);

        _ = await _authRepository.AtualizarSenhaUsuarioAsync(entity.UsuarioId, novaSenhaHash, cancellationToken);

        await _authRepository.MarcarRecuperacaoSenhaComoUsadoAsync(entity.Id, cancellationToken);

        return Result.OkResult("Senha alterada com sucesso");
    }

}
