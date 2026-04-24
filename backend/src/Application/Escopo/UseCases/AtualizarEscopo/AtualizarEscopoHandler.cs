using Portal.Domain.Base;
using Portal.Domain.Escopo;
using Portal.Domain.Escopo.Interfaces;

namespace Portal.Application.Escopo.UseCases.AtualizarEscopo;

public class AtualizarEscopoHandler
{
    private readonly IEscopoRepository _repository;

    public AtualizarEscopoHandler(IEscopoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<string>> Handle(AtualizarEscopoRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsValid()) return Result.ValidationResult<string>(request.ObterErros());

        var atual = await _repository.ObterPorIdAsync(request.Id, cancellationToken);
        if (atual is null)
        {
            return Result.NotFoundResult<string>("Escopo não encontrado");
        }

        if (await _repository.ExisteNomeAsync(request.Nome.Trim(), request.Id, cancellationToken))
        {
            return Result.BusinessResult<string>("Nome do escopo já existe");
        }

        await _repository.AtualizarAsync(new EscopoCommand { Id = request.Id, Nome = request.Nome.Trim() }, cancellationToken);
        return Result.OkResult("Escopo atualizado com sucesso");
    }
}
