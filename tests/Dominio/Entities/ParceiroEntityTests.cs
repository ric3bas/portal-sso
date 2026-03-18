using Portal.Domain.Entities;

namespace sso.global;

public class ParceiroEntityTests
{
    [Fact]
    public void ToResponse_WithAllPropertiesSet_ReturnsCorrectResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var nome = "Test Parceiro";
        var descricao = "Test Description";
        var ativo = true;
        
        var entity = new ParceiroEntity
        {
            Id = id,
            Nome = nome,
            Descricao = descricao,
            Ativo = ativo
        };

        // Act
        var response = entity.ToResponse();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Equal(nome, response.Nome);
        Assert.Equal(descricao, response.Descricao);
        Assert.Equal(ativo, response.Ativo);
    }

    [Fact]
    public void ToResponse_WithNullNome_ReturnsResponseWithNullNome()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new ParceiroEntity
        {
            Id = id,
            Nome = null,
            Descricao = "Test Description",
            Ativo = true
        };

        // Act
        var response = entity.ToResponse();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Null(response.Nome);
        Assert.Equal("Test Description", response.Descricao);
        Assert.True(response.Ativo);
    }

    [Fact]
    public void ToResponse_WithNullDescricao_ReturnsResponseWithNullDescricao()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new ParceiroEntity
        {
            Id = id,
            Nome = "Test Parceiro",
            Descricao = null,
            Ativo = true
        };

        // Act
        var response = entity.ToResponse();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Equal("Test Parceiro", response.Nome);
        Assert.Null(response.Descricao);
        Assert.True(response.Ativo);
    }

    [Fact]
    public void ToResponse_WithAtivoFalse_ReturnsResponseWithAtivoFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new ParceiroEntity
        {
            Id = id,
            Nome = "Test Parceiro",
            Descricao = "Test Description",
            Ativo = false
        };

        // Act
        var response = entity.ToResponse();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Equal("Test Parceiro", response.Nome);
        Assert.Equal("Test Description", response.Descricao);
        Assert.False(response.Ativo);
    }

    [Fact]
    public void ToResponse_WithEmptyGuid_ReturnsResponseWithEmptyGuid()
    {
        // Arrange
        var entity = new ParceiroEntity
        {
            Id = Guid.Empty,
            Nome = "Test Parceiro",
            Descricao = "Test Description",
            Ativo = true
        };

        // Act
        var response = entity.ToResponse();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(Guid.Empty, response.Id);
        Assert.Equal("Test Parceiro", response.Nome);
        Assert.Equal("Test Description", response.Descricao);
        Assert.True(response.Ativo);
    }

    [Fact]
    public void ToResponse_WithAllNullablePropertiesNull_ReturnsResponseWithNulls()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new ParceiroEntity
        {
            Id = id,
            Nome = null,
            Descricao = null,
            Ativo = false
        };

        // Act
        var response = entity.ToResponse();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Null(response.Nome);
        Assert.Null(response.Descricao);
        Assert.False(response.Ativo);
    }

    [Fact]
    public void ToResponse_WithDefaultValues_ReturnsResponseWithDefaults()
    {
        // Arrange
        var entity = new ParceiroEntity();

        // Act
        var response = entity.ToResponse();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(Guid.Empty, response.Id);
        Assert.Null(response.Nome);
        Assert.Null(response.Descricao);
        Assert.True(response.Ativo);
    }

    [Fact]
    public void ToResponse_WithEmptyStrings_ReturnsResponseWithEmptyStrings()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new ParceiroEntity
        {
            Id = id,
            Nome = string.Empty,
            Descricao = string.Empty,
            Ativo = true
        };

        // Act
        var response = entity.ToResponse();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Equal(string.Empty, response.Nome);
        Assert.Equal(string.Empty, response.Descricao);
        Assert.True(response.Ativo);
    }
}
