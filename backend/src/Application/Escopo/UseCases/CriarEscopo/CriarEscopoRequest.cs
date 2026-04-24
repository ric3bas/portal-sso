using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Application.Escopo.UseCases.CriarEscopo;

public class CriarEscopoRequest : BaseRequest
{
    public string Nome { get; set; } = string.Empty;

    public override bool IsValid()
    {
        var validator = new InlineValidator<CriarEscopoRequest>();
        validator.RuleFor(x => x.Nome).NotEmpty().MinimumLength(3).MaximumLength(100);
        return Validate(this, validator);
    }
}
