using FluentValidation;

namespace Portal.Features.Parceiro.Domain.Validations {
    public class ParceiroIdRequestValidator : AbstractValidator<string>
    {
        public ParceiroIdRequestValidator()
        {
            RuleFor(x => x)
                .Must(id => Guid.TryParse(id, out var guid) && guid != Guid.Empty)
                .WithMessage("Campo Id obrigatório ou inválido");
        }
    }
}