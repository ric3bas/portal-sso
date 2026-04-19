using Portal.Domain.Categoria;
using Portal.Domain.Categoria.Interfaces;
using DataDapperRepository = Portal.Infrastructure.Data.DapperRepository;
using DataUnitOfWork = Portal.Infrastructure.Data.IUnitOfWork;

namespace Portal.Infrastructure.Repositories;

public class CategoriaRepository : DataDapperRepository, ICategoriaRepository
{
    public CategoriaRepository(DataUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<IEnumerable<CategoriaQuery>> ObterTodasAsync(CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT Id, Nome, Ativo, parceiro_id as ParceiroId 
            FROM l6a.Categoria 
            ORDER BY Nome";

        return await QueryAsync<CategoriaQuery>(sql);
    }

    public async Task<IEnumerable<CategoriaQuery>> ObterPorParceiroAsync(Guid parceiroId, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT Id, Nome, Ativo, parceiro_id as ParceiroId 
            FROM l6a.Categoria 
            WHERE parceiro_id = @ParceiroId 
            ORDER BY Nome";

        return await QueryAsync<CategoriaQuery>(sql, new { ParceiroId = parceiroId });
    }

    public async Task<IEnumerable<CategoriaQuery>> ObterPorFiltroAsync(string nome, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT Id, Nome, Ativo, parceiro_id as ParceiroId 
            FROM l6a.Categoria 
            WHERE Nome LIKE @Nome 
            ORDER BY Nome";

        return await QueryAsync<CategoriaQuery>(sql, new { Nome = $"%{nome}%" });
    }

    public async Task<IEnumerable<CategoriaQuery>> ObterPorFiltroEParceiroAsync(Guid parceiroId, string nome, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT Id, Nome, Ativo, parceiro_id as ParceiroId 
            FROM l6a.Categoria 
            WHERE parceiro_id = @ParceiroId 
            AND Nome LIKE @Nome 
            ORDER BY Nome";

        return await QueryAsync<CategoriaQuery>(sql, new { ParceiroId = parceiroId, Nome = $"%{nome}%" });
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
