using FluentValidation;
using FluentValidation.TestHelper;
using Portal.Domain.Base;

namespace sso.global;

public class ValidacaoExtensionsTests
{
    [Fact]
    public void AplicaRegraMaximoCaracteres_WhenValueExceedsMaxLength_ShouldReturnValidationError()
    {
        var validator = new TestValidatorWithMaxLength(10);
        var model = new TestModel { Value = "12345678901" }; // 11 characters

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.Value)
            .WithErrorMessage("Limite máximo de caracteres excedido do Campo Value. Qtd enviada: 11, Máximo: 10");
    }

    [Fact]
    public void AplicaRegraMaximoCaracteres_WhenValueIsWithinMaxLength_ShouldNotReturnValidationError()
    {
        var validator = new TestValidatorWithMaxLength(10);
        var model = new TestModel { Value = "1234567890" }; // 10 characters

        var result = validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(m => m.Value);
    }

    [Fact]
    public void AplicaRegraMaximoCaracteres_WhenValueIsNull_ShouldNotReturnValidationError()
    {
        var validator = new TestValidatorWithMaxLength(10);
        var model = new TestModel { Value = null };

        var result = validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(m => m.Value);
    }

    [Fact]
    public void AplicaRegraMaximoCaracteres_WhenValueIsEmpty_ShouldNotReturnValidationError()
    {
        var validator = new TestValidatorWithMaxLength(10);
        var model = new TestModel { Value = "" };

        var result = validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(m => m.Value);
    }

    [Fact]
    public void AplicaRegraMinimoCaracteres_WhenValueIsBelowMinLength_ShouldReturnValidationError()
    {
        var validator = new TestValidatorWithMinLength(5);
        var model = new TestModel { Value = "1234" }; // 4 characters

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.Value)
            .WithErrorMessage("Limite minímo de caracteres não atingido do Campo Value. Qtd enviada: 4, Minímo: 5");
    }

    [Fact]
    public void AplicaRegraMinimoCaracteres_WhenValueIsAtMinLength_ShouldNotReturnValidationError()
    {
        var validator = new TestValidatorWithMinLength(5);
        var model = new TestModel { Value = "12345" }; // 5 characters

        var result = validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(m => m.Value);
    }

    [Fact]
    public void AplicaRegraMinimoCaracteres_WhenValueIsNull_ShouldNotReturnValidationError()
    {
        var validator = new TestValidatorWithMinLength(5);
        var model = new TestModel { Value = null };

        var result = validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(m => m.Value);
    }

    [Fact]
    public void AplicaRegraMinimoCaracteres_WhenValueIsEmpty_ShouldReturnValidationError()
    {
        var validator = new TestValidatorWithMinLength(5);
        var model = new TestModel { Value = "" };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.Value)
            .WithErrorMessage("Limite minímo de caracteres não atingido do Campo Value. Qtd enviada: 0, Minímo: 5");
    }

    [Fact]
    public void AplicaRegraCampoObrigatorio_WhenValueIsEmpty_ShouldReturnValidationError()
    {
        var validator = new TestValidatorWithRequiredField();
        var model = new TestModel { Value = "" };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.Value)
            .WithErrorMessage("Campo Value obrigatório")
            .WithErrorCode("0001");
    }

    [Fact]
    public void AplicaRegraCampoObrigatorio_WhenValueIsNull_ShouldReturnValidationError()
    {
        var validator = new TestValidatorWithRequiredField();
        var model = new TestModel { Value = null };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.Value)
            .WithErrorMessage("Campo Value obrigatório")
            .WithErrorCode("0001");
    }

    [Fact]
    public void AplicaRegraCampoObrigatorio_WhenValueIsNotEmpty_ShouldNotReturnValidationError()
    {
        var validator = new TestValidatorWithRequiredField();
        var model = new TestModel { Value = "test" };

        var result = validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(m => m.Value);
    }

    [Fact]
    public void AplicaRegraCampoObrigatorio_WhenConditionalIsTrue_ShouldApplyValidation()
    {
        var validator = new TestValidatorWithConditionalRequiredField(x => true);
        var model = new TestModel { Value = "" };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.Value)
            .WithErrorMessage("Campo Value obrigatório")
            .WithErrorCode("0001");
    }

    [Fact]
    public void AplicaRegraCampoObrigatorio_WhenConditionalIsFalse_ShouldNotApplyValidation()
    {
        var validator = new TestValidatorWithConditionalRequiredField(x => false);
        var model = new TestModel { Value = "" };

        var result = validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(m => m.Value);
    }

    [Fact]
    public void AplicaRegraCampoObrigatorio_WhenConditionalIsNull_ShouldApplyValidation()
    {
        var validator = new TestValidatorWithConditionalRequiredField(null);
        var model = new TestModel { Value = "" };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.Value)
            .WithErrorMessage("Campo Value obrigatório")
            .WithErrorCode("0001");
    }

    [Fact]
    public void AplicaRegraCampoObrigatorio_WithIntProperty_WhenValueIsDefault_ShouldReturnValidationError()
    {
        var validator = new TestValidatorWithRequiredIntField();
        var model = new TestModelWithInt { IntValue = 0 };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(m => m.IntValue)
            .WithErrorMessage("Campo Int Value obrigatório")
            .WithErrorCode("0001");
    }

    [Fact]
    public void AplicaRegraCampoObrigatorio_WithIntProperty_WhenValueIsNotDefault_ShouldNotReturnValidationError()
    {
        var validator = new TestValidatorWithRequiredIntField();
        var model = new TestModelWithInt { IntValue = 5 };

        var result = validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(m => m.IntValue);
    }

    private class TestValidatorWithMaxLength : AbstractValidator<TestModel>
    {
        public TestValidatorWithMaxLength(int maxLength)
        {
            RuleFor(x => x.Value).AplicaRegraMaximoCaracteres(maxLength);
        }
    }

    private class TestValidatorWithMinLength : AbstractValidator<TestModel>
    {
        public TestValidatorWithMinLength(int minLength)
        {
            RuleFor(x => x.Value).AplicaRegraMinimoCaracteres(minLength);
        }
    }

    private class TestValidatorWithRequiredField : AbstractValidator<TestModel>
    {
        public TestValidatorWithRequiredField()
        {
            RuleFor(x => x.Value).AplicaRegraCampoObrigatorio();
        }
    }

    private class TestValidatorWithConditionalRequiredField : AbstractValidator<TestModel>
    {
        public TestValidatorWithConditionalRequiredField(Func<TestModel, bool>? conditional)
        {
            RuleFor(x => x.Value).AplicaRegraCampoObrigatorio(conditional);
        }
    }

    private class TestValidatorWithRequiredIntField : AbstractValidator<TestModelWithInt>
    {
        public TestValidatorWithRequiredIntField()
        {
            RuleFor(x => x.IntValue).AplicaRegraCampoObrigatorio();
        }
    }

    private class TestModel
    {
        public string? Value { get; set; }
    }

    private class TestModelWithInt
    {
        public int IntValue { get; set; }
    }
}
