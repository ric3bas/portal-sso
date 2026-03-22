using FluentValidation;
using Portal.Domain.Base;
using Portal.Features.Auth.Domain.Requests;

namespace Portal.Features.Usuario.Domain.Validations {
    public class RefreshTokenValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenValidator()
        {
            RuleFor(x => x.RefreshToken).AplicaRegraCampoObrigatorio();
        }
    }
}
