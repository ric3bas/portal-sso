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
            return Result.NotFoundResult<CancelarLocacaoResponse>("LocaÃ§Ã£o nÃ£o encontrada");

        if (locacao.Status == StatusLocacao.Devolvida)
            return Result.ValidationResult<CancelarLocacaoResponse>("LocaÃ§Ãµes jÃ¡ devolvidas nÃ£o podem ser canceladas");

        if (locacao.Status == StatusLocacao.Cancelada)
            return Result.ValidationResult<CancelarLocacaoResponse>("LocaÃ§Ã£o jÃ¡ estÃ¡ cancelada");

        var rows = await _repository.CancelarAsync(request.Id, cancellationToken);
        if (rows == 0)
            return Result.NotFoundResult<CancelarLocacaoResponse>("LocaÃ§Ã£o nÃ£o encontrada");

        return Result.OkResult(new CancelarLocacaoResponse { Mensagem = "LocaÃ§Ã£o cancelada com sucesso" });
    }
}
