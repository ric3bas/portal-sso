using Portal.Domain.Equipamento;
using Portal.Domain.Common;

namespace Portal.Domain.Equipamento.Interfaces;

public interface IEquipamentoRepository
{
    Task<ResultadoPaginado<EquipamentoQuery>> ObterTodosAsync(Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken);
    Task<ResultadoPaginado<EquipamentoQuery>> ObterPorFiltroAsync(string? nome, string? marca, string? modelo, Guid? categoriaId, bool? ativo, Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken);
    Task<EquipamentoQuery?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Guid> CriarAsync(EquipamentoCommand equipamento, CancellationToken cancellationToken);
    Task<int> AtualizarAsync(EquipamentoCommand equipamento, CancellationToken cancellationToken);
    Task<int> InativarAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExisteNumeroSerieAsync(string numeroSerie, Guid? idIgnorar = null, CancellationToken cancellationToken = default);
    Task<bool> CategoriaExisteAsync(Guid categoriaId, CancellationToken cancellationToken);
}
