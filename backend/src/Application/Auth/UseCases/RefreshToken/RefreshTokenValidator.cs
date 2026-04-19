using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Application.Auth.UseCases.RefreshToken
{
    public class RefreshTokenValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenValidator()
        {
            RuleFor(x => x.RefreshToken).AplicaRegraCampoObrigatorio();
        }
    }
}
