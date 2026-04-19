using Portal.Domain.Base;
using Portal.Domain.Perfil.Interfaces;
using Portal.Domain.Portal.Extensions;

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
            return Result.ValidationResult<ObterPerfilPorIdResponse>("Id do perfil inválido");

        var perfil = await _repository.ObterPorIdAsync(request.Id, cancellationToken);
        if (perfil is null)
            return Result.NotFoundResult<ObterPerfilPorIdResponse>($"Perfil {request.Id} não encontrado");


        return Result.OkResult(perfil.ToResponse<ObterPerfilPorIdResponse>());
    }
}
