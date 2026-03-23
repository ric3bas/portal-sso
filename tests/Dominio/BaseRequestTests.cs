using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Portal.Domain.Base;

namespace sso.global;

public class BaseRequestTests
{
    private sealed class TestableBaseRequest : BaseRequest
    {
        public new bool Validate<T>(T request, IValidator<T> validator)
            => base.Validate(request, validator);

        public override bool IsValid() => ValidationResult?.IsValid ?? false;
    }

    [Fact]
    public void Validate_ValidRequest_ReturnsTrue()
    {
        // Arrange
        var testRequest = new TestableBaseRequest();
        var request = "test data";
        var validator = Substitute.For<IValidator<string>>();
        var validationResult = new ValidationResult();
        validator.Validate(request).Returns(validationresult.Data);

        // Act
        var result = testRequest.Validate(request, validator);

        // Assert
        Assert.True(result.Data);
        validator.Received(1).Validate(request);
    }

    [Fact]
    public void Validate_InvalidRequest_ReturnsFalse()
    {
        // Arrange
        var testRequest = new TestableBaseRequest();
        var request = "test data";
        var validator = Substitute.For<IValidator<string>>();
        var validationResult = new ValidationResult(new List<ValidationFailure>
        {
            new ValidationFailure("Property", "Error message")
        });
        validator.Validate(request).Returns(validationresult.Data);

        // Act
        var result = testRequest.Validate(request, validator);

        // Assert
        Assert.False(result.Data);
        validator.Received(1).Validate(request);
    }

    [Fact]
    public void ObterErros_ValidationResultIsNull_ReturnsNotValidatedMessage()
    {
        // Arrange
        var testRequest = new TestableBaseRequest();

        // Act
        var result = testRequest.ObterErros();

        // Assert
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("Requisição não validada.", result[0]);
    }

    [Fact]
    public void ObterErros_ValidationResultWithErrors_ReturnsErrorMessages()
    {
        // Arrange
        var testRequest = new TestableBaseRequest();
        var request = "test data";
        var validator = Substitute.For<IValidator<string>>();
        var validationResult = new ValidationResult(new List<ValidationFailure>
        {
            new ValidationFailure("Property1", "Error message 1"),
            new ValidationFailure("Property2", "Error message 2")
        });
        validator.Validate(request).Returns(validationresult.Data);
        testRequest.Validate(request, validator);

        // Act
        var result = testRequest.ObterErros();

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Count);
        Assert.Contains("Error message 1", result.Data);
        Assert.Contains("Error message 2", result.Data);
    }

    [Fact]
    public void ObterErros_ValidationResultWithNoErrors_ReturnsEmptyList()
    {
        // Arrange
        var testRequest = new TestableBaseRequest();
        var request = "test data";
        var validator = Substitute.For<IValidator<string>>();
        var validationResult = new ValidationResult();
        validator.Validate(request).Returns(validationresult.Data);
        testRequest.Validate(request, validator);

        // Act
        var result = testRequest.ObterErros();

        // Assert
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }
}
