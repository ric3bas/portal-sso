using Dapper;
using Portal.Features.Escopo.Domain.Interfaces;
using Portal.Infra;
using Portal.Features.Escopo.Domain;
using EscopoEntity = Portal.Dominio.Entities.Escopo;

namespace Portal.Features.Escopo.Infra
{
    public class EscopoRepository : DapperRepository<EscopoEntity>, IDapperRepository<EscopoEntity>, IEscopoRepository
    {
        public EscopoRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<IEnumerable<EscopoResponse>> ListarAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "SELECT id, nome FROM sso.escopo ORDER BY id";
            return await Task.Run(() => _unitOfWork.Connection.Query<EscopoResponse>(sql, transaction: _unitOfWork.Transaction), cancellationToken);
        }

        public async Task<int> InserirAsync(EscopoEntity escopo, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "INSERT INTO sso.escopo (nome) VALUES (@Nome) RETURNING id";
            return await Task.Run(() => _unitOfWork.Connection.QuerySingle<int>(sql, new { escopo.Nome }, _unitOfWork.Transaction), cancellationToken);
        }

        public async Task<EscopoEntity?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "SELECT id, nome FROM sso.escopo WHERE id = @id";
            return await Task.Run(() => QuerySingle(sql, new { id }), cancellationToken);
        }

        public async Task<IEnumerable<int>> ObterIdsExistentesAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "SELECT id FROM sso.escopo WHERE id = ANY(@ids)";
            return await Task.Run(() => _unitOfWork.Connection.Query<int>(sql, new { ids = ids.ToArray() }, _unitOfWork.Transaction), cancellationToken);
        }

        public async Task<bool> ExisteNomeAsync(string nome, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "SELECT COUNT(1) FROM sso.escopo WHERE LOWER(nome) = LOWER(@nome)";
            var total = await Task.Run(() => _unitOfWork.Connection.ExecuteScalar<int>(sql, new { nome }, _unitOfWork.Transaction), cancellationToken);
            return total > 0;
        }
    }
}
