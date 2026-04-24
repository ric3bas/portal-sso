using Portal.Application.Locacao.Common;
using Portal.Domain.Base;
using Portal.Domain.Locacao.Interfaces;

namespace Portal.Application.Locacao.UseCases.ObterLocacaoPorId;

public class ObterLocacaoPorIdHandler
{
    private readonly ILocacaoRepository _repository;

    public ObterLocacaoPorIdHandler(ILocacaoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ObterLocacaoPorIdResponse>> Handle(ObterLocacaoPorIdRequest request, CancellationToken cancellationToken)
    {
        var locacao = await _repository.ObterPorIdAsync(request.Id, cancellationToken);
        if (locacao is null)
            return Result.NotFoundResult<ObterLocacaoPorIdResponse>("LocaÃ§Ã£o nÃ£o encontrada");

        return Result.OkResult(locacao.ToResponse<ObterLocacaoPorIdResponse>());
    }
}
