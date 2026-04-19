using FluentValidation;

namespace Portal.Domain.Base
{
    public class StringValidator : AbstractValidator<string>
    {
        public StringValidator()
        {
            RuleFor(x => x)
                .NotEmpty().WithMessage("Campo tipo string obrigatório")
                .MinimumLength(3).WithMessage("Campo deve ter no mínimo 3 caracteres")
                .MaximumLength(100).WithMessage("Campo deve ter no máximo 100 caracteres");
        }
    }
}
