using Portal.Domain.Base;
using Portal.Domain.Perfil.Interfaces;

namespace Portal.Application.Perfil.UseCases.ApagarPerfil;

public class ApagarPerfilHandler
{
    private readonly IPerfilRepository _repository;

    public ApagarPerfilHandler(IPerfilRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ApagarPerfilResponse>> Handle(ApagarPerfilRequest request, CancellationToken cancellationToken)
    {
        var perfilExiste = await _repository.ExistePerfilAsync(request.Id, cancellationToken);
        if (!perfilExiste)
            return Result.NotFoundResult<ApagarPerfilResponse>($"Perfil {request.Id} nÃ£o encontrado");

        await _repository.DeletarAsync(request.Id, cancellationToken);
        return Result.OkResult(new ApagarPerfilResponse { Mensagem = "Apagado com sucesso" });
    }
}
