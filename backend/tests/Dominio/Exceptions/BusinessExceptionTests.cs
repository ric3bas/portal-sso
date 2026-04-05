using Portal.Domain.Exceptions;

namespace sso.global;

public class BusinessExceptionTests
{
    [Fact]
    public void Constructor_WithListOfErrors_SetsErrorsPropertyAndMessage()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2", "Error 3" };

        // Act
        var exception = new BusinessException(errors);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal("Erro de negócio", exception.Message);
        Assert.Same(errors, exception.Errors);
    }

    [Fact]
    public void Constructor_WithEmptyList_SetsEmptyErrorsList()
    {
        // Arrange
        var errors = new List<string>();

        // Act
        var exception = new BusinessException(errors);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal("Erro de negócio", exception.Message);
        Assert.Same(errors, exception.Errors);
        Assert.Empty(exception.Errors);
    }

    [Fact]
    public void Constructor_WithSingleError_SetsSingleErrorInList()
    {
        // Arrange
        var errors = new List<string> { "Single error" };

        // Act
        var exception = new BusinessException(errors);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal("Erro de negócio", exception.Message);
        Assert.Same(errors, exception.Errors);
        Assert.Single(exception.Errors);
        Assert.Equal("Single error", exception.Errors[0]);
    }

    [Fact]
    public void Constructor_WithListOfErrors_InheritsFromException()
    {
        // Arrange
        var errors = new List<string> { "Error 1" };

        // Act
        var exception = new BusinessException(errors);

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact]
    public void Constructor_WithListOfErrors_CanBeThrown()
    {
        // Arrange
        var errors = new List<string> { "Test error" };
        Action act = () => throw new BusinessException(errors);

        // Act & Assert
        var exception = Assert.Throws<BusinessException>(act);
        Assert.Equal("Erro de negócio", exception.Message);
        Assert.Same(errors, exception.Errors);
    }
}
