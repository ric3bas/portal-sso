using FluentValidation;
using Portal.Dominio;
using Portal.Features.Auth.Domain.Requests;

namespace Portal.Features.Auth.Domain.Validations
{
    public class TrocarSenhaRequestValidator : AbstractValidator<TrocarSenhaRequest>
    {
        public TrocarSenhaRequestValidator()
        {
            RuleFor(x => x.Token).AplicaRegraCampoObrigatorio();
            RuleFor(x => x.NovaSenha).AplicaRegraCampoObrigatorio();
            RuleFor(x => x.ConfirmarSenha).AplicaRegraCampoObrigatorio();
            RuleFor(x => x)
                .Must(x => x.NovaSenha == x.ConfirmarSenha)
                .WithMessage("As senhas não conferem");
        }
    }
}
