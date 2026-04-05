using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Features.Perfil.Domain.Validations {
    public class PerfilRequestValidator : AbstractValidator<PerfilRequest> {
        public PerfilRequestValidator() {
            RuleFor(x => x.Nome)
                .AplicaRegraCampoObrigatorio()
                .AplicaRegraMinimoCaracteres(3)
                .AplicaRegraMaximoCaracteres(100);
        }
    }
}
