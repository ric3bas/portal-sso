using Portal.Features.Categoria.Domain;
using Portal.Features.Categoria.Domain.Interfaces;
using Portal.Infra;

namespace Portal.Features.Categoria.Infra
{
    public class CategoriaRepository : DapperRepository, ICategoriaRepository
    {
        public CategoriaRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<IEnumerable<CategoriaEntity>> ObterTodasAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT Id, Nome, Ativo, parceiro_id as ParceiroId 
                FROM l6a.Categoria 
                ORDER BY Nome";

            return await QueryAsync<CategoriaEntity>(sql);
        }

        public async Task<IEnumerable<CategoriaEntity>> ObterPorParceiroAsync(Guid parceiroId, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT Id, Nome, Ativo, parceiro_id as ParceiroId 
                FROM l6a.Categoria 
                WHERE parceiro_id = @ParceiroId 
                ORDER BY Nome";

            return await QueryAsync<CategoriaEntity>(sql, new { ParceiroId = parceiroId });
        }

        public async Task<IEnumerable<CategoriaEntity>> ObterPorFiltroAsync(string nome, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT Id, Nome, Ativo, parceiro_id as ParceiroId 
                FROM l6a.Categoria 
                WHERE Nome LIKE @Nome 
                ORDER BY Nome";

            return await QueryAsync<CategoriaEntity>(sql, new { Nome = $"%{nome}%" });
        }

        public async Task<IEnumerable<CategoriaEntity>> ObterPorFiltroEParceiroAsync(Guid parceiroId, string nome, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT Id, Nome, Ativo, parceiro_id as ParceiroId 
                FROM l6a.Categoria 
                WHERE parceiro_id = @ParceiroId 
                AND Nome LIKE @Nome 
                ORDER BY Nome";

            return await QueryAsync<CategoriaEntity>(sql, new { ParceiroId = parceiroId, Nome = $"%{nome}%" });
        }

        public async Task<CategoriaEntity?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT Id, Nome, Ativo, parceiro_id as ParceiroId 
                FROM l6a.Categoria 
                WHERE Id = @Id";

            return await QuerySingleAsync<CategoriaEntity>(sql, new { Id = id });
        }

        public async Task<CategoriaEntity?> ObterPorIdEParceiroAsync(Guid id, Guid parceiroId, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT Id, Nome, Ativo, parceiro_id as ParceiroId 
                FROM l6a.Categoria 
                WHERE Id = @Id AND parceiro_id = @ParceiroId";

            return await QuerySingleAsync<CategoriaEntity>(sql, new { Id = id, ParceiroId = parceiroId });
        }

        public async Task<Guid> CriarAsync(string nome, Guid parceiroId, CancellationToken cancellationToken)
        {
            var id = Guid.NewGuid();

            const string sql = @"
                INSERT INTO l6a.Categoria (Id, Nome, Ativo, parceiro_id)
                VALUES (@Id, @Nome, @Ativo, @ParceiroId)";

            await ExecuteAsync(sql, new { Id = id, Nome = nome, Ativo = true, ParceiroId = parceiroId });
            return id;
        }

        public async Task<int> AtualizarAsync(Guid id, string nome, bool? ativo, Guid parceiroId, CancellationToken cancellationToken)
        {
            const string sql = @"
                UPDATE l6a.Categoria 
                SET Nome = @Nome, 
                    Ativo = COALESCE(@Ativo, Ativo)
                WHERE Id = @Id AND parceiro_id = @ParceiroId";

            return await ExecuteAsync(sql, new { Id = id, Nome = nome, Ativo = ativo, ParceiroId = parceiroId });
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
}