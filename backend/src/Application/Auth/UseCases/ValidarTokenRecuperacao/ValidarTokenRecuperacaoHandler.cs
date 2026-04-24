using Portal.Application.Auth.UseCases.ValidarTokenRecuperacao;
using Portal.Domain.Base;
using Portal.Domain.Usuario.Interfaces;

namespace Portal.Application.Auth.UseCases.TrocarSenha;

public class ValidarTokenRecuperacaoHandler
{
    private readonly IAuthRepository _authRepository;

    public ValidarTokenRecuperacaoHandler(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public async Task<Result<string>> Handle(ValidarTokenRecuperacaoRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsValid())
            return Result.ValidationResult<string>(request.ObterErros());

        var entity = await _authRepository.ObterRecuperacaoSenhaPorTokenAsync(request.Token, cancellationToken);

        if (entity == null)
            return Result.BusinessResult<string>("Token invÃ¡lido");

        if (entity.Usado)
            return Result.BusinessResult<string>("Token jÃ¡ utilizado");

        if (entity.ExpiraEm < DateTime.UtcNow)
            return Result.BusinessResult<string>("Token expirado");

        return Result.OkResult("Token vÃ¡lido, pode prosseguir com alteraÃ§Ã£o de senha");
    }
}
