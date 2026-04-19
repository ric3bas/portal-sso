using Portal.Domain.Base;
using Portal.Domain.Perfil.Interfaces;

namespace Portal.Application.Perfil.UseCases.AtualizarNomePerfil;

public class AtualizarNomePerfilHandler
{
    private readonly IPerfilRepository _repository;

    public AtualizarNomePerfilHandler(IPerfilRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AtualizarNomePerfilResponse>> Handle(AtualizarNomePerfilRequest request, CancellationToken cancellationToken)
    {
        if (request.Id <= 0 || string.IsNullOrWhiteSpace(request.NovoNome))
            return Result.ValidationResult<AtualizarNomePerfilResponse>("Id ou nome inválido");

        var perfilExiste = await _repository.ExistePerfilAsync(request.Id, cancellationToken);
        if (!perfilExiste)
            return Result.NotFoundResult<AtualizarNomePerfilResponse>($"Perfil {request.Id} não encontrado");

        await _repository.AtualizarNomeAsync(request.Id, request.NovoNome, cancellationToken);
        return Result.OkResult(new AtualizarNomePerfilResponse { Mensagem = "Alterado com sucesso" });
    }
}
