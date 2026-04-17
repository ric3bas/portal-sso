using Portal.Features.Financeiro.Domain;
using Portal.Features.Financeiro.Domain.Interfaces;
using Portal.Infra;

namespace Portal.Features.Financeiro.Infra
{
    public class FinanceiroRepository : DapperRepository, IFinanceiroRepository
    {
        public FinanceiroRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<Guid> CriarLancamentoAsync(Guid locacaoId, DateTime dataDevolucao, CancellationToken cancellationToken)
        {
            var id = Guid.NewGuid();

            const string sql = @"
                INSERT INTO l6a.Financeiro (Id, locacao_id, cliente_id, equipamento_id, data_retirada, 
                                      data_devolucao, dias_locados, valor_diaria, valor_total, data_lancamento, parceiro_id)
                SELECT @Id, 
                       l.Id,
                       l.cliente_id,
                       l.equipamento_id,
                       l.data_retirada,
                       @DataDevolucao,
                       EXTRACT(DAY FROM @DataDevolucao - l.data_retirada) + 1,
                       l.valor_diaria,
                       (EXTRACT(DAY FROM @DataDevolucao - l.data_retirada) + 1) * l.valor_diaria,
                       NOW(),
                       l.parceiro_id
                FROM l6a.Locacao l
                WHERE l.Id = @LocacaoId";

            await ExecuteAsync(sql, new { Id = id, LocacaoId = locacaoId, DataDevolucao = dataDevolucao });
            return id;
        }

        public async Task<IEnumerable<FinanceiroEntity>> ObterTodosAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT f.Id, f.locacao_id as LocacaoId, f.cliente_id as ClienteId, c.Nome as ClienteNome,
                       f.equipamento_id as EquipamentoId, e.Nome as EquipamentoNome,
                       f.data_retirada as DataRetirada, f.data_devolucao as DataDevolucao, 
                       f.dias_locados as DiasLocados, f.valor_diaria as ValorDiaria, 
                       f.valor_total as ValorTotal, f.data_lancamento as DataLancamento,
                       f.parceiro_id as ParceiroId
                FROM l6a.Financeiro f
                INNER JOIN l6a.Cliente c ON f.cliente_id = c.Id
                INNER JOIN l6a.Equipamento e ON f.equipamento_id = e.Id
                ORDER BY f.data_lancamento DESC";

            return await QueryAsync<FinanceiroEntity>(sql);
        }

        public async Task<IEnumerable<FinanceiroEntity>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT f.Id, f.locacao_id as LocacaoId, f.cliente_id as ClienteId, c.Nome as ClienteNome,
                       f.equipamento_id as EquipamentoId, e.Nome as EquipamentoNome,
                       f.data_retirada as DataRetirada, f.data_devolucao as DataDevolucao, 
                       f.dias_locados as DiasLocados, f.valor_diaria as ValorDiaria, 
                       f.valor_total as ValorTotal, f.data_lancamento as DataLancamento,
                       f.parceiro_id as ParceiroId
                FROM l6a.Financeiro f
                INNER JOIN l6a.Cliente c ON f.cliente_id = c.Id
                INNER JOIN l6a.Equipamento e ON f.equipamento_id = e.Id
                WHERE f.data_lancamento >= @DataInicio AND f.data_lancamento <= @DataFim
                ORDER BY f.data_lancamento DESC";

            return await QueryAsync<FinanceiroEntity>(sql, new { DataInicio = dataInicio, DataFim = dataFim });
        }

        public async Task<bool> ExisteLancamentoParaLocacaoAsync(Guid locacaoId, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT COUNT(1) 
                FROM l6a.Financeiro 
                WHERE locacao_id = @LocacaoId";

            var count = await QuerySingleAsync<int>(sql, new { LocacaoId = locacaoId });
            return count > 0;
        }
    }
}