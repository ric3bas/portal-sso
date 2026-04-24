using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Application.Auth.UseCases.RecuperarSenha;

public class RecuperarSenhaRequest : BaseRequest
{
    public string Login { get; set; } = string.Empty;

    public override bool IsValid()
    {
        var validator = new InlineValidator<RecuperarSenhaRequest>();

        validator.RuleFor(x => x.Login).AplicaRegraCampoObrigatorio();

        return Validate(this, validator);
    }
}

