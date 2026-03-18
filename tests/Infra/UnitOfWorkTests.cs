using System.Data;
using NSubstitute;
using Portal.Infra;

namespace sso.repositories;
public class UnitOfWorkTests
{
    [Fact]
    public void Begin_WhenConnectionClosed_OpensConnectionAndStartsTransaction()
    {
        var connection = Substitute.For<IDbConnection>();
        var transaction = Substitute.For<IDbTransaction>();
        connection.State.Returns(ConnectionState.Closed);
        connection.BeginTransaction().Returns(transaction);

        var unitOfWork = new UnitOfWork(connection);

        unitOfWork.Begin();

        connection.Received(1).Open();
        connection.Received(1).BeginTransaction();
        Assert.NotNull(unitOfWork.Transaction);
    }

    [Fact]
    public void Begin_WhenConnectionAlreadyOpen_DoesNotOpenAgain()
    {
        var connection = Substitute.For<IDbConnection>();
        var transaction = Substitute.For<IDbTransaction>();
        connection.State.Returns(ConnectionState.Open);
        connection.BeginTransaction().Returns(transaction);

        var unitOfWork = new UnitOfWork(connection);

        unitOfWork.Begin();

        connection.DidNotReceive().Open();
        connection.Received(1).BeginTransaction();
    }

    [Fact]
    public void Commit_DisposesTransactionAndClearsReference()
    {
        var connection = Substitute.For<IDbConnection>();
        var transaction = Substitute.For<IDbTransaction>();
        connection.State.Returns(ConnectionState.Open);
        connection.BeginTransaction().Returns(transaction);

        var unitOfWork = new UnitOfWork(connection);
        unitOfWork.Begin();

        unitOfWork.Commit();

        transaction.Received(1).Commit();
        transaction.Received(1).Dispose();
        Assert.Null(unitOfWork.Transaction);
    }

    [Fact]
    public void Rollback_DisposesTransactionAndClearsReference()
    {
        var connection = Substitute.For<IDbConnection>();
        var transaction = Substitute.For<IDbTransaction>();
        connection.State.Returns(ConnectionState.Open);
        connection.BeginTransaction().Returns(transaction);

        var unitOfWork = new UnitOfWork(connection);
        unitOfWork.Begin();

        unitOfWork.Rollback();

        transaction.Received(1).Rollback();
        transaction.Received(1).Dispose();
        Assert.Null(unitOfWork.Transaction);
    }

    [Fact]
    public void Dispose_DisposesTransactionAndConnection()
    {
        var connection = Substitute.For<IDbConnection>();
        var transaction = Substitute.For<IDbTransaction>();
        connection.State.Returns(ConnectionState.Open);
        connection.BeginTransaction().Returns(transaction);

        var unitOfWork = new UnitOfWork(connection);
        unitOfWork.Begin();

        unitOfWork.Dispose();

        transaction.Received(1).Dispose();
        connection.Received(1).Dispose();
    }
}
