using Portal.Domain.Equipamento;

namespace Portal.Domain.Equipamento.Interfaces;

public interface IEquipamentoRepository
{
    Task<IEnumerable<EquipamentoQuery>> ObterTodosAsync(CancellationToken cancellationToken);
    Task<IEnumerable<EquipamentoQuery>> ObterPorFiltroAsync(string? nome, string? marca, string? modelo, Guid? categoriaId, bool? ativo, CancellationToken cancellationToken);
    Task<EquipamentoQuery?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Guid> CriarAsync(EquipamentoCommand equipamento, CancellationToken cancellationToken);
    Task<int> AtualizarAsync(EquipamentoCommand equipamento, CancellationToken cancellationToken);
    Task<int> InativarAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExisteNumeroSerieAsync(string numeroSerie, Guid? idIgnorar = null, CancellationToken cancellationToken = default);
    Task<bool> CategoriaExisteAsync(Guid categoriaId, CancellationToken cancellationToken);
}
