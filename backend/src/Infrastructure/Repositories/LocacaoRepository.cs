using Dapper;
using Portal.Domain.Locacao;
using Portal.Domain.Locacao.Interfaces;
using Portal.Domain.Common;
using DataDapperRepository = Portal.Infrastructure.Data.DapperRepository;
using DataUnitOfWork = Portal.Infrastructure.Data.IUnitOfWork;

namespace Portal.Infrastructure.Repositories;

public class LocacaoRepository : DataDapperRepository, ILocacaoRepository
{
    public LocacaoRepository(DataUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<ResultadoPaginado<LocacaoQuery>> ObterTodasAsync(Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken)
    {
        var paginacao = PaginacaoHelper.Criar(direcao, pagina, tamanhoPagina);

        var sql = $@"
            SELECT COUNT(1)
            FROM l6a.Locacao l
            INNER JOIN l6a.Cliente c ON l.cliente_id = c.Id
            INNER JOIN l6a.Equipamento e ON l.equipamento_id = e.Id;

            SELECT l.Id, l.cliente_id as ClienteId, c.Nome as ClienteNome,
                   l.equipamento_id as EquipamentoId, e.Nome as EquipamentoNome,
                   l.Status, l.data_retirada as DataRetirada, l.previsao_devolucao as PrevisaoDevolucao,
                   l.data_devolucao_real as DataDevolucaoReal, l.valor_diaria as ValorDiaria, l.Observacao,
                   l.parceiro_id as ParceiroId,
                   CASE
                       WHEN l.data_devolucao_real IS NOT NULL
                       THEN EXTRACT(DAY FROM l.data_devolucao_real - l.data_retirada) + 1
                       ELSE NULL
                   END as DiasLocados,
                   CASE
                       WHEN l.data_devolucao_real IS NOT NULL
                       THEN (EXTRACT(DAY FROM l.data_devolucao_real - l.data_retirada) + 1) * l.valor_diaria
                       ELSE NULL
                   END as ValorTotal
            FROM l6a.Locacao l
            INNER JOIN l6a.Cliente c ON l.cliente_id = c.Id
            INNER JOIN l6a.Equipamento e ON l.equipamento_id = e.Id
            ORDER BY c.Nome {paginacao.OrdemSql}
            LIMIT @TamanhoPagina OFFSET @Offset";

        using var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, new { TamanhoPagina = paginacao.TamanhoPagina, Offset = paginacao.Offset });
        var total = await multi.ReadSingleAsync<int>();
        var itens = (await multi.ReadAsync<LocacaoQuery>()).ToList();
        return new ResultadoPaginado<LocacaoQuery>(itens, total, paginacao.Pagina, paginacao.TamanhoPagina);
    }

