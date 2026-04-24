using Dapper;
using Portal.Domain.Financeiro;
using Portal.Domain.Financeiro.Interfaces;
using Portal.Domain.Common;
using DataDapperRepository = Portal.Infrastructure.Data.DapperRepository;
using DataUnitOfWork = Portal.Infrastructure.Data.IUnitOfWork;

namespace Portal.Infrastructure.Repositories;

public class FinanceiroRepository : DataDapperRepository, IFinanceiroRepository
{
    public FinanceiroRepository(DataUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<Guid> CriarLancamentoAsync(FinanceiroCommand lancamento, CancellationToken cancellationToken)
    {
        var id = lancamento.Id == Guid.Empty ? Guid.NewGuid() : lancamento.Id;

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

        await ExecuteAsync(sql, new { Id = id, lancamento.LocacaoId, lancamento.DataDevolucao });
        return id;
    }

    public async Task<ResultadoPaginado<FinanceiroQuery>> ObterTodosAsync(Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken)
    {
        var paginacao = PaginacaoHelper.Criar(direcao, pagina, tamanhoPagina);

        var sql = $@"
            SELECT COUNT(1)
            FROM l6a.Financeiro f
            INNER JOIN l6a.Cliente c ON f.cliente_id = c.Id
            INNER JOIN l6a.Equipamento e ON f.equipamento_id = e.Id;

            SELECT f.Id, f.locacao_id as LocacaoId, f.cliente_id as ClienteId, c.Nome as ClienteNome,
                   f.equipamento_id as EquipamentoId, e.Nome as EquipamentoNome,
                   f.data_retirada as DataRetirada, f.data_devolucao as DataDevolucao,
                   f.dias_locados as DiasLocados, f.valor_diaria as ValorDiaria,
                   f.valor_total as ValorTotal, f.data_lancamento as DataLancamento,
                   f.parceiro_id as ParceiroId
            FROM l6a.Financeiro f
            INNER JOIN l6a.Cliente c ON f.cliente_id = c.Id
            INNER JOIN l6a.Equipamento e ON f.equipamento_id = e.Id
            ORDER BY c.Nome {paginacao.OrdemSql}
            LIMIT @TamanhoPagina OFFSET @Offset";

        using var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, new { TamanhoPagina = paginacao.TamanhoPagina, Offset = paginacao.Offset });
        var total = await multi.ReadSingleAsync<int>();
        var itens = (await multi.ReadAsync<FinanceiroQuery>()).ToList();
        return new ResultadoPaginado<FinanceiroQuery>(itens, total, paginacao.Pagina, paginacao.TamanhoPagina);
    }

    public async Task<ResultadoPaginado<FinanceiroQuery>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken)
    {
        var paginacao = PaginacaoHelper.Criar(direcao, pagina, tamanhoPagina);

        var sql = $@"
            SELECT COUNT(1)
            FROM l6a.Financeiro f
            INNER JOIN l6a.Cliente c ON f.cliente_id = c.Id
            INNER JOIN l6a.Equipamento e ON f.equipamento_id = e.Id
            WHERE f.data_lancamento >= @DataInicio AND f.data_lancamento <= @DataFim;

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
            ORDER BY c.Nome {paginacao.OrdemSql}
            LIMIT @TamanhoPagina OFFSET @Offset";

        var param = new { DataInicio = dataInicio, DataFim = dataFim, TamanhoPagina = paginacao.TamanhoPagina, Offset = paginacao.Offset };
        using var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, param);
        var total = await multi.ReadSingleAsync<int>();
        var itens = (await multi.ReadAsync<FinanceiroQuery>()).ToList();
        return new ResultadoPaginado<FinanceiroQuery>(itens, total, paginacao.Pagina, paginacao.TamanhoPagina);
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
