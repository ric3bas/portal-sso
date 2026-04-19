using FluentValidation;
using Portal.Application.Cliente.Common;

namespace Portal.Application.Cliente.Validators;

public class EnderecoRequestValidator : AbstractValidator<EnderecoRequest>
{
    public EnderecoRequestValidator()
    {
        RuleFor(x => x.Logradouro)
            .NotEmpty().WithMessage("Logradouro é obrigatório")
            .MaximumLength(200).WithMessage("Logradouro não pode ter mais de 200 caracteres");

        RuleFor(x => x.Cidade)
            .NotEmpty().WithMessage("Cidade é obrigatória")
            .MaximumLength(100).WithMessage("Cidade não pode ter mais de 100 caracteres");

        RuleFor(x => x.Estado)
            .NotEmpty().WithMessage("Estado é obrigatório")
            .Length(2).WithMessage("Estado deve ter 2 caracteres");

        RuleFor(x => x.Numero)
            .NotEmpty().WithMessage("Número é obrigatório")
            .MaximumLength(10).WithMessage("Número não pode ter mais de 10 caracteres");

        RuleFor(x => x.Complemento)
            .MaximumLength(100).WithMessage("Complemento não pode ter mais de 100 caracteres");
    }
}
