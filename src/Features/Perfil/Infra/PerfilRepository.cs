using Dapper;
using Portal.Features.Perfil.Domain.Interfaces;
using Portal.Infra;
using Portal.Features.Perfil.Domain;
using PerfilEntity = Portal.Dominio.Entities.Perfil;

namespace Portal.Features.Perfil.Infra
{
    public class PerfilRepository : DapperRepository<PerfilEntity>, IDapperRepository<PerfilEntity>, IPerfilRepository
    {

        public PerfilRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<IEnumerable<PerfilComEscopoResponse>> ListarComEscoposAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = @"SELECT p.id   AS PerfilId,
                                        p.nome AS PerfilNome,
                                        e.id   AS EscopoId,
                                        e.nome AS EscopoNome
                                 FROM sso.perfil p
                                 LEFT JOIN sso.perfil_escopo pe ON pe.perfil_id = p.id
                                 LEFT JOIN sso.escopo e ON e.id = pe.escopo_id
                                 ORDER BY p.id, e.id";

            var rows = await _unitOfWork.Connection.QueryAsync<PerfilEscopoRowResponse>(sql, transaction: _unitOfWork.Transaction);

            return rows
                .GroupBy(r => new { r.PerfilId, r.PerfilNome })
                .Select(group => new PerfilComEscopoResponse
                {
                    Id   = group.Key.PerfilId,
                    Nome = group.Key.PerfilNome,
                    Escopos = group
                        .Where(x => x.EscopoId.HasValue)
                        .Select(x => new PerfilEscopoItemResponse
                        {
                            Id   = x.EscopoId!.Value,
                            Nome = x.EscopoNome ?? string.Empty
                        })
                        .DistinctBy(x => x.Id)
                        .ToList()
                })
                .ToList();
        }

        public async Task<int> InserirAsync(PerfilEntity perfil, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "INSERT INTO sso.perfil (nome) VALUES (@Nome) RETURNING id";
            return await _unitOfWork.Connection.QuerySingleAsync<int>(sql, new { perfil.Nome }, _unitOfWork.Transaction);
        }

        public async Task<PerfilEntity?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "SELECT id, nome FROM sso.perfil WHERE id = @id";
            return await _unitOfWork.Connection.QuerySingleOrDefaultAsync<PerfilEntity>(sql, new { id }, _unitOfWork.Transaction);
        }

        public async Task<bool> ExistePerfilAsync(int perfilId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "SELECT COUNT(1) FROM sso.perfil WHERE id = @perfilId";
            var total = await _unitOfWork.Connection.ExecuteScalarAsync<int>(sql, new { perfilId }, _unitOfWork.Transaction);
            return total > 0;
        }

        public async Task<bool> ExisteNomeAsync(string nome, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "SELECT COUNT(1) FROM sso.perfil WHERE LOWER(nome) = LOWER(@nome)";
            var total = await _unitOfWork.Connection.ExecuteScalarAsync<int>(sql, new { nome }, _unitOfWork.Transaction);
            return total > 0;
        }

        public async Task VincularEscoposAsync(int perfilId, IEnumerable<int> escopoIds, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "INSERT INTO sso.perfil_escopo (perfil_id, escopo_id) VALUES (@perfilId, @escopoId) ON CONFLICT DO NOTHING";
            var parametros = escopoIds.Select(escopoId => new { perfilId, escopoId });
            await _unitOfWork.Connection.ExecuteAsync(sql, parametros, _unitOfWork.Transaction);
        }

    }
}
