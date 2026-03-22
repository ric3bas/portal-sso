using Dapper;
using Portal.Features.Parceiro.Domain.Interfaces;
using Portal.Infra;
using System.Data;

namespace Portal.Features.Parceiro.Infra {
    [DbContext("SSO_POSTGRES")]
    public class ParceiroRepository : DapperRepository, IDapperRepository, IParceiroRepository {
        public ParceiroRepository(IUnitOfWork unitOfWork) : base(unitOfWork) {}

        public async Task<IEnumerable<ParceiroQuery>> ObterTodosAsync(string? nome, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            var sql   = "SELECT id, nome, descricao, ativo FROM sso.parceiro";
            object? param = null;
            if (!string.IsNullOrWhiteSpace(nome)) {
                sql  += " WHERE nome ILIKE @Nome";
                param = new { Nome = $"%{nome}%" };
            }
            return await QueryAsync<ParceiroQuery>(sql, param);
        }

        public async Task<ParceiroQuery?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "SELECT id, nome, descricao, ativo FROM sso.parceiro WHERE id = @Id";
            return await QuerySingleAsync<ParceiroQuery>(sql, new { Id = id });
        }

        public async Task<ParceiroQuery?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "SELECT id, nome, descricao, ativo FROM sso.parceiro WHERE LOWER(nome) = LOWER(@Nome) LIMIT 1";
            return await QuerySingleAsync<ParceiroQuery>(sql, new { Nome = nome });
        }

        public async Task<(bool Existe, bool NomeConflito)> ValidarAtualizacaoAsync(Guid id, string nome, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = @"
                SELECT COUNT(1) FROM sso.parceiro WHERE id = @Id;
                SELECT COUNT(1) FROM sso.parceiro WHERE LOWER(nome) = LOWER(@Nome) AND id != @Id;";

            using var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, new { Id = id, Nome = nome });
            return (await multi.ReadSingleAsync<int>() > 0, await multi.ReadSingleAsync<int>() > 0);
        }


        public async Task<Guid> InserirAsync(ParceiroCommand parceiro, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@Id, @Nome, @Descricao, @Ativo)";
            await ExecuteAsync(sql, parceiro);
            return parceiro.Id;
        }

        public async Task AtualizarAsync(ParceiroCommand parceiro, CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "UPDATE sso.parceiro SET nome = @Nome, descricao = @Descricao, ativo = @Ativo WHERE id = @Id";
            await ExecuteAsync(sql, parceiro);
        }
    }
}