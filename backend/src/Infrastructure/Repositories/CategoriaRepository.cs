using Dapper;
using Portal.Domain.Categoria;
using Portal.Domain.Categoria.Interfaces;
using Portal.Domain.Common;
using DataDapperRepository = Portal.Infrastructure.Data.DapperRepository;
using DataUnitOfWork = Portal.Infrastructure.Data.IUnitOfWork;

namespace Portal.Infrastructure.Repositories;

public class CategoriaRepository : DataDapperRepository, ICategoriaRepository
{
    public CategoriaRepository(DataUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<ResultadoPaginado<CategoriaQuery>> ObterTodasAsync(Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken)
    {
        var paginacao = PaginacaoHelper.Criar(direcao, pagina, tamanhoPagina);

        var sql = $@"
            SELECT COUNT(1) FROM l6a.Categoria;
            SELECT Id, Nome, Ativo, parceiro_id as ParceiroId 
            FROM l6a.Categoria 
            ORDER BY Nome {paginacao.OrdemSql}
            LIMIT @TamanhoPagina OFFSET @Offset";

        using var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, new { TamanhoPagina = paginacao.TamanhoPagina, Offset = paginacao.Offset });
        var total = await multi.ReadSingleAsync<int>();
        var itens = (await multi.ReadAsync<CategoriaQuery>()).ToList();
        return new ResultadoPaginado<CategoriaQuery>(itens, total, paginacao.Pagina, paginacao.TamanhoPagina);
    }

    public async Task<ResultadoPaginado<CategoriaQuery>> ObterPorParceiroAsync(Guid parceiroId, Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken)
    {
        var paginacao = PaginacaoHelper.Criar(direcao, pagina, tamanhoPagina);
        var filtrarParceiro = parceiroId != Guid.Empty;
        var whereParceiro = filtrarParceiro ? "WHERE parceiro_id = @ParceiroId" : string.Empty;

        var sql = $@"
            SELECT COUNT(1) 
            FROM l6a.Categoria 
            {whereParceiro};

            SELECT Id, Nome, Ativo, parceiro_id as ParceiroId 
            FROM l6a.Categoria 
            {whereParceiro}
            ORDER BY Nome {paginacao.OrdemSql}
            LIMIT @TamanhoPagina OFFSET @Offset";

        using var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, new { ParceiroId = parceiroId, TamanhoPagina = paginacao.TamanhoPagina, Offset = paginacao.Offset });
        var total = await multi.ReadSingleAsync<int>();
        var itens = (await multi.ReadAsync<CategoriaQuery>()).ToList();
        return new ResultadoPaginado<CategoriaQuery>(itens, total, paginacao.Pagina, paginacao.TamanhoPagina);
    }

    public async Task<ResultadoPaginado<CategoriaQuery>> ObterPorFiltroAsync(string nome, Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken)
    {
        var paginacao = PaginacaoHelper.Criar(direcao, pagina, tamanhoPagina);

        var sql = $@"
            SELECT COUNT(1) 
            FROM l6a.Categoria 
            WHERE Nome LIKE @Nome;

            SELECT Id, Nome, Ativo, parceiro_id as ParceiroId 
            FROM l6a.Categoria 
            WHERE Nome LIKE @Nome 
            ORDER BY Nome {paginacao.OrdemSql}
            LIMIT @TamanhoPagina OFFSET @Offset";

        var param = new { Nome = $"%{nome}%", TamanhoPagina = paginacao.TamanhoPagina, Offset = paginacao.Offset };
        using var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, param);
        var total = await multi.ReadSingleAsync<int>();
        var itens = (await multi.ReadAsync<CategoriaQuery>()).ToList();
        return new ResultadoPaginado<CategoriaQuery>(itens, total, paginacao.Pagina, paginacao.TamanhoPagina);
    }

    public async Task<ResultadoPaginado<CategoriaQuery>> ObterPorFiltroEParceiroAsync(Guid parceiroId, string nome, Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken)
    {
        var paginacao = PaginacaoHelper.Criar(direcao, pagina, tamanhoPagina);

        var sql = $@"
            SELECT COUNT(1)
            FROM l6a.Categoria 
            WHERE parceiro_id = @ParceiroId 
            AND Nome LIKE @Nome;

            SELECT Id, Nome, Ativo, parceiro_id as ParceiroId 
            FROM l6a.Categoria 
            WHERE parceiro_id = @ParceiroId 
            AND Nome LIKE @Nome 
            ORDER BY Nome {paginacao.OrdemSql}
            LIMIT @TamanhoPagina OFFSET @Offset";

        var param = new { ParceiroId = parceiroId, Nome = $"%{nome}%", TamanhoPagina = paginacao.TamanhoPagina, Offset = paginacao.Offset };
        using var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, param);
        var total = await multi.ReadSingleAsync<int>();
        var itens = (await multi.ReadAsync<CategoriaQuery>()).ToList();
        return new ResultadoPaginado<CategoriaQuery>(itens, total, paginacao.Pagina, paginacao.TamanhoPagina);
    }

    public async Task<CategoriaQuery?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT Id, Nome, Ativo, parceiro_id as ParceiroId 
            FROM l6a.Categoria 
            WHERE Id = @Id";

        return await QuerySingleAsync<CategoriaQuery>(sql, new { Id = id });
    }

    public async Task<CategoriaQuery?> ObterPorIdEParceiroAsync(Guid id, Guid parceiroId, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT Id, Nome, Ativo, parceiro_id as ParceiroId 
            FROM l6a.Categoria 
            WHERE Id = @Id AND parceiro_id = @ParceiroId";

        return await QuerySingleAsync<CategoriaQuery>(sql, new { Id = id, ParceiroId = parceiroId });
    }

    public async Task<Guid> CriarAsync(CategoriaCommand categoria, CancellationToken cancellationToken)
    {
        var id = categoria.Id == Guid.Empty ? Guid.NewGuid() : categoria.Id;

        const string sql = @"
            INSERT INTO l6a.Categoria (Id, Nome, Ativo, parceiro_id) 
            VALUES (@Id, @Nome, @Ativo, @ParceiroId)";

        await ExecuteAsync(sql, new { Id = id, categoria.Nome, Ativo = categoria.Ativo, categoria.ParceiroId });
        return id;
    }

    public async Task<int> AtualizarAsync(CategoriaCommand categoria, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE l6a.Categoria 
            SET Nome = @Nome, 
                Ativo = @Ativo
            WHERE Id = @Id AND parceiro_id = @ParceiroId";

        return await ExecuteAsync(sql, new { categoria.Id, categoria.Nome, categoria.Ativo, categoria.ParceiroId });
    }

    public async Task<int> InativarAsync(Guid id, Guid parceiroId, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE l6a.Categoria 
            SET Ativo = false 
            WHERE Id = @Id AND parceiro_id = @ParceiroId";

        return await ExecuteAsync(sql, new { Id = id, ParceiroId = parceiroId });
    }

    public async Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT COUNT(1) 
            FROM l6a.Categoria 
            WHERE Id = @Id";

        var count = await QuerySingleAsync<int>(sql, new { Id = id });
        return count > 0;
    }

    public async Task<bool> ExisteNomeAsync(string nome, Guid parceiroId, Guid? idIgnorar = null, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(1) 
            FROM l6a.Categoria 
            WHERE Nome = @Nome 
            AND parceiro_id = @ParceiroId
            AND (@IdIgnorar IS NULL OR Id != @IdIgnorar)";

        var count = await QuerySingleAsync<int>(sql, new { Nome = nome, ParceiroId = parceiroId, IdIgnorar = idIgnorar });
        return count > 0;
    }
}
