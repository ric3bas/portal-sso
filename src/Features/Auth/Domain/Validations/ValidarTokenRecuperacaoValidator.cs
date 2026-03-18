using FluentValidation;
using Portal.Dominio;
using Portal.Features.Auth.Domain.Requests;

namespace Portal.Features.Auth.Domain.Validations {
    public class ValidarTokenRecuperacaoValidator : AbstractValidator<ValidarTokenRecuperacaoRequest> {
        public ValidarTokenRecuperacaoValidator() {
            RuleFor(x => x.Token).AplicaRegraCampoObrigatorio();
        }
    }
}
