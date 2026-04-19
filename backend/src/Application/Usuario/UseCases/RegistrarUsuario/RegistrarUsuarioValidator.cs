using FluentValidation;

namespace Portal.Application.Usuario.UseCases.RegistrarUsuario
{
    public class RegistrarUsuarioValidator : AbstractValidator<RegistrarUsuarioRequest>
    {
        public RegistrarUsuarioValidator()
        {
            RuleFor(x => x.Nome)
               .NotEmpty().WithMessage("Campo obrigatório")
               .MinimumLength(3).WithMessage("Campo deve conter no mínimo 3 caracteres")
               .MaximumLength(100).WithMessage("Campo deve conter no máximo 100 caracteres");

            RuleFor(x => x.Login)
                .NotEmpty().WithMessage("Campo obrigatório")
                .MinimumLength(3).WithMessage("Campo deve conter no mínimo 3 caracteres")
                .MaximumLength(50).WithMessage("Campo deve conter no máximo 50 caracteres");

            RuleFor(x => x.Senha)
                .NotEmpty().WithMessage("Campo obrigatório")
                .MinimumLength(6).WithMessage("Campo deve conter no mínimo 6 caracteres")
                .MaximumLength(20).WithMessage("Campo deve conter no máximo 20 caracteres")
                .Matches("[A-Z]").WithMessage("Campo Senha deve conter ao menos uma letra maiúscula")
                .Matches("[0-9]").WithMessage("Campo Senha deve conter ao menos um número");
        }
    }
}
