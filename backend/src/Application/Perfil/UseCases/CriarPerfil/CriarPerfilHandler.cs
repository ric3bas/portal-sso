using FluentValidation;
using Portal.Domain.Base;
using Portal.Domain.Perfil;
using Portal.Domain.Perfil.Interfaces;

namespace Portal.Application.Perfil.UseCases.CriarPerfil;

public class CriarPerfilHandler
{
    private readonly IPerfilRepository _repository;
    private readonly IValidator<CriarPerfilRequest> _validator;

    public CriarPerfilHandler(IPerfilRepository repository, IValidator<CriarPerfilRequest> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Result<CriarPerfilResponse>> Handle(CriarPerfilRequest request, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.ValidationResult<CriarPerfilResponse>(validation.Errors.Select(e => e.ErrorMessage));

        var nomeExiste = await _repository.ExisteNomeAsync(request.Nome, cancellationToken);
        if (nomeExiste)
            return Result.BusinessResult<CriarPerfilResponse>($"Já existe um perfil com o nome {request.Nome}");

        var id = await _repository.InserirAsync(new PerfilCommand { Nome = request.Nome }, cancellationToken);
        return Result.OkResult(new CriarPerfilResponse { Id = id, Mensagem = "Criado com sucesso" });
    }
}
