using Dapper;
using Portal.Domain.Common;
using Portal.Domain.Equipamento;
using Portal.Domain.Equipamento.Interfaces;
using DataDapperRepository = Portal.Infrastructure.Data.DapperRepository;
using DataUnitOfWork = Portal.Infrastructure.Data.IUnitOfWork;

namespace Portal.Infrastructure.Repositories;

public class EquipamentoRepository : DataDapperRepository, IEquipamentoRepository
{
    public EquipamentoRepository(DataUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<ResultadoPaginado<EquipamentoQuery>> ObterTodosAsync(Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken)
    {
        var paginacao = PaginacaoHelper.Criar(direcao, pagina, tamanhoPagina);

        var sql = $@"
            SELECT COUNT(1)
            FROM l6a.Equipamento e
            INNER JOIN l6a.Categoria c ON e.categoria_id = c.Id;

            SELECT e.Id, e.Nome, e.categoria_id as CategoriaId, e.quantidade_estoque as QuantidadeEstoque,
                   e.preco_diaria as PrecoDiaria, e.Marca, e.Modelo, e.numero_serie as NumeroSerie,
                   e.ano_fabricacao as AnoFabricacao, e.Descricao, e.observacao_internas as ObservacaoInternas,
                   e.Ativo, e.parceiro_id as ParceiroId, c.Nome as CategoriaNome
            FROM l6a.Equipamento e
            INNER JOIN l6a.Categoria c ON e.categoria_id = c.Id
            ORDER BY e.Nome {paginacao.OrdemSql}
            LIMIT @TamanhoPagina OFFSET @Offset";

        using var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, new { TamanhoPagina = paginacao.TamanhoPagina, Offset = paginacao.Offset });
        var total = await multi.ReadSingleAsync<int>();
        var itens = (await multi.ReadAsync<EquipamentoQuery>()).ToList();
        return new ResultadoPaginado<EquipamentoQuery>(itens, total, paginacao.Pagina, paginacao.TamanhoPagina);
    }

    public async Task<ResultadoPaginado<EquipamentoQuery>> ObterPorFiltroAsync(string? nome, string? marca, string? modelo, Guid? categoriaId, bool? ativo, Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken)
    {
        var whereClause = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(nome))
        {
            whereClause.Add("e.Nome ILIKE @Nome");
            parameters.Add("Nome", $"%{nome}%");
        }

        if (!string.IsNullOrWhiteSpace(marca))
        {
            whereClause.Add("e.Marca ILIKE @Marca");
            parameters.Add("Marca", $"%{marca}%");
        }

        if (!string.IsNullOrWhiteSpace(modelo))
        {
            whereClause.Add("e.Modelo ILIKE @Modelo");
            parameters.Add("Modelo", $"%{modelo}%");
        }

        if (categoriaId.HasValue)
        {
            whereClause.Add("e.categoria_id = @CategoriaId");
            parameters.Add("CategoriaId", categoriaId.Value);
        }

        if (ativo.HasValue)
        {
            whereClause.Add("e.Ativo = @Ativo");
            parameters.Add("Ativo", ativo.Value);
        }

        var whereString = whereClause.Count > 0 ? $"WHERE {string.Join(" AND ", whereClause)}" : string.Empty;

        var paginacao = PaginacaoHelper.Criar(direcao, pagina, tamanhoPagina);

        var sql = $@"
            SELECT COUNT(1)
            FROM l6a.Equipamento e
            INNER JOIN l6a.Categoria c ON e.categoria_id = c.Id
            {whereString};

            SELECT e.Id, e.Nome, e.categoria_id as CategoriaId, e.quantidade_estoque as QuantidadeEstoque,
                   e.preco_diaria as PrecoDiaria, e.Marca, e.Modelo, e.numero_serie as NumeroSerie,
                   e.ano_fabricacao as AnoFabricacao, e.Descricao, e.observacao_internas as ObservacaoInternas,
                   e.Ativo, e.parceiro_id as ParceiroId, c.Nome as CategoriaNome
            FROM l6a.Equipamento e
            INNER JOIN l6a.Categoria c ON e.categoria_id = c.Id
            {whereString}
            ORDER BY e.Nome {paginacao.OrdemSql}
            LIMIT @TamanhoPagina OFFSET @Offset";

        parameters.Add("TamanhoPagina", paginacao.TamanhoPagina);
        parameters.Add("Offset", paginacao.Offset);

        using var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, parameters);
        var total = await multi.ReadSingleAsync<int>();
        var itens = (await multi.ReadAsync<EquipamentoQuery>()).ToList();
        return new ResultadoPaginado<EquipamentoQuery>(itens, total, paginacao.Pagina, paginacao.TamanhoPagina);
    }

    public async Task<EquipamentoQuery?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT e.Id, e.Nome, e.categoria_id as CategoriaId, e.quantidade_estoque as QuantidadeEstoque,
                   e.preco_diaria as PrecoDiaria, e.Marca, e.Modelo, e.numero_serie as NumeroSerie,
                   e.ano_fabricacao as AnoFabricacao, e.Descricao, e.observacao_internas as ObservacaoInternas,
                   e.Ativo, e.parceiro_id as ParceiroId, c.Nome as CategoriaNome
            FROM l6a.Equipamento e
            INNER JOIN l6a.Categoria c ON e.categoria_id = c.Id
            WHERE e.Id = @Id";

        return await QuerySingleAsync<EquipamentoQuery>(sql, new { Id = id });
    }

    public async Task<Guid> CriarAsync(EquipamentoCommand equipamento, CancellationToken cancellationToken)
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
            equipamento.CategoriaId,
            equipamento.QuantidadeEstoque,
            equipamento.PrecoDiaria,
            equipamento.Marca,
            equipamento.Modelo,
            equipamento.NumeroSerie,
            equipamento.AnoFabricacao,
            Descricao = equipamento.Descricao ?? string.Empty,
            ObservacaoInternas = equipamento.ObservacaoInternas ?? string.Empty,
            Ativo = equipamento.Ativo,
            ParceiroId = Guid.Parse("00000000-0000-0000-0000-000000000001")
        });

        return id;
    }

    public async Task<int> AtualizarAsync(EquipamentoCommand equipamento, CancellationToken cancellationToken)
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
                Ativo = @Ativo
            WHERE Id = @Id";

        return await ExecuteAsync(sql, new
        {
            equipamento.Id,
            equipamento.Nome,
            equipamento.CategoriaId,
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
