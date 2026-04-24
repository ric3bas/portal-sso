using Portal.Domain.Base;
using Portal.Domain.Perfil;
using Portal.Domain.Perfil.Interfaces;

namespace Portal.Application.Perfil.UseCases.ClonarPerfil;

public class ClonarPerfilHandler
{
    private readonly IPerfilRepository _repository;

    public ClonarPerfilHandler(IPerfilRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ClonarPerfilResponse>> Handle(ClonarPerfilRequest request, CancellationToken cancellationToken)
    {
        var perfil = await _repository.ObterPorIdAsync(request.Id, cancellationToken);
        if (perfil is null)
            return Result.NotFoundResult<ClonarPerfilResponse>($"Perfil {request.Id} nÃ£o encontrado");

        var novoPerfilId = await _repository.InserirAsync(new PerfilCommand { Nome = perfil.Nome + " (CÃ³pia)" }, cancellationToken);
        var escopoIds = perfil.Escopos.Select(e => e.Id);
        await _repository.VincularEscoposAsync(novoPerfilId, escopoIds, cancellationToken);

        return Result.OkResult(new ClonarPerfilResponse { Mensagem = "Clonado com sucesso" });
    }
}
