using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Application.Parceiro.UseCases.CriarParceiro;

public class CriarParceiroRequest : BaseRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? CorPrimaria { get; set; }
    public string? CorSecundaria { get; set; }

    public override bool IsValid()
    {
        var validator = new InlineValidator<CriarParceiroRequest>();
        validator.RuleFor(x => x.Nome).NotEmpty().MinimumLength(3).MaximumLength(100);
        validator.RuleFor(x => x.Descricao).MaximumLength(255);
        validator.RuleFor(x => x.CorPrimaria)
            .MaximumLength(7)
            .Matches("^#[0-9A-Fa-f]{6}$")
            .When(x => !string.IsNullOrWhiteSpace(x.CorPrimaria));
        validator.RuleFor(x => x.CorSecundaria)
            .MaximumLength(7)
            .Matches("^#[0-9A-Fa-f]{6}$")
            .When(x => !string.IsNullOrWhiteSpace(x.CorSecundaria));
        return Validate(this, validator);
    }
}
