using Portal.Domain.Base;

namespace Portal.Features.Parceiro.Domain.Interfaces {
    public interface IParceiroService {
        Task<Result<IEnumerable<ParceiroResponse>>> ListarParceirosAsync(string? nome, CancellationToken cancellationToken);
        Task<Result<ParceiroResponse>> ObterParceiroAsync(string? id, CancellationToken cancellationToken);
        Task<Result<string>> CriarParceiroAsync(ParceiroRequest parceiro, CancellationToken cancellationToken);
        Task<Result<string>> AtualizarParceiroAsync(AtualizarParceiroRequest request, CancellationToken cancellationToken);
    }
 }
