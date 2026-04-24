using Dapper;
using Portal.Domain.Escopo;
using Portal.Domain.Escopo.Interfaces;
using Portal.Domain.Common;
using DataDapperRepository = Portal.Infrastructure.Data.DapperRepository;
using DataUnitOfWork = Portal.Infrastructure.Data.IUnitOfWork;

namespace Portal.Infrastructure.Repositories;

public class EscopoRepository : DataDapperRepository, IEscopoRepository
{
    public EscopoRepository(DataUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<ResultadoPaginado<EscopoQuery>> ObterTodosAsync(string? nome, Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken = default)
    {
        var paginacao = PaginacaoHelper.Criar(direcao, pagina, tamanhoPagina);
        var temFiltroNome = !string.IsNullOrWhiteSpace(nome);

        var sql = $@"
            SELECT COUNT(1)
            FROM sso.escopo
            {(temFiltroNome ? "WHERE LOWER(nome) LIKE LOWER(@Nome)" : string.Empty)};

            SELECT id, nome
            FROM sso.escopo
            {(temFiltroNome ? "WHERE LOWER(nome) LIKE LOWER(@Nome)" : string.Empty)}
            ORDER BY nome {paginacao.OrdemSql}
            LIMIT @TamanhoPagina OFFSET @Offset;";

        using var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, new
        {
            Nome = $"%{nome}%",
            TamanhoPagina = paginacao.TamanhoPagina,
            Offset = paginacao.Offset
        });

        var totalRegistros = await multi.ReadSingleAsync<int>();
        var itens = (await multi.ReadAsync<EscopoQuery>()).ToList();

        return new ResultadoPaginado<EscopoQuery>(itens, totalRegistros, paginacao.Pagina, paginacao.TamanhoPagina);
    }

    public async Task<EscopoQuery?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT id, nome FROM sso.escopo WHERE id = @id";
        return await QuerySingleAsync<EscopoQuery>(sql, new { id });
    }

    public async Task<IEnumerable<int>> ObterIdsExistentesAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT id FROM sso.escopo WHERE id = ANY(@ids)";
        return await QueryAsync<int>(sql, new { ids = ids.ToArray() });
    }

    public async Task<int> CriarAsync(EscopoCommand escopo, CancellationToken cancellationToken = default)
    {
        const string sql = "INSERT INTO sso.escopo (nome) VALUES (@Nome) RETURNING id";
        var result = await QuerySingleAsync<int>(sql, new { escopo.Nome });
        return result;
    }

    public async Task AtualizarAsync(EscopoCommand escopo, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE sso.escopo SET nome = @Nome WHERE id = @Id";
        await ExecuteAsync(sql, new { escopo.Id, escopo.Nome });
    }

    public async Task<bool> ExisteNomeAsync(string nome, int? idIgnorar = null, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM sso.escopo
            WHERE LOWER(nome) = LOWER(@nome)
              AND (@IdIgnorar IS NULL OR id != @IdIgnorar)";

        var total = await QuerySingleAsync<int>(sql, new { nome, IdIgnorar = idIgnorar });
        return total > 0;
    }
}
