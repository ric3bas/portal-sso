using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Application.Auth.UseCases.Logout
{
    public class LogoutValidator : AbstractValidator<LogoutRequest>
    {
        public LogoutValidator()
        {
            RuleFor(x => x.RefreshToken).AplicaRegraCampoObrigatorio();

        }
    }
}
