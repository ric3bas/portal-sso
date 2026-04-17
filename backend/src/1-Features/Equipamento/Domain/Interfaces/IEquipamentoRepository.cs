using Portal.Features.Equipamento.Domain;

namespace Portal.Features.Equipamento.Domain.Interfaces
{
    public interface IEquipamentoRepository
    {
        Task<IEnumerable<EquipamentoEntity>> ObterTodosAsync(CancellationToken cancellationToken);
        Task<IEnumerable<EquipamentoEntity>> ObterPorFiltroAsync(FiltroEquipamentoRequest filtro, CancellationToken cancellationToken);
        Task<EquipamentoEntity?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Guid> CriarAsync(EquipamentoRequest equipamento, CancellationToken cancellationToken);
        Task<int> AtualizarAsync(Guid id, AtualizarEquipamentoRequest equipamento, CancellationToken cancellationToken);
        Task<int> InativarAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> ExisteNumeroSerieAsync(string numeroSerie, Guid? idIgnorar = null, CancellationToken cancellationToken = default);
        Task<bool> CategoriaExisteAsync(Guid categoriaId, CancellationToken cancellationToken);
    }
}