using FluentValidation;

namespace Portal.Features.Categoria.Domain.Validations
{
    public class CategoriaRequestValidator : AbstractValidator<CategoriaRequest>
    {
        public CategoriaRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotNull().NotEmpty().WithMessage("Nome da categoria é obrigatório")
                .MinimumLength(2).WithMessage("Nome da categoria deve ter pelo menos 2 caracteres")
                .MaximumLength(100).WithMessage("Nome da categoria não pode ter mais de 100 caracteres");
        }
    }

    public class AtualizarCategoriaRequestValidator : AbstractValidator<AtualizarCategoriaRequest>
    {
        public AtualizarCategoriaRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotNull().NotEmpty().WithMessage("Id da categoria é obrigatório");

            RuleFor(x => x.Nome)
                .NotNull().NotEmpty().WithMessage("Nome da categoria é obrigatório")
                .MinimumLength(2).WithMessage("Nome da categoria deve ter pelo menos 2 caracteres")
                .MaximumLength(100).WithMessage("Nome da categoria não pode ter mais de 100 caracteres");
        }
    }
}