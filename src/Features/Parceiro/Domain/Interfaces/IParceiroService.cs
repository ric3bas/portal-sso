namespace Portal.Features.Parceiro.Domain.Interfaces {
    public interface IParceiroService {
        Task<IEnumerable<ParceiroResponse>> ListarParceirosAsync(string? nome, CancellationToken cancellationToken);
        Task<ParceiroResponse?> ObterParceiroAsync(string? id, CancellationToken cancellationToken);
        Task<Guid> CriarParceiroAsync(ParceiroRequest parceiro, CancellationToken cancellationToken);
        Task AtualizarParceiroAsync(AtualizarParceiroRequest request, CancellationToken cancellationToken);
    }
 }