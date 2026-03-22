using Portal.Features.Escopo.Domain.Interfaces;
using Portal.Infra;

namespace Portal.Features.Escopo.Infra
{
    [DbContext("SSO_POSTGRES")]
    public class EscopoRepository : DapperRepository, IDapperRepository, IEscopoRepository
    {
        public EscopoRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<IEnumerable<EscopoQuery>> ListarAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "SELECT id, nome FROM sso.escopo ORDER BY id";
            return await QueryAsync<EscopoQuery>(sql);
        }

        public async Task<int> InserirAsync(EscopoCommand escopo, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "INSERT INTO sso.escopo (nome) VALUES (@Nome) RETURNING id";
            var result = await QueryAsync<int>(sql, new { escopo.Nome });
            return result.FirstOrDefault();
        }

        public async Task<EscopoQuery?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "SELECT id, nome FROM sso.escopo WHERE id = @id";
            return await QuerySingleAsync<EscopoQuery>(sql, new { id });
        }

        public async Task<IEnumerable<int>> ObterIdsExistentesAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "SELECT id FROM sso.escopo WHERE id = ANY(@ids)";
            return await QueryAsync<int>(sql, new { ids = ids.ToArray() });
        }

        public async Task<bool> ExisteNomeAsync(string nome, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "SELECT COUNT(1) FROM sso.escopo WHERE LOWER(nome) = LOWER(@nome)";
            var total = await QuerySingleAsync<int>(sql, new { nome });
            return total > 0;
        }
    }
}
