using FluentValidation;

namespace Portal.Application.Perfil.UseCases.CriarPerfil
{
    public class CriarPerfilValidator : AbstractValidator<CriarPerfilRequest>
    {
        public CriarPerfilValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Campo obrigatório")
                .MinimumLength(3).WithMessage("Campo deve conter no mínimo 3 caracteres")
                .MaximumLength(100).WithMessage("Campo deve conter no máximo 100 caracteres");
        }
    }
}
