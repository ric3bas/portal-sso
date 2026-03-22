using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Features.Usuario.Domain.Validations {
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest> {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Nome)
               .AplicaRegraCampoObrigatorio()
               .AplicaRegraMinimoCaracteres(3)
               .AplicaRegraMaximoCaracteres(100);

            RuleFor(x => x.Login)
                .AplicaRegraCampoObrigatorio()
                .AplicaRegraMinimoCaracteres(3)
                .AplicaRegraMaximoCaracteres(50);

            RuleFor(x => x.Senha)
                .AplicaRegraCampoObrigatorio()
                .AplicaRegraMinimoCaracteres(6)
                .AplicaRegraMaximoCaracteres(100)
                .Matches("[A-Z]").WithMessage("Campo Senha deve conter ao menos uma letra maiúscula")
                .Matches("[0-9]").WithMessage("Campo Senha deve conter ao menos um número");
        }
    }
}
