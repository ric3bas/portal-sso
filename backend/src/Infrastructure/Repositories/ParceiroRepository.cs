using Dapper;
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

    public async Task<IEnumerable<ParceiroQuery>> ObterTodosAsync(Guid? id, CancellationToken cancellationToken = default)
    {
        var sql = "SELECT id, nome, descricao, ativo FROM sso.parceiro";
        object? param = null;

        if (id != Guid.Empty)
        {
            sql += " WHERE id = @id";
            param = new { id };
        }

        return await QueryAsync<ParceiroQuery>(sql, param);
    }

    public async Task<IEnumerable<ParceiroQuery>> ObterTodosPorFiltroAsync(string nome, CancellationToken cancellationToken = default)
    {
        var sql = "SELECT id, nome, descricao, ativo FROM sso.parceiro";
        object? param = null;

        if (!string.IsNullOrEmpty(nome))
        {
            sql += " WHERE LOWER(nome) LIKE LOWER(@Nome)";
            param = new { Nome = $"%{nome}%" };
        }

        return await QueryAsync<ParceiroQuery>(sql, param);
    }

    public async Task<ParceiroQuery?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT id, nome, descricao, ativo FROM sso.parceiro WHERE id = @Id";
        return await QuerySingleAsync<ParceiroQuery>(sql, new { Id = id });
    }

    public async Task<ParceiroQuery?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT id, nome, descricao, ativo FROM sso.parceiro WHERE LOWER(nome) = LOWER(@Nome) LIMIT 1";
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
        const string sql = "INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@Id, @Nome, @Descricao, @Ativo)";
        await ExecuteAsync(sql, parceiro);
        return parceiro.Id;
    }

    public async Task AtualizarAsync(ParceiroCommand parceiro, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE sso.parceiro SET nome = @Nome, descricao = @Descricao, ativo = @Ativo WHERE id = @Id";
        await ExecuteAsync(sql, parceiro);
    }
}
