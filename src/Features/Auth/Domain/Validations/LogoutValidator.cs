using FluentValidation;
using Portal.Dominio;
using Portal.Features.Auth.Domain.Requests;

namespace Portal.Features.Auth.Domain.Validations {
    public class LogoutValidator : AbstractValidator<LogoutRequest>
    {
        public LogoutValidator()
        {
            RuleFor(x => x.RefreshToken)
                .AplicaRegraCampoObrigatorio();

        }
    }

    
}
