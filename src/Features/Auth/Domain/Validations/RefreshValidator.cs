using FluentValidation;
using Portal.Dominio;
using Portal.Features.Auth.Domain.Requests;

namespace Portal.Features.Auth.Domain.Validations {
    public class RefreshValidator : AbstractValidator<RefreshRequest>
    {
        public RefreshValidator()
        {
            RuleFor(x => x.RefreshToken).AplicaRegraCampoObrigatorio();
        }
    }

    
}
