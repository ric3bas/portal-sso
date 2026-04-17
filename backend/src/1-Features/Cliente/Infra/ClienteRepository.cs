using Portal.Features.Cliente.Domain;
using Portal.Features.Cliente.Domain.Interfaces;
using Portal.Infra;
using System.Text.RegularExpressions;

namespace Portal.Features.Cliente.Infra
{
    public class ClienteRepository : DapperRepository, IClienteRepository
    {
        public ClienteRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<IEnumerable<ClienteEntity>> ObterTodosAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT Id, Nome, Cpf, Email, Observacao, Bloqueado, Ativo, parceiro_id as ParceiroId 
                FROM l6a.Cliente 
                ORDER BY Nome";

            var clientes = await QueryAsync<ClienteEntity>(sql);

            foreach (var cliente in clientes)
            {
                cliente.Telefone = (await ObterTelefoneAsync(cliente.Id, cancellationToken));
                cliente.Endereco = (await ObterEnderecoAsync(cliente.Id, cancellationToken));
            }

            return clientes;
        }

        public async Task<IEnumerable<ClienteEntity>> ObterPorFiltroAsync(FiltroClienteRequest filtro, CancellationToken cancellationToken)
        {
            var whereConditions = new List<string>();
            var parameters = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(filtro.Nome))
            {
                whereConditions.Add("Nome LIKE @Nome");
                parameters.Add("Nome", $"%{filtro.Nome}%");
            }

            if (!string.IsNullOrWhiteSpace(filtro.Cpf))
            {
                var cpfNumerico = Regex.Replace(filtro.Cpf, @"[^\d]", "");
                whereConditions.Add("Cpf LIKE @Cpf");
                parameters.Add("Cpf", $"%{cpfNumerico}%");
            }

            var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";

            var sql = $@"
                SELECT Id, Nome, Cpf, Email, Observacao, Bloqueado, Ativo, parceiro_id as ParceiroId 
                FROM l6a.Cliente 
                {whereClause}
                ORDER BY Nome";

            var clientes = await QueryAsync<ClienteEntity>(sql, parameters);
            
            foreach (var cliente in clientes)
            {
                cliente.Telefone = (await ObterTelefoneAsync(cliente.Id, cancellationToken));
                cliente.Endereco = (await ObterEnderecoAsync(cliente.Id, cancellationToken));
            }

