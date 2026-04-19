using FluentValidation;

namespace Portal.Application.Parceiro.UseCases.AtualizarParceiro
{

    public class AtualizarParceiroValidator : AbstractValidator<AtualizarParceiroRequest>
    {
        public AtualizarParceiroValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Campo Id obrigatório ou inválido");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Campo obrigatório")
                .MinimumLength(3).WithMessage("Campo deve conter no mínimo 3 caracteres")
                .MaximumLength(100).WithMessage("Campo deve conter no máximo 100 caracteres");

            RuleFor(x => x.Descricao)
                .MaximumLength(255).WithMessage("Campo deve conter no máximo 255 caracteres");
        }
    }
}
