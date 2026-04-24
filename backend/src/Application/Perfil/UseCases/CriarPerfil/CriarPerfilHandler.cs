using Portal.Domain.Base;
using Portal.Domain.Perfil;
using Portal.Domain.Perfil.Interfaces;

namespace Portal.Application.Perfil.UseCases.CriarPerfil;

public class CriarPerfilHandler
{
    private readonly IPerfilRepository _repository;

    public CriarPerfilHandler(IPerfilRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<string>> Handle(CriarPerfilRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsValid()) return Result.ValidationResult<string>(request.ObterErros());

        var nomeExiste = await _repository.ExisteNomeAsync(request.Nome, cancellationToken);
        if (nomeExiste)
            return Result.BusinessResult<string>($"Já existe um perfil com o nome {request.Nome}");

        _ = await _repository.InserirAsync(new PerfilCommand { Nome = request.Nome }, cancellationToken);
        return Result.OkResult("Perfil criado com sucesso");
    }
}
