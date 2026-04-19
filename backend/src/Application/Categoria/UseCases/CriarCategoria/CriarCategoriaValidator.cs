using FluentValidation;

namespace Portal.Application.Categoria.UseCases.CriarCategoria;

public class CriarCategoriaValidator : AbstractValidator<CriarCategoriaRequest>
{
    public CriarCategoriaValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome da categoria é obrigatório")
            .MaximumLength(100)
            .WithMessage("Nome da categoria não pode ter mais de 100 caracteres");
    }
}
