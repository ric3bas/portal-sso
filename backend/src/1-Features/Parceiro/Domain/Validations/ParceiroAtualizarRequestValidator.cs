using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Features.Parceiro.Domain.Validations {
    public class ParceiroAtualizarRequestValidator : AbstractValidator<AtualizarParceiroRequest>
    {
        public ParceiroAtualizarRequestValidator()
        {
            RuleFor(x => x.Nome)
                .AplicaRegraCampoObrigatorio()
                .AplicaRegraMinimoCaracteres(3)
                .AplicaRegraMaximoCaracteres(100);

            RuleFor(x => x.Descricao)
                .AplicaRegraMaximoCaracteres(255);

            RuleFor(x => x.Id)
              .Must(id => Guid.TryParse(id, out var guid) && guid != Guid.Empty)
              .WithMessage("Campo Id obrigatório ou inválido");
        }
    }
}
