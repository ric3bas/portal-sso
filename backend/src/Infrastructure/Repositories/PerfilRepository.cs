using Portal.Domain.Perfil;
using Portal.Domain.Perfil.Interfaces;
using DataDapperRepository = Portal.Infrastructure.Data.DapperRepository;
using DataUnitOfWork = Portal.Infrastructure.Data.IUnitOfWork;

namespace Portal.Infrastructure.Repositories;

public class PerfilRepository : DataDapperRepository, IPerfilRepository
{
    public PerfilRepository(DataUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<IEnumerable<PerfilQuery>> ListarComEscoposAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT p.id   AS PerfilId,
                                     p.nome AS PerfilNome,
                                     e.id   AS EscopoId,
                                     e.nome AS EscopoNome
                              FROM sso.perfil p
                              LEFT JOIN sso.perfil_escopo pe ON pe.perfil_id = p.id
                              LEFT JOIN sso.escopo e ON e.id = pe.escopo_id
                              ORDER BY p.id, e.id";

        var rows = await QueryAsync<PerfilEscopoRow>(sql);
        return rows
            .GroupBy(r => new { r.PerfilId, r.PerfilNome })
            .Select(group => new PerfilQuery
            {
                Id = group.Key.PerfilId,
                Nome = group.Key.PerfilNome,
                Escopos = group.Where(x => x.EscopoId.HasValue)
                    .Select(x => new PerfilEscopoQuery { Id = x.EscopoId!.Value, Nome = x.EscopoNome ?? string.Empty })
                    .DistinctBy(x => x.Id)
                    .ToList()
            })
            .ToList();
    }

    public async Task<IEnumerable<PerfilQuery>> ObterPerfilParaComboAsync(bool isMaster, CancellationToken cancellationToken = default)
    {
        var sql = "SELECT p.id, p.nome FROM sso.perfil p";
        if (!isMaster)
            sql += " WHERE p.is_master = False";

        var rows = await QueryAsync<PerfilComboRow>(sql);
        return rows.Select(x => new PerfilQuery { Id = x.Id, Nome = x.Nome ?? string.Empty }).ToList();
    }

    public async Task<PerfilQuery?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT p.id   AS PerfilId,
                                     p.nome AS PerfilNome,
                                     e.id   AS EscopoId,
                                     e.nome AS EscopoNome
                              FROM sso.perfil p
                              LEFT JOIN sso.perfil_escopo pe ON pe.perfil_id = p.id
                              LEFT JOIN sso.escopo e ON e.id = pe.escopo_id
                              WHERE p.id = @id
                              ORDER BY p.id, e.id";

        var rows = await QueryAsync<PerfilEscopoRow>(sql, new { id });
        var perfil = rows.FirstOrDefault();
        if (perfil is null)
            return null;

        return new PerfilQuery
        {
            Id = perfil.PerfilId,
            Nome = perfil.PerfilNome,
            Escopos = rows.Where(r => r.EscopoId.HasValue)
                .Select(r => new PerfilEscopoQuery { Id = r.EscopoId!.Value, Nome = r.EscopoNome ?? string.Empty })
                .ToList()
        };
    }

    public async Task<int> InserirAsync(PerfilCommand perfil, CancellationToken cancellationToken = default)
    {
        const string sql = "INSERT INTO sso.perfil (nome) VALUES (@Nome) RETURNING id";
        var result = await QueryAsync<int>(sql, new { perfil.Nome });
        return result.FirstOrDefault();
    }

    public async Task<bool> ExistePerfilAsync(int perfilId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(1) FROM sso.perfil WHERE id = @perfilId";
        var total = await QuerySingleAsync<int>(sql, new { perfilId });
        return total > 0;
    }

    public async Task<bool> ExisteNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(1) FROM sso.perfil WHERE LOWER(nome) = LOWER(@nome)";
        var total = await QuerySingleAsync<int>(sql, new { nome });
        return total > 0;
    }

    public async Task VincularEscoposAsync(int perfilId, IEnumerable<int> escopoIds, CancellationToken cancellationToken = default)
    {
        const string deleteSql = "DELETE FROM sso.perfil_escopo WHERE perfil_id = @perfilId";
        await ExecuteAsync(deleteSql, new { perfilId });

        const string insertSql = "INSERT INTO sso.perfil_escopo (perfil_id, escopo_id) VALUES (@perfilId, @escopoId) ON CONFLICT DO NOTHING";
        var parametros = escopoIds.Select(escopoId => new { perfilId, escopoId });
        await ExecuteAsync(insertSql, parametros);
    }

    public async Task DeletarAsync(int id, CancellationToken cancellationToken = default)
    {
        const string deleteVinculos = "DELETE FROM sso.perfil_escopo WHERE perfil_id = @id";
        await ExecuteAsync(deleteVinculos, new { id });

        const string deletePerfil = "DELETE FROM sso.perfil WHERE id = @id";
        await ExecuteAsync(deletePerfil, new { id });
    }

    public async Task AtualizarNomeAsync(int id, string novoNome, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE sso.perfil SET nome = @novoNome WHERE id = @id";
        await ExecuteAsync(sql, new { id, novoNome });
    }

    private sealed class PerfilEscopoRow
    {
        public int PerfilId { get; set; }
        public string PerfilNome { get; set; } = string.Empty;
        public int? EscopoId { get; set; }
        public string? EscopoNome { get; set; }
    }

    private sealed class PerfilComboRow
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
    }
}
