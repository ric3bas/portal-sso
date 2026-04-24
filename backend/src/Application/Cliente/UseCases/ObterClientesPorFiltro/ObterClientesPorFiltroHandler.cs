using Portal.Application.Cliente.Common;
using Portal.Domain.Base;
using Portal.Domain.Common;
using Portal.Domain.Cliente.Interfaces;

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

    public async Task<Result<TabelaPaginadaResponse<ObterClientesPorFiltroResponse>>> Handle(ObterClientesPorFiltroRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listando clientes por filtro");

        var resultado = await _repository.ObterPorFiltroAsync(request.Nome, request.Cpf, request.DirecaoEnum, request.Pagina, request.TamanhoPagina, cancellationToken);
        var response = resultado.Itens.Select(c => c.ToResponse<ObterClientesPorFiltroResponse>()).ToList();
        var tabela = TabelaPaginadaResponse<ObterClientesPorFiltroResponse>.Criar(response, resultado.TotalRegistros, resultado.TotalRegistros, resultado.NumeroPagina, resultado.TamanhoPagina);

        return Result.OkResult(tabela);
    }
}
