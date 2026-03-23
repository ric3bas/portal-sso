namespace sso.repositories;

public class DapperRepositoryTests
{
    [Fact]
    public void Query_DeveRetornarResultados()
    {
        // Arrange
        //var rows = new List<Dictionary<string, object?>>
        //{
        //    new() { ["id"] = Guid.NewGuid(), ["nome"] = "Parceiro", ["descricao"] = "desc", ["ativo"] = true }
        //};
        //var fakeReader = new FakeDbReader(rows);
        //var fakeConnection = new FakeDbConnection(fakeReader);

        //var uow = Substitute.For<IUnitOfWork>();
        //uow.Connection.Returns(fakeConnection);

        //var repo = new ParceiroRepository(uow);

        //// Act
        //var result = repo.Query("SELECT * FROM sso.parceiro", null);

        //// Assert
        //Assert.NotNull(result.Data);
        //Assert.Single(result.Data);
        //Assert.Equal("Parceiro", result.First().Nome);
    }
}
