using Portal.Domain.Exceptions;

namespace sso.global;

public class BusinessExceptionTests
{
    [Fact]
    public void Constructor_WithListOfErrors_SetsErrorsPropertyAndMessage()
    {
        var errors = new List<string> { "Error 1", "Error 2", "Error 3" };

        var exception = new BusinessException(errors);

        Assert.NotNull(exception);
        Assert.Equal("Erro de negócio", exception.Message);
        Assert.Same(errors, exception.Errors);
    }

    [Fact]
    public void Constructor_WithEmptyList_SetsEmptyErrorsList()
    {
        var errors = new List<string>();

        var exception = new BusinessException(errors);

        Assert.NotNull(exception);
        Assert.Equal("Erro de negócio", exception.Message);
        Assert.Same(errors, exception.Errors);
        Assert.Empty(exception.Errors);
    }

    [Fact]
    public void Constructor_WithSingleError_SetsSingleErrorInList()
    {
        var errors = new List<string> { "Single error" };

        var exception = new BusinessException(errors);

        Assert.NotNull(exception);
        Assert.Equal("Erro de negócio", exception.Message);
        Assert.Same(errors, exception.Errors);
        Assert.Single(exception.Errors);
        Assert.Equal("Single error", exception.Errors[0]);
    }

    [Fact]
    public void Constructor_WithListOfErrors_InheritsFromException()
    {
        var errors = new List<string> { "Error 1" };

        var exception = new BusinessException(errors);

        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact]
    public void Constructor_WithListOfErrors_CanBeThrown()
    {
        var errors = new List<string> { "Test error" };
        Action act = () => throw new BusinessException(errors);

        var exception = Assert.Throws<BusinessException>(act);
        Assert.Equal("Erro de negócio", exception.Message);
        Assert.Same(errors, exception.Errors);
    }
}
