using FluentValidation;
using Portal.Dominio;
using Portal.Features.Auth.Domain.Requests;

namespace Portal.Features.Auth.Domain.Validations {
    public class LoginRequestValidator : AbstractValidator<LoginRequest> {
        public LoginRequestValidator() {
            RuleFor(x => x.Login)
                .AplicaRegraCampoObrigatorio()
                .AplicaRegraMinimoCaracteres(3)
                .AplicaRegraMaximoCaracteres(20);

            RuleFor(x => x.Senha)
                .AplicaRegraCampoObrigatorio();

        }
    }
}
