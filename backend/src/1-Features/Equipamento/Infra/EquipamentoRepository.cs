using Portal.Features.Equipamento.Domain;
using Portal.Features.Equipamento.Domain.Interfaces;
using Portal.Infra;
using Dapper;

namespace Portal.Features.Equipamento.Infra
{
    public class EquipamentoRepository : DapperRepository, IEquipamentoRepository
    {
        public EquipamentoRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<IEnumerable<EquipamentoEntity>> ObterTodosAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT e.Id, e.Nome, e.categoria_id as CategoriaId, e.quantidade_estoque as QuantidadeEstoque, 
                       e.preco_diaria as PrecoDiaria, e.Marca, e.Modelo, e.numero_serie as NumeroSerie, 
                       e.ano_fabricacao as AnoFabricacao, e.Descricao, e.observacao_internas as ObservacaoInternas, 
                       e.Ativo, e.parceiro_id as ParceiroId, c.Nome as CategoriaNome
                FROM l6a.Equipamento e
                INNER JOIN l6a.Categoria c ON e.categoria_id = c.Id
                ORDER BY e.Nome";

            return await QueryAsync<EquipamentoEntity>(sql);
        }

        public async Task<IEnumerable<EquipamentoEntity>> ObterPorFiltroAsync(FiltroEquipamentoRequest filtro, CancellationToken cancellationToken)
        {
            var whereClause = new List<string>();
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(filtro.Nome))
            {
                whereClause.Add("e.Nome LIKE @Nome");
                parameters.Add("Nome", $"%{filtro.Nome}%");
            }

            if (!string.IsNullOrWhiteSpace(filtro.CategoriaId) && Guid.TryParse(filtro.CategoriaId, out var categoriaId))
            {
                whereClause.Add("e.categoria_id = @CategoriaId");
                parameters.Add("CategoriaId", categoriaId);
            }

            if (!string.IsNullOrWhiteSpace(filtro.Marca))
            {
                whereClause.Add("e.Marca LIKE @Marca");
                parameters.Add("Marca", $"%{filtro.Marca}%");
            }

            if (filtro.Ativo.HasValue)
            {
                whereClause.Add("e.Ativo = @Ativo");
                parameters.Add("Ativo", filtro.Ativo.Value);
            }

            var whereString = whereClause.Count > 0 ? $"WHERE {string.Join(" AND ", whereClause)}" : "";

            var sql = $@"
                SELECT e.Id, e.Nome, e.categoria_id as CategoriaId, e.quantidade_estoque as QuantidadeEstoque, 
                       e.preco_diaria as PrecoDiaria, e.Marca, e.Modelo, e.numero_serie as NumeroSerie, 
                       e.ano_fabricacao as AnoFabricacao, e.Descricao, e.observacao_internas as ObservacaoInternas, 
                       e.Ativo, e.parceiro_id as ParceiroId, c.Nome as CategoriaNome
                FROM l6a.Equipamento e
                INNER JOIN l6a.Categoria c ON e.categoria_id = c.Id
                {whereString}
                ORDER BY e.Nome";

