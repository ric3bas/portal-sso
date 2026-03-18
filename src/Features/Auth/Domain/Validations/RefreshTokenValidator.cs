using FluentValidation;
using Portal.Dominio;
using Portal.Features.Auth.Domain.Requests;

namespace Portal.Features.Auth.Domain.Validations {
    public class RefreshTokenValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenValidator()
        {
            RuleFor(x => x.RefreshToken).AplicaRegraCampoObrigatorio();
        }
    }
}
