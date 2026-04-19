using FluentValidation;

namespace Portal.Application.Parceiro.UseCases.CriarParceiro
{
    public class CriarParceiroValidator : AbstractValidator<CriarParceiroRequest>
    {
        public CriarParceiroValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Campo obrigatório")
                .MinimumLength(3).WithMessage("Campo deve conter no mínimo 3 caracteres")
                .MaximumLength(100).WithMessage("Campo deve conter no máximo 100 caracteres");

            RuleFor(x => x.Descricao)
                .MaximumLength(255).WithMessage("Campo deve conter no máximo 255 caracteres");
        }
    }
}
