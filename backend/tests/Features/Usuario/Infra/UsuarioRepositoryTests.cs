using NSubstitute;
using Portal.Features.Usuario.Infra;
using Portal.Infra;
using System.Data;

namespace sso.repositories;


public class UsuarioRepositoryTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDbConnection _connection;
    private readonly IDbTransaction _transaction;
    private readonly TestableUsuarioRepository _repository;

    public UsuarioRepositoryTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _connection = Substitute.For<IDbConnection>();
        _transaction = Substitute.For<IDbTransaction>();

        _unitOfWork.Connection.Returns(_connection);
        _unitOfWork.Transaction.Returns(_transaction);

        _repository = new TestableUsuarioRepository(_unitOfWork);
    }

    [Fact]
    public void Constructor_InitializesRepository()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var repository = new UsuarioRepository(unitOfWork);

        Assert.NotNull(repository);
    }

    [Fact]
    public async Task IncrementarTentativaLoginAsync_ValidUserId_ExecutesUpdate()
    {
        var usuarioId = 123;
        var expectedSql = "UPDATE sso.usuario SET tentativas_login = tentativas_login + 1, ultimo_erro_login = NOW() WHERE id = @usuarioId";
        _repository.ExecuteAsyncCalls.Clear();

        await _repository.IncrementarTentativaLoginAsync(usuarioId);

        Assert.Single(_repository.ExecuteAsyncCalls);
        var call = _repository.ExecuteAsyncCalls[0];
        Assert.Equal(expectedSql, call.sql);
        Assert.NotNull(call.param);
        var property = call.param!.GetType().GetProperty("usuarioId");
        Assert.NotNull(property);
        Assert.Equal(usuarioId, property!.GetValue(call.param));
    }

    [Fact]
    public async Task IncrementarTentativaLoginAsync_WithCancellationToken_ExecutesUpdate()
    {
        var usuarioId = 456;
        var cancellationToken = new CancellationToken();

        await _repository.IncrementarTentativaLoginAsync(usuarioId, cancellationToken);

        Assert.Single(_repository.ExecuteAsyncCalls);
    }

    [Fact]
    public async Task IncrementarTentativaLoginAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var usuarioId = 789;
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _repository.IncrementarTentativaLoginAsync(usuarioId, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task ResetarTentativasLoginAsync_ValidUserId_ExecutesUpdate()
    {
        var usuarioId = 123;
        var expectedSql = "UPDATE sso.usuario SET tentativas_login = 0, ultimo_erro_login = NULL WHERE id = @usuarioId";
        _repository.ExecuteAsyncCalls.Clear();

        await _repository.ResetarTentativasLoginAsync(usuarioId);

        Assert.Single(_repository.ExecuteAsyncCalls);
        var call = _repository.ExecuteAsyncCalls[0];
        Assert.Equal(expectedSql, call.sql);
        Assert.NotNull(call.param);
        var property = call.param!.GetType().GetProperty("usuarioId");
        Assert.NotNull(property);
        Assert.Equal(usuarioId, property!.GetValue(call.param));
    }

    [Fact]
    public async Task ResetarTentativasLoginAsync_WithCancellationToken_ExecutesUpdate()
    {
        var usuarioId = 456;
        var cancellationToken = new CancellationToken();

        await _repository.ResetarTentativasLoginAsync(usuarioId, cancellationToken);

        Assert.Single(_repository.ExecuteAsyncCalls);
    }

    [Fact]
    public async Task ResetarTentativasLoginAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var usuarioId = 789;
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _repository.ResetarTentativasLoginAsync(usuarioId, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task BloquearUsuarioAsync_ValidUserId_ExecutesUpdate()
    {
        var usuarioId = 123;
        var expectedSql = "UPDATE sso.usuario SET tentativas_login = 5 WHERE id = @usuarioId";
        _repository.ExecuteAsyncCalls.Clear();

        await _repository.BloquearUsuarioAsync(usuarioId);

        Assert.Single(_repository.ExecuteAsyncCalls);
        var call = _repository.ExecuteAsyncCalls[0];
        Assert.Equal(expectedSql, call.sql);
        Assert.NotNull(call.param);
        var property = call.param!.GetType().GetProperty("usuarioId");
        Assert.NotNull(property);
        Assert.Equal(usuarioId, property!.GetValue(call.param));
    }

    [Fact]
    public async Task BloquearUsuarioAsync_WithCancellationToken_ExecutesUpdate()
    {
        var usuarioId = 456;
        var cancellationToken = new CancellationToken();

        await _repository.BloquearUsuarioAsync(usuarioId, cancellationToken);

        Assert.Single(_repository.ExecuteAsyncCalls);
    }

    [Fact]
    public async Task BloquearUsuarioAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var usuarioId = 789;
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _repository.BloquearUsuarioAsync(usuarioId, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task ObterPorIdAsync_ValidUserId_ReturnsUsuario()
    {
        var usuarioId = 123;
        var expectedSql = "SELECT id, nome, email, login, senha, parceiro_id, perfil_id, tentativas_login, ultimo_erro_login FROM sso.usuario WHERE id = @usuarioId";
        var expectedUsuario = new UsuarioCommand
        {
            Id = usuarioId,
            Nome = "Test User",
            Email = "test@example.com",
            Login = "testuser"
        };
        _repository.SetQuerySingleAsyncResult(expectedUsuario);
        _repository.QuerySingleAsyncCalls.Clear();

        var result = await _repository.ObterPorIdAsync(usuarioId);

        Assert.NotNull(result.Data);
        Assert.Equal(expectedUsuario.Id, result!.Id);
        Assert.Equal(expectedUsuario.Nome, result.Data.Nome);
        Assert.Single(_repository.QuerySingleAsyncCalls);
        var call = _repository.QuerySingleAsyncCalls[0];
        Assert.Equal(expectedSql, call.sql);
        Assert.NotNull(call.param);
        var property = call.param!.GetType().GetProperty("usuarioId");
        Assert.NotNull(property);
        Assert.Equal(usuarioId, property!.GetValue(call.param));
    }

    [Fact]
    public async Task ObterPorIdAsync_UserNotFound_ReturnsNull()
    {
        var usuarioId = 999;
        _repository.SetQuerySingleAsyncResult<UsuarioQuery>(null);

        var result = await _repository.ObterPorIdAsync(usuarioId);

        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ObterPorIdAsync_WithCancellationToken_ExecutesQuery()
    {
        var usuarioId = 456;
        var cancellationToken = new CancellationToken();
        _repository.SetQuerySingleAsyncResult<UsuarioQuery>(null);

        await _repository.ObterPorIdAsync(usuarioId, cancellationToken);

        Assert.Single(_repository.QuerySingleAsyncCalls);
    }

    [Fact]
    public async Task ObterPorIdAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var usuarioId = 789;
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _repository.ObterPorIdAsync(usuarioId, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task AtualizarAsync_ValidUsuario_ExecutesUpdate()
    {
        var usuario = new UsuarioCommand
        {
            Id = 123,
            Nome = "Updated User",
            Email = "updated@example.com",
            Login = "updateduser",
            Senha = "hashedpassword",
            ParceiroId = Guid.NewGuid(),
            PerfilId = 5,
            TentativasLogin = 2,
            UltimoErroLogin = DateTime.Now
        };
        _repository.ExecuteAsyncCalls.Clear();

        await _repository.AtualizarAsync(usuario);

        Assert.Single(_repository.ExecuteAsyncCalls);
        var call = _repository.ExecuteAsyncCalls[0];
        Assert.Contains("UPDATE sso.usuario SET", call.sql);
        Assert.Contains("nome = @Nome", call.sql);
        Assert.Contains("email = @Email", call.sql);
        Assert.Contains("login = @Login", call.sql);
        Assert.Contains("senha = @Senha", call.sql);
        Assert.Contains("parceiro_id = @ParceiroId", call.sql);
        Assert.Contains("perfil_id = @PerfilId", call.sql);
        Assert.Contains("tentativas_login = @TentativasLogin", call.sql);
        Assert.Contains("ultimo_erro_login = @UltimoErroLogin", call.sql);
        Assert.Contains("WHERE id = @Id", call.sql);
        Assert.Same(usuario, call.param);
    }

    [Fact]
    public async Task AtualizarAsync_WithCancellationToken_ExecutesUpdate()
    {
        var usuario = new UsuarioCommand
        {
            Id = 456,
            Nome = "Test User",
            Email = "test@example.com",
            Login = "testuser",
            Senha = "password",
            ParceiroId = Guid.NewGuid(),
            PerfilId = 1,
            TentativasLogin = 0,
            UltimoErroLogin = null
        };
        var cancellationToken = new CancellationToken();

        await _repository.AtualizarAsync(usuario, cancellationToken);

        Assert.Single(_repository.ExecuteAsyncCalls);
    }

    [Fact]
    public async Task AtualizarAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var usuario = new UsuarioCommand
        {
            Id = 789,
            Nome = "Test User",
            Email = "test@example.com",
            Login = "testuser",
            Senha = "password",
            ParceiroId = Guid.NewGuid(),
            PerfilId = 1,
            TentativasLogin = 0,
            UltimoErroLogin = null
        };
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _repository.AtualizarAsync(usuario, cancellationTokenSource.Token));
    }

    private class TestableUsuarioRepository : UsuarioRepository
    {
        public List<(string sql, object? param)> ExecuteAsyncCalls { get; } = new();
        public List<(string sql, object? param)> QuerySingleAsyncCalls { get; } = new();
        private object? _querySingleAsyncResult;

        public TestableUsuarioRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void SetQuerySingleAsyncResult<T>(T? result)
        {
            _querySingleAsyncResult = result;
        }

        public override async Task<int> ExecuteAsync(string sql, object? param = null, int? timeout = null)
        {
            ExecuteAsyncCalls.Add((sql, param));
            return await Task.FromResult(1);
        }

        public override async Task<T?> QuerySingleAsync<T>(string sql, object? param = null, int? timeout = null) where T : default
        {
            QuerySingleAsyncCalls.Add((sql, param));
            if (_querySingleAsyncResult is T result)
            {
                return await Task.FromResult(result);
            }
            return await Task.FromResult<T?>(default);
        }
    }
}
