using Portal.Domain.Base;
using Portal.Domain.Perfil.Interfaces;

namespace Portal.Application.Perfil.UseCases.ObterPerfilPorId;

public class ObterPerfilPorIdHandler
{
    private readonly IPerfilRepository _repository;

    public ObterPerfilPorIdHandler(IPerfilRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ObterPerfilPorIdResponse>> Handle(ObterPerfilPorIdRequest request, CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
            return Result.ValidationResult<ObterPerfilPorIdResponse>("Id do perfil invÃ¡lido");

        var perfil = await _repository.ObterPorIdAsync(request.Id, cancellationToken);
        if (perfil is null)
            return Result.NotFoundResult<ObterPerfilPorIdResponse>($"Perfil {request.Id} nÃ£o encontrado");


        return Result.OkResult(perfil.ToResponsePorId());
    }
}
