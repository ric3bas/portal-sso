using System.Data;
using Dapper;
using Portal.Infra;
using ParceiroEntity = Portal.Domain.Entities.ParceiroEntity;
using Portal.Features.Parceiro.Domain;
using Portal.Features.Parceiro.Domain.Interfaces;

namespace Portal.Features.Parceiro.Infra {
    [DbContext("SSO_POSTGRES")]
    public class ParceiroRepository : DapperRepository, IDapperRepository, IParceiroRepository {
        public ParceiroRepository(IUnitOfWork unitOfWork) : base(unitOfWork) {}

        public async Task<IEnumerable<ParceiroResponse>> ObterTodosAsync(string? nome, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            var sql   = "SELECT id, nome, descricao, ativo FROM sso.parceiro";
            object? param = null;
            if (!string.IsNullOrWhiteSpace(nome)) {
                sql  += " WHERE nome ILIKE @Nome";
                param = new { Nome = $"%{nome}%" };
            }
            var result = await QueryAsync<ParceiroEntity>(sql, param);
            return result.Select(p => p.ToResponse());
        }

        public async Task<ParceiroResponse?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "SELECT id, nome, descricao, ativo FROM sso.parceiro WHERE id = @Id";
            var result = await QuerySingleAsync<ParceiroEntity>(sql, new { Id = id });
            return result?.ToResponse();
        }

        public async Task<ParceiroResponse?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "SELECT id, nome, descricao, ativo FROM sso.parceiro WHERE LOWER(nome) = LOWER(@Nome) LIMIT 1";
            var result = await QuerySingleAsync<ParceiroEntity>(sql, new { Nome = nome });
            return result?.ToResponse();
        }

        public async Task<(bool Existe, bool NomeConflito)> ValidarAtualizacaoAsync(Guid id, string nome, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = @"
                SELECT COUNT(1) FROM sso.parceiro WHERE id = @Id;
                SELECT COUNT(1) FROM sso.parceiro WHERE LOWER(nome) = LOWER(@Nome) AND id != @Id;";
            // QueryMultipleAsync não está disponível diretamente, então mantenha o uso do connection se necessário
            using var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, new { Id = id, Nome = nome });
            return (await multi.ReadSingleAsync<int>() > 0, await multi.ReadSingleAsync<int>() > 0);
        }


        public async Task<Guid> InserirAsync(ParceiroEntity parceiro, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "INSERT INTO sso.parceiro (id, nome, descricao, ativo) VALUES (@Id, @Nome, @Descricao, @Ativo)";
            await ExecuteAsync(sql, parceiro);
            return parceiro.Id;
        }

        public async Task AtualizarAsync(ParceiroEntity parceiro, CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            const string sql = "UPDATE sso.parceiro SET nome = @Nome, descricao = @Descricao, ativo = @Ativo WHERE id = @Id";
            await ExecuteAsync(sql, parceiro);
        }
    }
}