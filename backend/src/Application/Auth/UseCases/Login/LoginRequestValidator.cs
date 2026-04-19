using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Application.Auth.UseCases.Login
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Login)
                .AplicaRegraCampoObrigatorio()
                .AplicaRegraMinimoCaracteres(3)
                .AplicaRegraMaximoCaracteres(50);

            RuleFor(x => x.Senha)
                .AplicaRegraCampoObrigatorio();

        }
    }
}
