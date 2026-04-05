using FluentValidation;
using Portal.Domain.Base;
using Portal.Features.Auth.Domain.Requests;

namespace Portal.Features.Usuario.Domain.Validations {
    public class LogoutValidator : AbstractValidator<LogoutRequest>
    {
        public LogoutValidator()
        {
            RuleFor(x => x.RefreshToken)
                .AplicaRegraCampoObrigatorio();

        }
    }

    
}
