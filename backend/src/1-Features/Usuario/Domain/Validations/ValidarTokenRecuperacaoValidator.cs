using FluentValidation;
using Portal.Domain.Base;
using Portal.Features.Auth.Domain.Requests;

namespace Portal.Features.Usuario.Domain.Validations {
    public class ValidarTokenRecuperacaoValidator : AbstractValidator<ValidarTokenRecuperacaoRequest> {
        public ValidarTokenRecuperacaoValidator() {
            RuleFor(x => x.Token).AplicaRegraCampoObrigatorio();
        }
    }
}
