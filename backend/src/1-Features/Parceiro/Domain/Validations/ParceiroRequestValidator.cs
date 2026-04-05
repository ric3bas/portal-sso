using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Features.Parceiro.Domain.Validations {
    public class ParceiroRequestValidator : AbstractValidator<ParceiroRequest> {
        public ParceiroRequestValidator() {
            RuleFor(x => x.Nome)
                .AplicaRegraCampoObrigatorio()
                .AplicaRegraMinimoCaracteres(3)
                .AplicaRegraMaximoCaracteres(100);

            RuleFor(x => x.Descricao)
                .AplicaRegraMaximoCaracteres(255);
        }
    }
}
