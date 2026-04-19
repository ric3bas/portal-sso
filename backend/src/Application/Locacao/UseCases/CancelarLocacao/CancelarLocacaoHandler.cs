using Portal.Domain.Base;
using Portal.Domain.Locacao;
using Portal.Domain.Locacao.Interfaces;

namespace Portal.Application.Locacao.UseCases.CancelarLocacao;

public class CancelarLocacaoHandler
{
    private readonly ILocacaoRepository _repository;

    public CancelarLocacaoHandler(ILocacaoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CancelarLocacaoResponse>> Handle(CancelarLocacaoRequest request, CancellationToken cancellationToken)
    {
        var locacao = await _repository.ObterPorIdAsync(request.Id, cancellationToken);
        if (locacao is null)
            return Result.NotFoundResult<CancelarLocacaoResponse>("Locação não encontrada");

        if (locacao.Status == StatusLocacao.Devolvida)
            return Result.ValidationResult<CancelarLocacaoResponse>("Locações já devolvidas não podem ser canceladas");

        if (locacao.Status == StatusLocacao.Cancelada)
            return Result.ValidationResult<CancelarLocacaoResponse>("Locação já está cancelada");

        var rows = await _repository.CancelarAsync(request.Id, cancellationToken);
        if (rows == 0)
            return Result.NotFoundResult<CancelarLocacaoResponse>("Locação não encontrada");

        return Result.OkResult(new CancelarLocacaoResponse { Mensagem = "Locação cancelada com sucesso" });
    }
}
