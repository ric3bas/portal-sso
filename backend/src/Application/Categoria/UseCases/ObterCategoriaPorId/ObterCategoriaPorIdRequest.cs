using FluentValidation;
using Portal.Application.Categoria.UseCases.InativarCategoria;
using Portal.Domain.Base;

namespace Portal.Application.Categoria.UseCases.ObterCategoriaPorId;

public class ObterCategoriaPorIdRequest : BaseRequest
{
    public Guid Id { get; set; }

    public override bool IsValid()
    {
        var validator = new InlineValidator<ObterCategoriaPorIdRequest>();
        validator.RuleFor(x => x.Id).NotEmpty().WithMessage("Campo Id obrigatório ou inválido");

        return Validate(this, validator);
    }
}
