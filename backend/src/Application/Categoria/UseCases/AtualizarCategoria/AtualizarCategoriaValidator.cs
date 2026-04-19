using FluentValidation;

namespace Portal.Application.Categoria.UseCases.AtualizarCategoria;

public class AtualizarCategoriaValidator : AbstractValidator<AtualizarCategoriaRequest>
{
    public AtualizarCategoriaValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID da categoria é obrigatório");

        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome da categoria é obrigatório")
            .MaximumLength(100)
            .WithMessage("Nome da categoria não pode ter mais de 100 caracteres");
    }
}
