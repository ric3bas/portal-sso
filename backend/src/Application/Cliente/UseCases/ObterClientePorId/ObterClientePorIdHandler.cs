using Portal.Application.Cliente.Common;
using Portal.Domain.Base;
using Portal.Domain.Cliente.Interfaces;
using Portal.Domain.Portal.Extensions;

namespace Portal.Application.Cliente.UseCases.ObterClientePorId;

public class ObterClientePorIdHandler
{
    private readonly IClienteRepository _repository;
    private readonly ILogger<ObterClientePorIdHandler> _logger;

    public ObterClientePorIdHandler(IClienteRepository repository, ILogger<ObterClientePorIdHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<ObterClientePorIdResponse>> Handle(ObterClientePorIdRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obtendo cliente por Id: {Id}", request.Id);

        var cliente = await _repository.ObterPorIdAsync(request.Id, cancellationToken);
        if (cliente is null)
        {
            return Result.NotFoundResult<ObterClientePorIdResponse>("Cliente não encontrado");
        }

        return Result.OkResult(cliente.ToResponse<ObterClientePorIdResponse>());
    }
}