            return await QueryAsync<EquipamentoEntity>(sql, parameters);
        }

        public async Task<EquipamentoEntity?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT e.Id, e.Nome, e.categoria_id as CategoriaId, e.quantidade_estoque as QuantidadeEstoque, 
                       e.preco_diaria as PrecoDiaria, e.Marca, e.Modelo, e.numero_serie as NumeroSerie, 
                       e.ano_fabricacao as AnoFabricacao, e.Descricao, e.observacao_internas as ObservacaoInternas, 
                       e.Ativo, e.parceiro_id as ParceiroId, c.Nome as CategoriaNome
                FROM l6a.Equipamento e
                INNER JOIN l6a.Categoria c ON e.categoria_id = c.Id
                WHERE e.Id = @Id";

            return await QuerySingleAsync<EquipamentoEntity>(sql, new { Id = id });
        }

        public async Task<Guid> CriarAsync(EquipamentoRequest equipamento, CancellationToken cancellationToken)
        {
            var id = Guid.NewGuid();

            const string sql = @"
                INSERT INTO l6a.Equipamento (Id, Nome, categoria_id, quantidade_estoque, preco_diaria,
                                       Marca, Modelo, numero_serie, ano_fabricacao, Descricao, 
                                       observacao_internas, Ativo, parceiro_id) 
                VALUES (@Id, @Nome, @CategoriaId, @QuantidadeEstoque, @PrecoDiaria, 
                        @Marca, @Modelo, @NumeroSerie, @AnoFabricacao, @Descricao, 
                        @ObservacaoInternas, @Ativo, @ParceiroId)";

            await ExecuteAsync(sql, new
            {
                Id = id,
                equipamento.Nome,
                CategoriaId = Guid.Parse(equipamento.CategoriaId),
                equipamento.QuantidadeEstoque,
                equipamento.PrecoDiaria,
                equipamento.Marca,
                equipamento.Modelo,
                equipamento.NumeroSerie,
                equipamento.AnoFabricacao,
                Descricao = equipamento.Descricao ?? string.Empty,
                ObservacaoInternas = equipamento.ObservacaoInternas ?? string.Empty,
                Ativo = true,
                ParceiroId = Guid.Parse("00000000-0000-0000-0000-000000000001")
            });
            
            return id;
        }

        public async Task<int> AtualizarAsync(Guid id, AtualizarEquipamentoRequest equipamento, CancellationToken cancellationToken)
        {
            const string sql = @"
                UPDATE l6a.Equipamento
                SET Nome = @Nome, 
                    categoria_id = @CategoriaId,
                    quantidade_estoque = @QuantidadeEstoque,
                    preco_diaria = @PrecoDiaria,
                    Marca = @Marca,
                    Modelo = @Modelo,
                    numero_serie = @NumeroSerie,
                    ano_fabricacao = @AnoFabricacao,
                    Descricao = @Descricao,
                    observacao_internas = @ObservacaoInternas,
                    Ativo = COALESCE(@Ativo, Ativo)
                WHERE Id = @Id";

            return await ExecuteAsync(sql, new
            {
                Id = id,
                equipamento.Nome,
                CategoriaId = Guid.Parse(equipamento.CategoriaId),
                equipamento.QuantidadeEstoque,
                equipamento.PrecoDiaria,
                equipamento.Marca,
                equipamento.Modelo,
                equipamento.NumeroSerie,
                equipamento.AnoFabricacao,
                Descricao = equipamento.Descricao ?? string.Empty,
                ObservacaoInternas = equipamento.ObservacaoInternas ?? string.Empty,
                equipamento.Ativo
            });
        }

        public async Task<int> InativarAsync(Guid id, CancellationToken cancellationToken)
        {
            const string sql = @"
                UPDATE l6a.Equipamento 
                SET Ativo = false
                WHERE Id = @Id";

            return await ExecuteAsync(sql, new { Id = id });
        }

        public async Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT COUNT(1) 
                FROM l6a.Equipamento 
                WHERE Id = @Id";

            var count = await QuerySingleAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> ExisteNumeroSerieAsync(string numeroSerie, Guid? idIgnorar = null, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT COUNT(1) 
                FROM l6a.Equipamento 
                WHERE numero_serie = @NumeroSerie
                AND (@IdIgnorar IS NULL OR Id != @IdIgnorar)";

            var count = await QuerySingleAsync<int>(sql, new { NumeroSerie = numeroSerie, IdIgnorar = idIgnorar });
            return count > 0;
        }

        public async Task<bool> CategoriaExisteAsync(Guid categoriaId, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT COUNT(1) 
                FROM l6a.Categoria 
                WHERE Id = @CategoriaId AND Ativo = true";

            var count = await QuerySingleAsync<int>(sql, new { CategoriaId = categoriaId });
            return count > 0;
        }

    }
}