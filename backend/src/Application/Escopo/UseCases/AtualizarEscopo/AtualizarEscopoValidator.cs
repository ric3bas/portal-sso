using FluentValidation;

namespace Portal.Application.Escopo.UseCases.AtualizarEscopo
{
    public class AtualizarEscopoValidator : AbstractValidator<AtualizarEscopoRequest>
    {
        public AtualizarEscopoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Campo Id obrigatório ou inválido");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Campo obrigatório")
                .MinimumLength(3).WithMessage("Campo deve conter no mínimo 3 caracteres")
                .MaximumLength(100).WithMessage("Campo deve conter no máximo 100 caracteres");
        }
    }
}
