using Portal.Application.Cliente.Common;
using Portal.Domain.Base;
using Portal.Domain.Common;
using Portal.Domain.Cliente.Interfaces;

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

    public async Task<Result<TabelaPaginadaResponse<ObterClientesResponse>>> Handle(ObterClientesRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listando todos os clientes");

        var resultado = await _repository.ObterTodosAsync(request.DirecaoEnum, request.Pagina, request.TamanhoPagina, cancellationToken);
        var response = resultado.Itens.Select(c => c.ToResponse<ObterClientesResponse>()).ToList();
        var tabela = TabelaPaginadaResponse<ObterClientesResponse>.Criar(response, resultado.TotalRegistros, resultado.TotalRegistros, resultado.NumeroPagina, resultado.TamanhoPagina);

        return Result.OkResult(tabela);
    }
}
