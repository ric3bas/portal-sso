using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Application.Perfil.UseCases.CriarPerfil;

public class CriarPerfilRequest : BaseRequest
{
    public string Nome { get; set; } = string.Empty;

    public override bool IsValid()
    {
        var validator = new InlineValidator<CriarPerfilRequest>();
        validator.RuleFor(x => x.Nome).NotEmpty().MinimumLength(3).MaximumLength(100);
        return Validate(this, validator);
    }
}
