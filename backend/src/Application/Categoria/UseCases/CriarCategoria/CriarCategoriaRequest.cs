using FluentValidation;
using Portal.Application.Categoria.UseCases.AtualizarCategoria;
using Portal.Domain.Base;

namespace Portal.Application.Categoria.UseCases.CriarCategoria;

public class CriarCategoriaRequest : BaseRequest
{
    public string Nome { get; set; } = string.Empty;

    public override bool IsValid()
    {
        var validator = new InlineValidator<CriarCategoriaRequest>();
        validator.RuleFor(x => x.Nome).NotEmpty().WithMessage("Nome da categoria não obrigatório").MaximumLength(100).WithMessage("Nome da categoria não pode ter mais de 100 caracteres");

        return Validate(this, validator);
    }
}
