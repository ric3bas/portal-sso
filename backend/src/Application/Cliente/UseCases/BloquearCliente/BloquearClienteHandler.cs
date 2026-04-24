using Portal.Domain.Base;
using Portal.Domain.Cliente.Interfaces;

namespace Portal.Application.Cliente.UseCases.BloquearCliente;

public class BloquearClienteHandler
{
    private readonly IClienteRepository _repository;

    public BloquearClienteHandler(IClienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<BloquearClienteResponse>> Handle(BloquearClienteRequest request, CancellationToken cancellationToken)
    {
        if (!await _repository.ExisteAsync(request.Id, cancellationToken))
        {
            return Result.NotFoundResult<BloquearClienteResponse>("Cliente nÃ£o encontrado");
        }

        var linhasAfetadas = await _repository.DefinirBloqueadoAsync(request.Id, true, cancellationToken);
        if (linhasAfetadas == 0)
        {
            return Result.NotFoundResult<BloquearClienteResponse>("Cliente nÃ£o encontrado");
        }

        return Result.OkResult(new BloquearClienteResponse { Mensagem = "Cliente bloqueado com sucesso" });
    }
}
