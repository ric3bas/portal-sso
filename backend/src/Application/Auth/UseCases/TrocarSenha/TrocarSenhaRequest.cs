using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Application.Auth.UseCases.TrocarSenha;

public class TrocarSenhaRequest : BaseRequest
{
    public string Token { get; set; } = string.Empty;
    public string NovaSenha { get; set; } = string.Empty;
    public string ConfirmarSenha { get; set; } = string.Empty;

    public override bool IsValid()
    {
        var validator = new InlineValidator<TrocarSenhaRequest>();
        validator.RuleFor(x => x.Token).AplicaRegraCampoObrigatorio();
        validator.RuleFor(x => x.NovaSenha).AplicaRegraCampoObrigatorio();
        validator.RuleFor(x => x.ConfirmarSenha).AplicaRegraCampoObrigatorio();
        validator.RuleFor(x => x).Must(x => x.NovaSenha == x.ConfirmarSenha).WithMessage("As senhas nÃ£o conferem");
        return Validate(this, validator);
    }
}
