using Portal.Features.Perfil.Domain.Interfaces;
using Portal.Infra;
using Portal.Features.Perfil.Domain;

namespace Portal.Features.Perfil.Infra
{
    [DbContext("SSO_POSTGRES")]
    public class PerfilRepository : DapperRepository, IDapperRepository, IPerfilRepository
    {
        public PerfilRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<IEnumerable<PerfilComEscopoQuery>> ListarComEscoposAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();   
            const string sql = @"SELECT p.id   AS PerfilId,
                                        p.nome AS PerfilNome,
                                        e.id   AS EscopoId,
                                        e.nome AS EscopoNome,
                                        p.is_master AS Master,
                                        e.is_master AS EscopoMaster
                                 FROM sso.perfil p
                                 LEFT JOIN sso.perfil_escopo pe ON pe.perfil_id = p.id
                                 LEFT JOIN sso.escopo e ON e.id = pe.escopo_id AND p.is_master = true 
                                 ORDER BY p.id, e.id";

            var rows = await QueryAsync<PerfilEscopoRowResponse>(sql);
            return MapToPerfisComEscopo(rows);
        }

        public async Task<IEnumerable<PerfilQuery>> ObterPerfilParaComboAsync(bool isMaster, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sql = "SELECT p.id ,p.nome  FROM sso.perfil p";

            if (!isMaster) sql += " WHERE p.is_master = False";

            return await QueryAsync<PerfilQuery>(sql);
           
        }

        private static List<PerfilComEscopoQuery> MapToPerfisComEscopo(IEnumerable<PerfilEscopoRowResponse> rows)
        {
            return rows
                .GroupBy(r => new { r.PerfilId, r.PerfilNome })
                .Select(group => new PerfilComEscopoQuery
                {
                    Id = group.Key.PerfilId,
                    Nome = group.Key.PerfilNome,
                    Escopos = MapEscopos(group)
                })
                .ToList();
        }

        private static List<PerfilEscopoItemQuery> MapEscopos(IEnumerable<PerfilEscopoRowResponse> rows)
        {
            return rows
                .Where(x => x.EscopoId.HasValue && x.EscopoMaster == true)
                .Select(x => new PerfilEscopoItemQuery
                {
                    Id = x.EscopoId!.Value,
                    Nome = x.EscopoNome ?? string.Empty
                })
                .DistinctBy(x => x.Id)
                .ToList();
        }

        public async Task<PerfilComEscopoQuery?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = @"
                                SELECT p.id   AS PerfilId,
                                       p.nome AS PerfilNome,
                                       e.id   AS EscopoId,
                                       e.nome AS EscopoNome
                                FROM sso.perfil p
                                LEFT JOIN sso.perfil_escopo pe ON pe.perfil_id = p.id
                                LEFT JOIN sso.escopo e ON e.id = pe.escopo_id
                                WHERE p.id = @id
                                ORDER BY p.id, e.id";

            var rows = await QueryAsync<PerfilEscopoRowResponse>(sql, new { id });

            var perfil = rows.FirstOrDefault();
            if (perfil is null)
                return null;

            return new PerfilComEscopoQuery
            {
                Id = perfil.PerfilId,
                Nome = perfil.PerfilNome,
                Escopos = rows
                    .Where(r => r.EscopoId.HasValue)
                    .Select(r => new PerfilEscopoItemQuery
                    {
                        Id = r.EscopoId!.Value,
                        Nome = r.EscopoNome ?? string.Empty
                    })
                    .ToList()
            };
        }

        public async Task<List<int>?> ObterEscoposPorPerfilAsync(int perfilId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = @"SELECT pe.escopo_id
                         FROM sso.perfil p
                         INNER JOIN sso.perfil_escopo pe ON pe.perfil_id = p.id
                         WHERE p.id = @perfilId";

            var result = await QueryAsync<int>(sql, new { perfilId });
            return result.Any() ? result.ToList() : null;
        }

        public async Task<int> InserirAsync(PerfilCommand perfil, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "INSERT INTO sso.perfil (nome) VALUES (@Nome) RETURNING id";
            var result = await QueryAsync<int>(sql, new { perfil.Nome });
            return result.FirstOrDefault();
        }

        public async Task<bool> ExistePerfilAsync(int perfilId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "SELECT COUNT(1) FROM sso.perfil WHERE id = @perfilId";
            var total = await QuerySingleAsync<int>(sql, new { perfilId });
            return total > 0;
        }

        public async Task<bool> ExisteNomeAsync(string nome, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "SELECT COUNT(1) FROM sso.perfil WHERE LOWER(nome) = LOWER(@nome)";
            var total = await QuerySingleAsync<int>(sql, new { nome });
            return total > 0;
        }

        public async Task VincularEscoposAsync(int perfilId, IEnumerable<int> escopoIds, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string deleteSql = "DELETE FROM sso.perfil_escopo WHERE perfil_id = @perfilId";
            await ExecuteAsync(deleteSql, new { perfilId });

            const string insertSql = "INSERT INTO sso.perfil_escopo (perfil_id, escopo_id) VALUES (@perfilId, @escopoId) ON CONFLICT DO NOTHING";
            var parametros = escopoIds.Select(escopoId => new { perfilId, escopoId });
            await ExecuteAsync(insertSql, parametros);
        }

        public async Task DeletarAsync(int id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Remove vínculos primeiro
            const string deleteVinculos = "DELETE FROM sso.perfil_escopo WHERE perfil_id = @id";
            await ExecuteAsync(deleteVinculos, new { id });
            // Remove o perfil
            const string deletePerfil = "DELETE FROM sso.perfil WHERE id = @id";
            await ExecuteAsync(deletePerfil, new { id });
        }

        public async Task AtualizarNomeAsync(int id, string novoNome, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "UPDATE sso.perfil SET nome = @novoNome WHERE id = @id";
            await ExecuteAsync(sql, new { id, novoNome });
        }
    } // fim da classe PerfilRepository
}
