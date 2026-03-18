using FluentValidation;
using Portal.Dominio;
using Portal.Features.Auth.Domain.Requests;

namespace Portal.Features.Auth.Domain.Validations {
    public class RecuperarSenhaValidator: AbstractValidator<RecuperarSenhaRequest>
    {
        public RecuperarSenhaValidator()
        {
            RuleFor(x => x.Login).AplicaRegraCampoObrigatorio();
        }
    }   
}
