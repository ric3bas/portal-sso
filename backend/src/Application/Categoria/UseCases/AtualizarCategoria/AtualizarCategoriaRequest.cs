using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Application.Categoria.UseCases.AtualizarCategoria;

public class AtualizarCategoriaRequest : BaseRequest
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool? Ativo { get; set; }

    public override bool IsValid()
    {
       var validator = new InlineValidator<AtualizarCategoriaRequest>();
       validator.RuleFor(x => x.Id).NotEmpty().WithMessage("Campo Id obrigatório ou inválido");
       validator.RuleFor(x => x.Nome).NotEmpty().WithMessage("Nome da categoria Ã© obrigatÃ³rio").MaximumLength(100).WithMessage("Nome da categoria nÃ£o pode ter mais de 100 caracteres");

       return Validate(this, validator);
    }
}
