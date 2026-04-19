using Portal.Domain.Base;
using Portal.Domain.Escopo.Interfaces;
using Portal.Domain.Portal.Extensions;

namespace Portal.Application.Escopo.UseCases.ObterEscopoPorId;

public class ObterEscopoPorIdHandler
{
    private readonly IEscopoRepository _repository;

    public ObterEscopoPorIdHandler(IEscopoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ObterEscopoPorIdResponse>> Handle(ObterEscopoPorIdRequest request, CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
        {
            return Result.ValidationResult<ObterEscopoPorIdResponse>("Id do escopo inválido");
        }

        var escopo = await _repository.ObterPorIdAsync(request.Id, cancellationToken);
        if (escopo is null)
        {
            return Result.NotFoundResult<ObterEscopoPorIdResponse>("Escopo não encontrado");
        }

        return Result.OkResult(escopo.ToResponse<ObterEscopoPorIdResponse>());
    }
}
