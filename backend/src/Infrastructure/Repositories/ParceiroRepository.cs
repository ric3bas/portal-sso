using Dapper;
using Portal.Domain.Common;
using Portal.Domain.Parceiro;
using Portal.Domain.Parceiro.Interfaces;
using DataDapperRepository = Portal.Infrastructure.Data.DapperRepository;
using DataUnitOfWork = Portal.Infrastructure.Data.IUnitOfWork;

namespace Portal.Infrastructure.Repositories;

public class ParceiroRepository : DataDapperRepository, IParceiroRepository
{
    public ParceiroRepository(DataUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<ResultadoPaginado<ParceiroQuery>> ObterTodosAsync(Guid? id, Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken = default)
    {
        var paginacao = PaginacaoHelper.Criar(direcao, pagina, tamanhoPagina);
        var temFiltroId = id.HasValue && id.Value != Guid.Empty;

        var where = temFiltroId ? " WHERE id = @id" : string.Empty;
        var sql = $"SELECT COUNT(1) FROM sso.parceiro{where};"
                + $" SELECT id, nome, descricao, cor_primaria as CorPrimaria, cor_secundaria as CorSecundaria, ativo FROM sso.parceiro{where} ORDER BY nome {paginacao.OrdemSql} LIMIT @TamanhoPagina OFFSET @Offset";

        using var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, temFiltroId
            ? new { id, TamanhoPagina = paginacao.TamanhoPagina, Offset = paginacao.Offset }
            : new { TamanhoPagina = paginacao.TamanhoPagina, Offset = paginacao.Offset });
        var total = await multi.ReadSingleAsync<int>();
        var itens = (await multi.ReadAsync<ParceiroQuery>()).ToList();
        return new ResultadoPaginado<ParceiroQuery>(itens, total, paginacao.Pagina, paginacao.TamanhoPagina);
    }

    public async Task<ResultadoPaginado<ParceiroQuery>> ObterTodosPorFiltroAsync(string nome, Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken = default)
    {
        var paginacao = PaginacaoHelper.Criar(direcao, pagina, tamanhoPagina);

        var temFiltro = !string.IsNullOrEmpty(nome);
        var where = temFiltro ? " WHERE LOWER(nome) LIKE LOWER(@Nome)" : string.Empty;
        var sql = $"SELECT COUNT(1) FROM sso.parceiro{where};"
                + $" SELECT id, nome, descricao, cor_primaria as CorPrimaria, cor_secundaria as CorSecundaria, ativo FROM sso.parceiro{where} ORDER BY nome {paginacao.OrdemSql} LIMIT @TamanhoPagina OFFSET @Offset";

        using var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, string.IsNullOrEmpty(nome)
            ? new { TamanhoPagina = paginacao.TamanhoPagina, Offset = paginacao.Offset }
            : new { Nome = $"%{nome}%", TamanhoPagina = paginacao.TamanhoPagina, Offset = paginacao.Offset });
        var total = await multi.ReadSingleAsync<int>();
        var itens = (await multi.ReadAsync<ParceiroQuery>()).ToList();
        return new ResultadoPaginado<ParceiroQuery>(itens, total, paginacao.Pagina, paginacao.TamanhoPagina);
    }

    public async Task<ParceiroQuery?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT id, nome, descricao, cor_primaria as CorPrimaria, cor_secundaria as CorSecundaria, ativo FROM sso.parceiro WHERE id = @Id";
        return await QuerySingleAsync<ParceiroQuery>(sql, new { Id = id });
    }

    public async Task<ParceiroQuery?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT id, nome, descricao, cor_primaria as CorPrimaria, cor_secundaria as CorSecundaria, ativo FROM sso.parceiro WHERE LOWER(nome) = LOWER(@Nome) LIMIT 1";
        return await QuerySingleAsync<ParceiroQuery>(sql, new { Nome = nome });
    }

    public async Task<(bool Existe, bool NomeConflito)> ValidarAtualizacaoAsync(Guid id, string nome, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(1) FROM sso.parceiro WHERE id = @Id;
            SELECT COUNT(1) FROM sso.parceiro WHERE LOWER(nome) = LOWER(@Nome) AND id != @Id;";

        using var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, new { Id = id, Nome = nome });
        return (await multi.ReadSingleAsync<int>() > 0, await multi.ReadSingleAsync<int>() > 0);
    }

    public async Task<Guid> InserirAsync(ParceiroCommand parceiro, CancellationToken cancellationToken = default)
    {
        const string sql = "INSERT INTO sso.parceiro (id, nome, descricao, cor_primaria, cor_secundaria, ativo) VALUES (@Id, @Nome, @Descricao, @CorPrimaria, @CorSecundaria, @Ativo)";
        await ExecuteAsync(sql, parceiro);
        return parceiro.Id;
    }

    public async Task AtualizarAsync(ParceiroCommand parceiro, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE sso.parceiro SET nome = @Nome, descricao = @Descricao, cor_primaria = @CorPrimaria, cor_secundaria = @CorSecundaria, ativo = @Ativo WHERE id = @Id";
        await ExecuteAsync(sql, parceiro);
    }
}
