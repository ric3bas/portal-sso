using FluentValidation;
using Portal.Application.Cliente.Common;

namespace Portal.Application.Cliente.Validators;

public class TelefoneRequestValidator : AbstractValidator<TelefoneRequest>
{
    public TelefoneRequestValidator()
    {
        RuleFor(x => x.Ddd)
            .NotEmpty().WithMessage("DDD é obrigatório")
            .Length(2).WithMessage("DDD deve ter 2 dígitos")
            .Matches(@"^\d{2}$").WithMessage("DDD deve conter apenas números");

        RuleFor(x => x.Numero)
            .NotEmpty().WithMessage("Número do telefone é obrigatório")
            .Matches(@"^[0-9]{8,9}$").WithMessage("Número do telefone deve ter 8 ou 9 dígitos");
    }
}
