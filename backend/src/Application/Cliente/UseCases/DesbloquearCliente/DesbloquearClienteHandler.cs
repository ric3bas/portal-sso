using Portal.Domain.Base;
using Portal.Domain.Cliente.Interfaces;

namespace Portal.Application.Cliente.UseCases.DesbloquearCliente;

public class DesbloquearClienteHandler
{
    private readonly IClienteRepository _repository;

    public DesbloquearClienteHandler(IClienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<DesbloquearClienteResponse>> Handle(DesbloquearClienteRequest request, CancellationToken cancellationToken)
    {
        if (!await _repository.ExisteAsync(request.Id, cancellationToken))
        {
            return Result.NotFoundResult<DesbloquearClienteResponse>("Cliente não encontrado");
        }

        var linhasAfetadas = await _repository.DefinirBloqueadoAsync(request.Id, false, cancellationToken);
        if (linhasAfetadas == 0)
        {
            return Result.NotFoundResult<DesbloquearClienteResponse>("Cliente não encontrado");
        }

        return Result.OkResult(new DesbloquearClienteResponse { Mensagem = "Cliente desbloqueado com sucesso" });
    }
}
