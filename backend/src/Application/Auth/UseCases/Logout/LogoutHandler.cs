using Portal.Domain.Base;
using TokenRepo = Portal.Domain.Usuario.Interfaces.ITokenAtualizacaoRepository;

namespace Portal.Application.Auth.UseCases.Logout;

public class LogoutHandler
{
    private readonly TokenRepo _tokenRepo;

    public LogoutHandler(TokenRepo tokenRepo)
    {
        _tokenRepo = tokenRepo;
    }

    public async Task<Result<bool>> Handle(LogoutRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsValid())
            return Result.ValidationResult<bool>(request.ObterErros());

        var token = await _tokenRepo.ObterPorTokenAsync(request.RefreshToken, cancellationToken);
        if (token is null || token.Revogado)
            return Result.BusinessResult<bool>("Refresh token invÃ¡lido ou jÃ¡ revogado");

        await _tokenRepo.RevogarAsync(request.RefreshToken, cancellationToken);
        return Result.OkResult(true);
    }
}
