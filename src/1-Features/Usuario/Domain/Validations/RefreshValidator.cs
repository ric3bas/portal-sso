using FluentValidation;
using Portal.Domain.Base;
using Portal.Features.Auth.Domain.Requests;

namespace Portal.Features.Usuario.Domain.Validations {
    public class RefreshValidator : AbstractValidator<RefreshRequest>
    {
        public RefreshValidator()
        {
            RuleFor(x => x.RefreshToken).AplicaRegraCampoObrigatorio();
        }
    }

    
}
