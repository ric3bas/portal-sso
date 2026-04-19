using Portal.Application.Cliente.Common;
using Portal.Domain.Base;
using Portal.Domain.Cliente.Interfaces;
using Portal.Domain.Portal.Extensions;

namespace Portal.Application.Cliente.UseCases.ObterClientesPorFiltro;

public class ObterClientesPorFiltroHandler
{
    private readonly IClienteRepository _repository;
    private readonly ILogger<ObterClientesPorFiltroHandler> _logger;

    public ObterClientesPorFiltroHandler(IClienteRepository repository, ILogger<ObterClientesPorFiltroHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<ObterClientesPorFiltroResponse>>> Handle(ObterClientesPorFiltroRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listando clientes por filtro");

        var clientes = await _repository.ObterPorFiltroAsync(request.Nome, request.Cpf, cancellationToken);
        if (clientes == null || !clientes.Any())
        {
            return Result.NotFoundResult<List<ObterClientesPorFiltroResponse>>("Nenhum cliente encontrado");
        }

        return Result.OkResult(clientes.Select(c => c.ToResponse<ObterClientesPorFiltroResponse>()).ToList());
    }
}
