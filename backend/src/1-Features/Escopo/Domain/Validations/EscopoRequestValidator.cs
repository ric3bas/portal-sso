using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Features.Escopo.Domain.Validations {
    public class EscopoRequestValidator : AbstractValidator<EscopoRequest> {
        public EscopoRequestValidator() {
            RuleFor(x => x.Nome)
                .AplicaRegraCampoObrigatorio()
                .AplicaRegraMinimoCaracteres(3)
                .AplicaRegraMaximoCaracteres(100);
        }
    }
}
