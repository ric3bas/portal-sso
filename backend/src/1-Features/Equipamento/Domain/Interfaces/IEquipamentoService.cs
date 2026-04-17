using Portal.Domain.Base;

namespace Portal.Features.Equipamento.Domain.Interfaces
{
    public interface IEquipamentoService
    {
        Task<Result<IEnumerable<EquipamentoResponse>>> ObterTodosEquipamentosAsync(CancellationToken cancellationToken);
        Task<Result<IEnumerable<EquipamentoResponse>>> ObterEquipamentosPorFiltroAsync(FiltroEquipamentoRequest filtro, CancellationToken cancellationToken);
        Task<Result<EquipamentoResponse>> ObterEquipamentoAsync(string? id, CancellationToken cancellationToken);
        Task<Result<string>> CriarEquipamentoAsync(EquipamentoRequest equipamento, CancellationToken cancellationToken);
        Task<Result<string>> AtualizarEquipamentoAsync(AtualizarEquipamentoRequest equipamento, CancellationToken cancellationToken);
        Task<Result<string>> InativarEquipamentoAsync(string? id, CancellationToken cancellationToken);
    }
}