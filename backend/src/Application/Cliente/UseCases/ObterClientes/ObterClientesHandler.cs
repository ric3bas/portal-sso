using Portal.Application.Cliente.Common;
using Portal.Domain.Base;
using Portal.Domain.Cliente.Interfaces;
using Portal.Domain.Portal.Extensions;

namespace Portal.Application.Cliente.UseCases.ObterClientes;

public class ObterClientesHandler
{
    private readonly IClienteRepository _repository;
    private readonly ILogger<ObterClientesHandler> _logger;

    public ObterClientesHandler(IClienteRepository repository, ILogger<ObterClientesHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<ObterClientesResponse>>> Handle(ObterClientesRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listando todos os clientes");

        var clientes = await _repository.ObterTodosAsync(cancellationToken);
        if (clientes == null || !clientes.Any())
        {
            return Result.NotFoundResult<List<ObterClientesResponse>>("Nenhum cliente encontrado");
        }

        return Result.OkResult(clientes.Select(c => c.ToResponse<ObterClientesResponse>()).ToList());
    }
}