    public async Task<ResultadoPaginado<LocacaoQuery>> ObterPorFiltroAsync(Guid? clienteId, Guid? equipamentoId, StatusLocacao? status, DateTime? dataRetiradaInicio, DateTime? dataRetiradaFim, Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken)
    {
        var whereConditions = new List<string>();
        var parameters = new Dictionary<string, object>();

        if (clienteId.HasValue)
        {
            whereConditions.Add("l.cliente_id = @ClienteId");
            parameters.Add("ClienteId", clienteId.Value);
        }

        if (equipamentoId.HasValue)
        {
            whereConditions.Add("l.equipamento_id = @EquipamentoId");
            parameters.Add("EquipamentoId", equipamentoId.Value);
        }

        if (status.HasValue)
        {
            whereConditions.Add("l.Status = @Status");
            parameters.Add("Status", (int)status.Value);
        }

        if (dataRetiradaInicio.HasValue)
        {
            whereConditions.Add("l.data_retirada >= @DataRetiradaInicio");
            parameters.Add("DataRetiradaInicio", dataRetiradaInicio.Value.Date);
        }

        if (dataRetiradaFim.HasValue)
        {
            whereConditions.Add("l.data_retirada <= @DataRetiradaFim");
            parameters.Add("DataRetiradaFim", dataRetiradaFim.Value.Date.AddDays(1).AddTicks(-1));
        }

        var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : string.Empty;

        var paginacao = PaginacaoHelper.Criar(direcao, pagina, tamanhoPagina);

        var sql = $@"
            SELECT COUNT(1)
            FROM l6a.Locacao l
            INNER JOIN l6a.Cliente c ON l.cliente_id = c.Id
            INNER JOIN l6a.Equipamento e ON l.equipamento_id = e.Id
            {whereClause};

            SELECT l.Id, l.cliente_id as ClienteId, c.Nome as ClienteNome,
                   l.equipamento_id as EquipamentoId, e.Nome as EquipamentoNome,
                   l.Status, l.data_retirada as DataRetirada, l.previsao_devolucao as PrevisaoDevolucao,
                   l.data_devolucao_real as DataDevolucaoReal, l.valor_diaria as ValorDiaria, l.Observacao,
                   l.parceiro_id as ParceiroId,
                   CASE
                       WHEN l.data_devolucao_real IS NOT NULL
                       THEN EXTRACT(DAY FROM l.data_devolucao_real - l.data_retirada) + 1
                       ELSE NULL
                   END as DiasLocados,
                   CASE
                       WHEN l.data_devolucao_real IS NOT NULL
                       THEN (EXTRACT(DAY FROM l.data_devolucao_real - l.data_retirada) + 1) * l.valor_diaria
                       ELSE NULL
                   END as ValorTotal
            FROM l6a.Locacao l
            INNER JOIN l6a.Cliente c ON l.cliente_id = c.Id
            INNER JOIN l6a.Equipamento e ON l.equipamento_id = e.Id
            {whereClause}
            ORDER BY c.Nome {paginacao.OrdemSql}
            LIMIT @TamanhoPagina OFFSET @Offset";

        parameters["TamanhoPagina"] = paginacao.TamanhoPagina;
        parameters["Offset"] = paginacao.Offset;

        using var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, parameters);
        var total = await multi.ReadSingleAsync<int>();
        var itens = (await multi.ReadAsync<LocacaoQuery>()).ToList();
        return new ResultadoPaginado<LocacaoQuery>(itens, total, paginacao.Pagina, paginacao.TamanhoPagina);
    }

    public async Task<LocacaoQuery?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT l.Id, l.cliente_id as ClienteId, c.Nome as ClienteNome,
                   l.equipamento_id as EquipamentoId, e.Nome as EquipamentoNome,
                   l.Status, l.data_retirada as DataRetirada, l.previsao_devolucao as PrevisaoDevolucao,
                   l.data_devolucao_real as DataDevolucaoReal, l.valor_diaria as ValorDiaria, l.Observacao,
                   l.parceiro_id as ParceiroId,
                   CASE
                       WHEN l.data_devolucao_real IS NOT NULL
                       THEN EXTRACT(DAY FROM l.data_devolucao_real - l.data_retirada) + 1
                       ELSE NULL
                   END as DiasLocados,
                   CASE
                       WHEN l.data_devolucao_real IS NOT NULL
                       THEN (EXTRACT(DAY FROM l.data_devolucao_real - l.data_retirada) + 1) * l.valor_diaria
                       ELSE NULL
                   END as ValorTotal
            FROM l6a.Locacao l
            INNER JOIN l6a.Cliente c ON l.cliente_id = c.Id
            INNER JOIN l6a.Equipamento e ON l.equipamento_id = e.Id
            WHERE l.Id = @Id";

        return await QuerySingleAsync<LocacaoQuery>(sql, new { Id = id });
    }

    public async Task<Guid> CriarAsync(LocacaoCommand locacao, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();

        const string sql = @"
            INSERT INTO l6a.Locacao (Id, cliente_id, equipamento_id, Status, data_retirada,
                               previsao_devolucao, valor_diaria, Observacao, parceiro_id)
            VALUES (@Id, @ClienteId, @EquipamentoId, @Status, @DataRetirada,
                    @PrevisaoDevolucao, @ValorDiaria, @Observacao, @ParceiroId)";

        await ExecuteAsync(sql, new
        {
            Id = id,
            locacao.ClienteId,
            locacao.EquipamentoId,
            Status = (int)StatusLocacao.Ativa,
            locacao.DataRetirada,
            locacao.PrevisaoDevolucao,
            locacao.ValorDiaria,
            Observacao = locacao.Observacao ?? string.Empty,
            ParceiroId = Guid.Parse("00000000-0000-0000-0000-000000000001")
        });

        return id;
    }

    public async Task<int> AtualizarAsync(LocacaoCommand locacao, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE l6a.Locacao
            SET cliente_id = @ClienteId,
                equipamento_id = @EquipamentoId,
                data_retirada = @DataRetirada,
                previsao_devolucao = @PrevisaoDevolucao,
                valor_diaria = @ValorDiaria,
                Observacao = @Observacao
            WHERE Id = @Id AND Status = @StatusAtiva";

        return await ExecuteAsync(sql, new
        {
            locacao.Id,
            locacao.ClienteId,
            locacao.EquipamentoId,
            locacao.DataRetirada,
            locacao.PrevisaoDevolucao,
            locacao.ValorDiaria,
            Observacao = locacao.Observacao ?? string.Empty,
            StatusAtiva = (int)StatusLocacao.Ativa
        });
    }

    public async Task<int> DevolverAsync(Guid id, DateTime dataDevolucao, string? observacao, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE l6a.Locacao
            SET Status = @StatusDevolvida,
                data_devolucao_real = @DataDevolucao,
                Observacao = CASE
                               WHEN @Observacao IS NOT NULL AND @Observacao != ''
                               THEN @Observacao
                               ELSE Observacao
                             END
            WHERE Id = @Id AND Status IN (@StatusAtiva, @StatusAtrasada)";

        return await ExecuteAsync(sql, new
        {
            Id = id,
            StatusDevolvida = (int)StatusLocacao.Devolvida,
            DataDevolucao = dataDevolucao,
            Observacao = observacao,
            StatusAtiva = (int)StatusLocacao.Ativa,
            StatusAtrasada = (int)StatusLocacao.Atrasada
        });
    }

    public async Task<int> CancelarAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE l6a.Locacao
            SET Status = @StatusCancelada
            WHERE Id = @Id AND Status != @StatusDevolvida";

        return await ExecuteAsync(sql, new
        {
            Id = id,
            StatusCancelada = (int)StatusLocacao.Cancelada,
            StatusDevolvida = (int)StatusLocacao.Devolvida
        });
    }

    public async Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM l6a.Locacao
            WHERE Id = @Id";

        var count = await QuerySingleAsync<int>(sql, new { Id = id });
        return count > 0;
    }

    public async Task<bool> ClienteExisteAsync(Guid clienteId, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM l6a.Cliente
            WHERE Id = @ClienteId AND Ativo = true";

        var count = await QuerySingleAsync<int>(sql, new { ClienteId = clienteId });
        return count > 0;
    }

    public async Task<bool> EquipamentoExisteAsync(Guid equipamentoId, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM l6a.Equipamento
            WHERE Id = @EquipamentoId AND Ativo = true";

        var count = await QuerySingleAsync<int>(sql, new { EquipamentoId = equipamentoId });
        return count > 0;
    }

    public async Task<bool> ClienteBloqueadoAsync(Guid clienteId, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT Bloqueado
            FROM l6a.Cliente
            WHERE Id = @ClienteId";

        var bloqueado = await QuerySingleAsync<bool?>(sql, new { ClienteId = clienteId });
        return bloqueado ?? false;
    }

    public async Task<bool> EquipamentoDisponivelAsync(Guid equipamentoId, DateTime dataRetirada, DateTime previsaoDevolucao, Guid? locacaoIdIgnorar = null, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM l6a.Locacao
            WHERE equipamento_id = @EquipamentoId
              AND Status IN (@StatusAtiva, @StatusAtrasada)
              AND (@DataRetirada <= previsao_devolucao AND @PrevisaoDevolucao >= data_retirada)
              AND (@LocacaoIdIgnorar IS NULL OR Id != @LocacaoIdIgnorar)";

        var count = await QuerySingleAsync<int>(sql, new
        {
            EquipamentoId = equipamentoId,
            DataRetirada = dataRetirada.Date,
            PrevisaoDevolucao = previsaoDevolucao.Date,
            LocacaoIdIgnorar = locacaoIdIgnorar,
            StatusAtiva = (int)StatusLocacao.Ativa,
            StatusAtrasada = (int)StatusLocacao.Atrasada
        });

        return count == 0;
    }

    public async Task<ResultadoPaginado<LocacaoQuery>> ObterAtrasadasAsync(Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken)
    {
        var paginacao = PaginacaoHelper.Criar(direcao, pagina, tamanhoPagina);

        var sql = $@"
            SELECT COUNT(1)
            FROM l6a.Locacao l
            WHERE l.Status = @StatusAtrasada;

            SELECT l.Id, l.cliente_id as ClienteId, c.Nome as ClienteNome,
                   l.equipamento_id as EquipamentoId, e.Nome as EquipamentoNome,
                   l.Status, l.data_retirada as DataRetirada, l.previsao_devolucao as PrevisaoDevolucao,
                   l.data_devolucao_real as DataDevolucaoReal, l.valor_diaria as ValorDiaria, l.Observacao,
                   l.parceiro_id as ParceiroId,
                   NULL as DiasLocados, NULL as ValorTotal
            FROM l6a.Locacao l
            INNER JOIN l6a.Cliente c ON l.cliente_id = c.Id
            INNER JOIN l6a.Equipamento e ON l.equipamento_id = e.Id
            WHERE l.Status = @StatusAtrasada
            ORDER BY c.Nome {paginacao.OrdemSql}
            LIMIT @TamanhoPagina OFFSET @Offset";

        var param = new { StatusAtrasada = (int)StatusLocacao.Atrasada, TamanhoPagina = paginacao.TamanhoPagina, Offset = paginacao.Offset };
        using var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, param);
        var total = await multi.ReadSingleAsync<int>();
        var itens = (await multi.ReadAsync<LocacaoQuery>()).ToList();
        return new ResultadoPaginado<LocacaoQuery>(itens, total, paginacao.Pagina, paginacao.TamanhoPagina);
    }

    public async Task AtualizarStatusAtrasadasAsync(CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE l6a.Locacao
            SET Status = @StatusAtrasada
            WHERE Status = @StatusAtiva
              AND previsao_devolucao < @DataAtual";

        await ExecuteAsync(sql, new
        {
            StatusAtrasada = (int)StatusLocacao.Atrasada,
            StatusAtiva = (int)StatusLocacao.Ativa,
            DataAtual = DateTime.Today
        });
    }

    public async Task<decimal> ObterValorDiariaEquipamentoAsync(Guid equipamentoId, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT preco_diaria
            FROM l6a.Equipamento
            WHERE Id = @EquipamentoId";

        var valor = await QuerySingleAsync<decimal?>(sql, new { EquipamentoId = equipamentoId });
        return valor ?? 0;
    }
}