            return clientes;
        }

        public async Task<ClienteEntity?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT Id, Nome, Cpf, Email, Observacao, Bloqueado, Ativo, parceiro_id as ParceiroId 
                FROM l6a.Cliente 
                WHERE Id = @Id";

            var cliente = await QuerySingleAsync<ClienteEntity>(sql, new { Id = id });
            if (cliente != null)
            {
                cliente.Telefone= (await ObterTelefoneAsync(cliente.Id, cancellationToken));
                cliente.Endereco = (await ObterEnderecoAsync(cliente.Id, cancellationToken));
            }

            return cliente;
        }

        public async Task<Guid> CriarAsync(ClienteRequest cliente, CancellationToken cancellationToken)
        {
            var id = Guid.NewGuid();
            var cpfNumerico = Regex.Replace(cliente.Cpf, @"[^\d]", "");

            const string sql = @"
                INSERT INTO l6a.Cliente (Id, Nome, Cpf, Email, Observacao, Bloqueado, Ativo, parceiro_id) 
                VALUES (@Id, @Nome, @Cpf, @Email, @Observacao, @Bloqueado, @Ativo, @ParceiroId)";

            await ExecuteAsync(sql, new
            {
                Id = id,
                cliente.Nome,
                Cpf = cpfNumerico,
                cliente.Email,
                Observacao = cliente.Observacao ?? string.Empty,
                Bloqueado = false,
                Ativo = true,
                ParceiroId = cliente.ParceiroId
            });

            // Inserir telefones
            await AtualizarTelefoneAsync(id, cliente.Telefone, cancellationToken);

            // Inserir endereços
            await AtualizarEnderecoAsync(id, cliente.Endereco, cancellationToken);

            return id;
        }

        public async Task<int> AtualizarAsync(Guid id, AtualizarClienteRequest cliente, CancellationToken cancellationToken)
        {
            var cpfNumerico = Regex.Replace(cliente.Cpf, @"[^\d]", "");

            const string sql = @"
                UPDATE l6a.Cliente 
                SET Nome = @Nome, 
                    Cpf = @Cpf,
                    Email = @Email,
                    Observacao = @Observacao,
                    Bloqueado = COALESCE(@Bloqueado, Bloqueado),
                    Ativo = COALESCE(@Ativo, Ativo)
                WHERE Id = @Id AND parceiro_id = @ParceiroId";

            var result = await ExecuteAsync(sql, new
            {
                Id = id,
                cliente.Nome,
                Cpf = cpfNumerico,
                cliente.Email,
                Observacao = cliente.Observacao ?? string.Empty,
                cliente.Bloqueado,
                cliente.Ativo
            });

            // Atualizar telefones
            await AtualizarTelefoneAsync(id, cliente.Telefone, cancellationToken);

            // Atualizar endereços
            await AtualizarEnderecoAsync(id, cliente.Endereco, cancellationToken);

            return result;
        }

        public async Task<int> BloquearAsync(Guid id, CancellationToken cancellationToken)
        {
            const string sql = @"
                UPDATE l6a.Cliente 
                SET Bloqueado = true 
                WHERE Id = @Id";

            return await ExecuteAsync(sql, new { Id = id});
        }

        public async Task<int> DesbloquearAsync(Guid id, CancellationToken cancellationToken)
        {
            const string sql = @"
                UPDATE l6a.Cliente 
                SET Bloqueado = false 
                WHERE Id = @Id";

            return await ExecuteAsync(sql, new { Id = id});
        }

        public async Task<int> InativarAsync(Guid id, CancellationToken cancellationToken)
        {
            const string sql = @"
                UPDATE l6a.Cliente 
                SET Ativo = false 
                WHERE Id = @Id";

            return await ExecuteAsync(sql, new { Id = id});
        }

        public async Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT COUNT(1) 
                FROM l6a.Cliente 
                WHERE Id = @Id";

            var count = await QuerySingleAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> ExisteCpfAsync(string cpf, Guid? idIgnorar = null, CancellationToken cancellationToken = default)
        {
            var cpfNumerico = Regex.Replace(cpf, @"[^\d]", "");

            const string sql = @"
                SELECT COUNT(1) 
                FROM l6a.Cliente 
                WHERE Cpf = @Cpf 
                AND (@IdIgnorar IS NULL OR Id != @IdIgnorar)";

            var count = await QuerySingleAsync<int>(sql, new { Cpf = cpfNumerico, IdIgnorar = idIgnorar });
            return count > 0;
        }

        public async Task<bool> ExisteEmailAsync(string email, Guid? idIgnorar = null, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT COUNT(1) 
                FROM l6a.Cliente 
                WHERE Email = @Email 
                AND (@IdIgnorar IS NULL OR Id != @IdIgnorar)";

            var count = await QuerySingleAsync<int>(sql, new { Email = email, IdIgnorar = idIgnorar });
            return count > 0;
        }

        public async Task<TelefoneEntity> ObterTelefoneAsync(Guid clienteId, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT Id, cliente_id as ClienteId, Ddd, Numero 
                FROM l6a.Telefone 
                WHERE cliente_id = @ClienteId";

            var telefones = await QuerySingleAsync<TelefoneEntity>(sql, new { ClienteId = clienteId });
            return telefones;
        }

        public async Task<EnderecoEntity> ObterEnderecoAsync(Guid clienteId, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT Id, cliente_id as ClienteId, Logradouro, Cidade, Estado, Numero, Complemento 
                FROM l6a.Endereco 
                WHERE cliente_id = @ClienteId";

            var enderecos = await QuerySingleAsync<EnderecoEntity>(sql, new { ClienteId = clienteId });
            return enderecos;
        }

        public async Task AtualizarTelefoneAsync(Guid clienteId, TelefoneRequest telefone, CancellationToken cancellationToken)
        {
            // Remove telefones existentes
            const string deleteSql = "DELETE FROM l6a.Telefone WHERE cliente_id = @ClienteId";
            await ExecuteAsync(deleteSql, new { ClienteId = clienteId });

            const string insertSql = @"
                    INSERT INTO l6a.Telefone (Id, cliente_id, Ddd, Numero) 
                    VALUES (@Id, @ClienteId, @Ddd, @Numero)";

            await ExecuteAsync(insertSql, new
            {
                Id = Guid.NewGuid(),
                ClienteId = clienteId,
                telefone.Ddd,
                telefone.Numero
            });
        }

        public async Task AtualizarEnderecoAsync(Guid clienteId, EnderecoRequest endereco, CancellationToken cancellationToken)
        {
            // Remove endereços existentes
            const string deleteSql = "DELETE FROM l6a.Endereco WHERE cliente_id = @ClienteId";
            await ExecuteAsync(deleteSql, new { ClienteId = clienteId });

            const string insertSql = @"
                    INSERT INTO l6a.Endereco (Id, cliente_id, Logradouro, Cidade, Estado, Numero, Complemento) 
                    VALUES (@Id, @ClienteId, @Logradouro, @Cidade, @Estado, @Numero, @Complemento)";


            await ExecuteAsync(insertSql, new
            {
                Id = Guid.NewGuid(),
                ClienteId = clienteId,
                endereco.Logradouro,
                endereco.Cidade,
                endereco.Estado,
                endereco.Numero,
                Complemento = endereco.Complemento ?? string.Empty
            });
        }

        //public Task<TelefoneEntity> ObterTelefoneAsync(Guid clienteId, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<EnderecoEntity> ObterEnderecoAsync(Guid clienteId, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task AtualizarTelefoneAsync(Guid clienteId, TelefoneRequest telefone, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task AtualizarEnderecoAsync(Guid clienteId, EnderecoRequest endereco, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}
    }
}