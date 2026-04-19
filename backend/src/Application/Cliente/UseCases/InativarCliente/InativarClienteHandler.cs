using Portal.Domain.Base;
using Portal.Domain.Cliente.Interfaces;

namespace Portal.Application.Cliente.UseCases.InativarCliente;

public class InativarClienteHandler
{
    private readonly IClienteRepository _repository;

    public InativarClienteHandler(IClienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<InativarClienteResponse>> Handle(InativarClienteRequest request, CancellationToken cancellationToken)
    {
        if (!await _repository.ExisteAsync(request.Id, cancellationToken))
        {
            return Result.NotFoundResult<InativarClienteResponse>("Cliente não encontrado");
        }

        var linhasAfetadas = await _repository.InativarAsync(request.Id, cancellationToken);
        if (linhasAfetadas == 0)
        {
            return Result.NotFoundResult<InativarClienteResponse>("Cliente não encontrado");
        }

        return Result.OkResult(new InativarClienteResponse { Mensagem = "Cliente inativado com sucesso" });
    }
}
