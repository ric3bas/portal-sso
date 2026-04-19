using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Application.Auth.UseCases.RecuperarSenha
{
    public class RecuperarSenhaValidator : AbstractValidator<RecuperarSenhaRequest>
    {
        public RecuperarSenhaValidator()
        {
            RuleFor(x => x.Login).AplicaRegraCampoObrigatorio();
        }
    }
}
