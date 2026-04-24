using FluentValidation;
using Portal.Application.Categoria.UseCases.CriarCategoria;
using Portal.Domain.Base;

namespace Portal.Application.Categoria.UseCases.InativarCategoria;

public class InativarCategoriaRequest : BaseRequest
{
    public Guid Id { get; set; }

    public override bool IsValid()
    {
        var validator = new InlineValidator<InativarCategoriaRequest>();
        validator.RuleFor(x => x.Id).NotEmpty().WithMessage("Campo Id obrigatório ou inválido");

        return Validate(this, validator);
    }
}
