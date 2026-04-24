using Portal.Domain.Base;
using Portal.Domain.Escopo.Interfaces;
using Portal.Domain.Perfil.Interfaces;

namespace Portal.Application.Perfil.UseCases.VincularEscoposPerfil;

public class VincularEscoposPerfilHandler
{
    private readonly IPerfilRepository _perfilRepository;
    private readonly IEscopoRepository _escopoRepository;

    public VincularEscoposPerfilHandler(IPerfilRepository perfilRepository, IEscopoRepository escopoRepository)
    {
        _perfilRepository = perfilRepository;
        _escopoRepository = escopoRepository;
    }

    public async Task<Result<VincularEscoposPerfilResponse>> Handle(VincularEscoposPerfilRequest request, CancellationToken cancellationToken)
    {
        if (request.PerfilId <= 0)
            return Result.ValidationResult<VincularEscoposPerfilResponse>("PerfilId invÃ¡lido");

        var ids = request.EscopoIds?.Distinct().ToList() ?? [];

        var perfilExiste = await _perfilRepository.ExistePerfilAsync(request.PerfilId, cancellationToken);
        if (!perfilExiste)
            return Result.NotFoundResult<VincularEscoposPerfilResponse>($"Perfil {request.PerfilId} nÃ£o encontrado");

        var idsExistentes = (await _escopoRepository.ObterIdsExistentesAsync(ids, cancellationToken)).ToHashSet();
        var idsInvalidos = ids.Except(idsExistentes).ToList();
        if (idsInvalidos.Count > 0)
            return Result.ValidationResult<VincularEscoposPerfilResponse>($"Os seguintes EscopoIds nÃ£o existem: {string.Join(", ", idsInvalidos)}");

        await _perfilRepository.VincularEscoposAsync(request.PerfilId, ids, cancellationToken);
        return Result.OkResult(new VincularEscoposPerfilResponse { Mensagem = "Vinculado com sucesso" });
    }
}
