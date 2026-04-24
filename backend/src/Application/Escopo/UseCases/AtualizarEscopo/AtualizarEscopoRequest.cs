using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Application.Escopo.UseCases.AtualizarEscopo;

public class AtualizarEscopoRequest : BaseRequest
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    public override bool IsValid()
    {
        var validator = new InlineValidator<AtualizarEscopoRequest>();
        validator.RuleFor(x => x.Id).GreaterThan(0);
        validator.RuleFor(x => x.Nome).NotEmpty().MinimumLength(3).MaximumLength(100);
        return Validate(this, validator);
    }
}
