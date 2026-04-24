using Portal.Domain.Cliente;
using Portal.Domain.Cliente.Interfaces;
using Portal.Domain.Common;
using Dapper;
using DataDapperRepository = Portal.Infrastructure.Data.DapperRepository;
using DataUnitOfWork = Portal.Infrastructure.Data.IUnitOfWork;
using System.Text.RegularExpressions;

namespace Portal.Infrastructure.Repositories;

public class ClienteRepository : DataDapperRepository, IClienteRepository
{
    public ClienteRepository(DataUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<ResultadoPaginado<ClienteQuery>> ObterTodosAsync(Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken)
    {
        var paginacao = PaginacaoHelper.Criar(direcao, pagina, tamanhoPagina);

        var sql = $@"
            SELECT COUNT(1) FROM l6a.Cliente;

            SELECT Id, Nome, Cpf, Email, Observacao, Bloqueado, Ativo, parceiro_id as ParceiroId 
            FROM l6a.Cliente 
            ORDER BY Nome {paginacao.OrdemSql}
            LIMIT @TamanhoPagina OFFSET @Offset";

        using var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, new { TamanhoPagina = paginacao.TamanhoPagina, Offset = paginacao.Offset });
        var total = await multi.ReadSingleAsync<int>();
        var clientes = (await multi.ReadAsync<ClienteQuery>()).ToList();

        foreach (var cliente in clientes)
        {
            cliente.Telefone = await ObterTelefoneAsync(cliente.Id, cancellationToken);
            cliente.Endereco = await ObterEnderecoAsync(cliente.Id, cancellationToken);
        }

        return new ResultadoPaginado<ClienteQuery>(clientes, total, paginacao.Pagina, paginacao.TamanhoPagina);
    }

    public async Task<ResultadoPaginado<ClienteQuery>> ObterPorFiltroAsync(string? nome, string? cpf, Direcao direcao, int pagina, int tamanhoPagina, CancellationToken cancellationToken)
    {
        var whereConditions = new List<string>();
        var parameters = new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(nome))
        {
            whereConditions.Add("Nome ILIKE @Nome");
            parameters.Add("Nome", $"%{nome}%");
        }

        if (!string.IsNullOrWhiteSpace(cpf))
        {
            var cpfNumerico = Regex.Replace(cpf, @"[^\d]", "");
            whereConditions.Add("Cpf LIKE @Cpf");
            parameters.Add("Cpf", $"%{cpfNumerico}%");
        }

        var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : string.Empty;

        var paginacao = PaginacaoHelper.Criar(direcao, pagina, tamanhoPagina);

        var sql = $@"
            SELECT COUNT(1)
            FROM l6a.Cliente
            {whereClause};

            SELECT Id, Nome, Cpf, Email, Observacao, Bloqueado, Ativo, parceiro_id as ParceiroId 
            FROM l6a.Cliente 
            {whereClause}
            ORDER BY Nome {paginacao.OrdemSql}
            LIMIT @TamanhoPagina OFFSET @Offset";

        parameters["TamanhoPagina"] = paginacao.TamanhoPagina;
        parameters["Offset"] = paginacao.Offset;

        using var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, parameters);
        var total = await multi.ReadSingleAsync<int>();
        var clientes = (await multi.ReadAsync<ClienteQuery>()).ToList();

        foreach (var cliente in clientes)
        {
            cliente.Telefone = await ObterTelefoneAsync(cliente.Id, cancellationToken);
            cliente.Endereco = await ObterEnderecoAsync(cliente.Id, cancellationToken);
        }

        return new ResultadoPaginado<ClienteQuery>(clientes, total, paginacao.Pagina, paginacao.TamanhoPagina);
    }

    public async Task<ClienteQuery?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT Id, Nome, Cpf, Email, Observacao, Bloqueado, Ativo, parceiro_id as ParceiroId 
            FROM l6a.Cliente 
            WHERE Id = @Id";

        var cliente = await QuerySingleAsync<ClienteQuery>(sql, new { Id = id });
        if (cliente is null)
        {
            return null;
        }

        cliente.Telefone = await ObterTelefoneAsync(cliente.Id, cancellationToken);
        cliente.Endereco = await ObterEnderecoAsync(cliente.Id, cancellationToken);

        return cliente;
    }

    public async Task<Guid> CriarAsync(ClienteCommand cliente, CancellationToken cancellationToken)
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
            cliente.Bloqueado,
            cliente.Ativo,
            ParceiroId = cliente.ParceiroId
        });

        if (cliente.Telefone is not null)
        {
            await AtualizarTelefoneAsync(id, cliente.Telefone, cancellationToken);
        }

        if (cliente.Endereco is not null)
        {
            await AtualizarEnderecoAsync(id, cliente.Endereco, cancellationToken);
        }

        return id;
    }

    public async Task<int> AtualizarAsync(ClienteCommand cliente, CancellationToken cancellationToken)
    {
        var cpfNumerico = Regex.Replace(cliente.Cpf, @"[^\d]", "");

        const string sql = @"
            UPDATE l6a.Cliente 
            SET Nome = @Nome,
                Cpf = @Cpf,
                Email = @Email,
                Observacao = @Observacao,
                Bloqueado = @Bloqueado,
                Ativo = @Ativo
            WHERE Id = @Id";

        var result = await ExecuteAsync(sql, new
        {
            cliente.Id,
            cliente.Nome,
            Cpf = cpfNumerico,
            cliente.Email,
            Observacao = cliente.Observacao ?? string.Empty,
            cliente.Bloqueado,
            cliente.Ativo
        });

        if (cliente.Telefone is not null)
        {
            await AtualizarTelefoneAsync(cliente.Id, cliente.Telefone, cancellationToken);
        }

        if (cliente.Endereco is not null)
        {
            await AtualizarEnderecoAsync(cliente.Id, cliente.Endereco, cancellationToken);
        }

        return result;
    }

    public async Task<int> DefinirBloqueadoAsync(Guid id, bool bloqueado, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE l6a.Cliente
            SET Bloqueado = @Bloqueado
            WHERE Id = @Id";

        return await ExecuteAsync(sql, new { Id = id, Bloqueado = bloqueado });
    }

    public async Task<int> InativarAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE l6a.Cliente
            SET Ativo = false
            WHERE Id = @Id";

        return await ExecuteAsync(sql, new { Id = id });
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

    private async Task<TelefoneQuery?> ObterTelefoneAsync(Guid clienteId, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT Id, cliente_id as ClienteId, Ddd, Numero
            FROM l6a.Telefone
            WHERE cliente_id = @ClienteId";

        return await QuerySingleAsync<TelefoneQuery>(sql, new { ClienteId = clienteId });
    }

    private async Task<EnderecoQuery?> ObterEnderecoAsync(Guid clienteId, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT Id, cliente_id as ClienteId, Logradouro, Cidade, Estado, Numero, Complemento
            FROM l6a.Endereco
            WHERE cliente_id = @ClienteId";

        return await QuerySingleAsync<EnderecoQuery>(sql, new { ClienteId = clienteId });
    }

    private async Task AtualizarTelefoneAsync(Guid clienteId, TelefoneCommand telefone, CancellationToken cancellationToken)
    {
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

    private async Task AtualizarEnderecoAsync(Guid clienteId, EnderecoCommand endereco, CancellationToken cancellationToken)
    {
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
}
