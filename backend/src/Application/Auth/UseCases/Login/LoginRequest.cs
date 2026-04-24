using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Application.Auth.UseCases.Login;

public class LoginRequest : BaseRequest
{
    public string Login { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string IpUsuario { get; set; } = string.Empty;

    public override bool IsValid()
    {
        var validator = new InlineValidator<LoginRequest>();
        validator.RuleFor(x => x.Login).AplicaRegraCampoObrigatorio().AplicaRegraMinimoCaracteres(3).AplicaRegraMaximoCaracteres(50);
        validator.RuleFor(x => x.Senha).AplicaRegraCampoObrigatorio();
        return Validate(this, validator);
    }
}
