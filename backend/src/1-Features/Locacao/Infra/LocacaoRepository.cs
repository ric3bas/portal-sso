using Portal.Features.Locacao.Domain;
using Portal.Features.Locacao.Domain.Interfaces;
using Portal.Infra;

namespace Portal.Features.Locacao.Infra
{
    public class LocacaoRepository : DapperRepository, ILocacaoRepository
    {
        public LocacaoRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<IEnumerable<LocacaoEntity>> ObterTodasAsync(CancellationToken cancellationToken)
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
                ORDER BY l.data_retirada DESC";

            return await QueryAsync<LocacaoEntity>(sql);
        }

        public async Task<IEnumerable<LocacaoEntity>> ObterPorFiltroAsync(FiltroLocacaoRequest filtro, CancellationToken cancellationToken)
        {
            var whereConditions = new List<string>();
            var parameters = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(filtro.ClienteId) && Guid.TryParse(filtro.ClienteId, out var clienteId))
            {
                whereConditions.Add("l.cliente_id = @ClienteId");
                parameters.Add("ClienteId", clienteId);
            }

            if (!string.IsNullOrWhiteSpace(filtro.EquipamentoId) && Guid.TryParse(filtro.EquipamentoId, out var equipamentoId))
            {
                whereConditions.Add("l.equipamento_id = @EquipamentoId");
                parameters.Add("EquipamentoId", equipamentoId);
            }

            if (filtro.Status.HasValue)
            {
                whereConditions.Add("l.Status = @Status");
                parameters.Add("Status", (int)filtro.Status.Value);
            }

            if (filtro.DataRetiradaInicio.HasValue)
            {
                whereConditions.Add("l.data_retirada >= @DataRetiradaInicio");
                parameters.Add("DataRetiradaInicio", filtro.DataRetiradaInicio.Value.Date);
            }

            if (filtro.DataRetiradaFim.HasValue)
            {
                whereConditions.Add("l.data_retirada <= @DataRetiradaFim");
                parameters.Add("DataRetiradaFim", filtro.DataRetiradaFim.Value.Date.AddDays(1).AddTicks(-1));
            }

            var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";

            var sql = $@"
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
                ORDER BY l.data_retirada DESC";

            return await QueryAsync<LocacaoEntity>(sql, parameters);
        }

        public async Task<LocacaoEntity?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
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

            return await QuerySingleAsync<LocacaoEntity>(sql, new { Id = id });
        }

        public async Task<Guid> CriarAsync(LocacaoRequest locacao, CancellationToken cancellationToken)
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
                ClienteId = Guid.Parse(locacao.ClienteId),
                EquipamentoId = Guid.Parse(locacao.EquipamentoId),
                Status = (int)StatusLocacao.Ativa,
                locacao.DataRetirada,
                locacao.PrevisaoDevolucao,
                locacao.ValorDiaria,
                Observacao = locacao.Observacao ?? string.Empty
            });

            return id;
        }

        public async Task<int> AtualizarAsync(Guid id, AtualizarLocacaoRequest locacao, CancellationToken cancellationToken)
        {
            const string sql = @"
                UPDATE l6a.Locacao 
                SET cliente_id = @ClienteId,
                    equipamento_id = @EquipamentoId,
                    data_retirada = @DataRetirada,
                    previsao_devolucao = @PrevisaoDevolucao,
                    valor_diaria = @ValorDiaria,
                    Observacao = @Observacao
                WHERE Id = @Id AND Status = @StatusAtiva AND parceiro_id = @ParceiroId";

            return await ExecuteAsync(sql, new
            {
                Id = id,
                ClienteId = Guid.Parse(locacao.ClienteId),
                EquipamentoId = Guid.Parse(locacao.EquipamentoId),
                locacao.DataRetirada,
                locacao.PrevisaoDevolucao,
                locacao.ValorDiaria,
                Observacao = locacao.Observacao ?? string.Empty,
                StatusAtiva = (int)StatusLocacao.Ativa,
                ParceiroId = Guid.Parse("00000000-0000-0000-0000-000000000001") // temp
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
                FROM Cliente 
                WHERE Id = @ClienteId AND Ativo = 1";

            var count = await QuerySingleAsync<int>(sql, new { ClienteId = clienteId });
            return count > 0;
        }

        public async Task<bool> EquipamentoExisteAsync(Guid equipamentoId, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT COUNT(1) 
                FROM Equipamento 
                WHERE Id = @EquipamentoId AND Ativo = 1";

            var count = await QuerySingleAsync<int>(sql, new { EquipamentoId = equipamentoId });
            return count > 0;
        }

        public async Task<bool> ClienteBloqueadoAsync(Guid clienteId, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT ISNULL(Bloqueado, 0) 
                FROM Cliente 
                WHERE Id = @ClienteId";

            var bloqueado = await QuerySingleAsync<bool?>(sql, new { ClienteId = clienteId });
            return bloqueado ?? false;
        }

        public async Task<bool> EquipamentoDisponivelAsync(Guid equipamentoId, DateTime dataRetirada, DateTime previsaoDevolucao, Guid? locacaoIdIgnorar = null, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT COUNT(1) 
                FROM Locacao 
                WHERE EquipamentoId = @EquipamentoId 
                AND Status IN (@StatusAtiva, @StatusAtrasada)
                AND (
                    (@DataRetirada <= PrevisaoDevolucao AND @PrevisaoDevolucao >= DataRetirada)
                )
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

        public async Task<IEnumerable<LocacaoEntity>> ObterAtrasadasAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT l.Id, l.ClienteId, c.Nome as ClienteNome,
                       l.EquipamentoId, e.Nome as EquipamentoNome,
                       l.Status, l.DataRetirada, l.PrevisaoDevolucao, l.DataDevolucaoReal,
                       l.ValorDiaria, l.Observacao,
                       NULL as DiasLocados, NULL as ValorTotal
                FROM Locacao l
                INNER JOIN Cliente c ON l.ClienteId = c.Id
                INNER JOIN Equipamento e ON l.EquipamentoId = e.Id
                WHERE l.Status = @StatusAtrasada
                ORDER BY l.PrevisaoDevolucao";

            return await QueryAsync<LocacaoEntity>(sql, new { StatusAtrasada = (int)StatusLocacao.Atrasada });
        }

        public async Task AtualizarStatusAtrasadasAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
                UPDATE Locacao 
                SET Status = @StatusAtrasada
                WHERE Status = @StatusAtiva 
                AND PrevisaoDevolucao < @DataAtual";

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
                SELECT PrecoDiaria 
                FROM Equipamento 
                WHERE Id = @EquipamentoId";

            var valor = await QuerySingleAsync<decimal?>(sql, new { EquipamentoId = equipamentoId });
            return valor ?? 0;
        }

    }
}