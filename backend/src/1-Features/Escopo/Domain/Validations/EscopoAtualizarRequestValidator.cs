using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Features.Escopo.Domain.Validations {
    public class EscopoAtualizarRequestValidator : AbstractValidator<AtualizarEscopoRequest>
    {
        public EscopoAtualizarRequestValidator()
        {
            RuleFor(x => x.Nome)
                .AplicaRegraCampoObrigatorio()
                .AplicaRegraMinimoCaracteres(3)
                .AplicaRegraMaximoCaracteres(100);

            RuleFor(x => x.Id)
              .GreaterThan(0)
              .WithMessage("Campo Id obrigatório ou inválido");
        }
    }
}
